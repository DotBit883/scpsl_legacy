using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponManager : NetworkBehaviour
{
	[Serializable]
	public class Weapon
	{
		public int itemID;

		public Animator model;

		public int magSize = 15;

		public AnimationCurve damageOverDistance;

		public float bodyDamageMultipiler = 1f;

		public float headDamageMultipiler = 2f;

		public float legDamageMultipiler = 0.5f;

		public bool useFullauto;

		public float fireRate = 3f;

		public AudioClip shootAudio;

		public AudioClip reloadAudio;

		public AudioClip ammoAudio;

		public string shootAudio_aac;

		public string reloadAudio_aac;

		public float cooldown;

		public float reloadingTime = 5f;

		public ParticleSystem[] shotGFX;

		public float zoomFOV;

		public float recoilScale;

		public float zoomRecoilCompression;

		public float zoomMouseSensitivityMultiplier;

		public float zoomSlowdown;

		public GameObject[] holes;

		public GameObject weaponCamera;

		public int ammoType;

		public float hitmarkerResizer = 1f;

		[HideInInspector]
		public int arrayID;
	}

	public Camera plyCam;

	public GameObject unfocusedWeaponCamera;

	public GameObject recoilCam;

	public float normalFov;

	public LayerMask mask;

	public string[] holeTags;

	public float sneakSpeed;

	public static bool friendlyFire;

	[SyncVar]
	private bool syncFF;

	private Inventory inv;

	private FirstPersonController fpc;

	private AmmoBox ammoBox;

	private InventoryDisplay invDis;

	private CharacterClassManager ccm;

	public int curWeapon;

	private Item curItem;

	private float timeToZoom;

	private float classRecoil = 1f;

	public Weapon[] weapons;

	public static List<GameObject> holes = new List<GameObject>();

	public float overallRecoilFactor = 1.5f;

	public float inventoryCooldown;

	private bool allowShoot;

	private int prevWeapon;

	private float rX;

	private float rY;

	private void Start()
	{
		inv = GetComponent<Inventory>();
		fpc = GetComponent<FirstPersonController>();
		ammoBox = GetComponent<AmmoBox>();
		ccm = GetComponent<CharacterClassManager>();
		invDis = FindAnyObjectByType<InventoryDisplay>();
		if (base.isLocalPlayer)
		{
			if (base.isServer)
			{
				CmdSyncFF(ConfigFile.GetString("friendly_fire", "true").ToLower() == "true");
			}
			holes.Clear();
		}
	}

	[Command(channel = 7)]
	private void CmdSyncFF(bool b)
	{
		syncFF = b;
	}

	private void CalculateCurWeapon()
	{
		curWeapon = -1;
		for (int i = 0; i < weapons.Length; i++)
		{
			if (weapons[i].itemID == inv.curItem)
			{
				curWeapon = i;
				curItem = inv.localInventoryItem;
			}
		}
	}

	private void Shoot()
	{
		if (curItem.durability > 0f)
		{
			CmdDoAnimation("Shoot");
			curItem.durability -= 1f;
			weapons[curWeapon].cooldown = 1f / weapons[curWeapon].fireRate;
			weapons[curWeapon].model.SetTrigger("Shoot");
			weapons[curWeapon].model.GetComponent<AudioSource>().PlayOneShot(weapons[curWeapon].shootAudio);
			CmdDoAudio(weapons[curWeapon].shootAudio_aac, false);
			ParticleSystem[] shotGFX = weapons[curWeapon].shotGFX;
			foreach (ParticleSystem particleSystem in shotGFX)
			{
				particleSystem.Play();
			}
			SetCurRecoil(UnityEngine.Random.Range(1f, 1.5f) * weapons[curWeapon].recoilScale * classRecoil, weapons[curWeapon].recoilScale * classRecoil * (float)UnityEngine.Random.Range(-1, 1));
			Ray ray = new Ray(plyCam.transform.position, plyCam.transform.forward);
			RaycastHit hitInfo;
			if (!Physics.Raycast(ray, out hitInfo, 10000f, mask))
			{
				return;
			}
			float num = weapons[curWeapon].damageOverDistance.Evaluate(hitInfo.distance);
			HitboxIdentity component = hitInfo.collider.GetComponent<HitboxIdentity>();
			if (component == null)
			{
				string[] array = holeTags;
				foreach (string text in array)
				{
					if (text == hitInfo.collider.tag)
					{
						CmdMakeHole(curWeapon, hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal), hitInfo.transform.name);
						break;
					}
				}
			}
			else if (GetShootPermission(component.GetComponentInParent<CharacterClassManager>()))
			{
				if (component.id == "LEG")
				{
					num *= weapons[curWeapon].legDamageMultipiler;
				}
				if (component.id == "BODY")
				{
					num *= weapons[curWeapon].bodyDamageMultipiler;
				}
				if (component.id == "HEAD")
				{
					num *= weapons[curWeapon].headDamageMultipiler;
				}
				if (component.id == "SCP106")
				{
					num *= 0.1f;
				}
				if (hitInfo.collider.tag != "Target")
				{
					Hitmarker.Hit(weapons[curWeapon].hitmarkerResizer);
					HitPlayer(hitInfo.collider.GetComponentInParent<NetworkIdentity>().gameObject, num, netId.ToString(), curWeapon);
				}
				GetComponent<ShootingRange>().PrintDamage(num);
			}
			return;
		}
		if (weapons[curWeapon].ammoAudio != null && weapons[curWeapon].cooldown <= 0f)
		{
			weapons[curWeapon].cooldown = 1f / weapons[curWeapon].fireRate;
			weapons[curWeapon].model.GetComponent<AudioSource>().PlayOneShot(weapons[curWeapon].ammoAudio);
		}
		if (!TutorialManager.status || ammoBox.types[weapons[curWeapon].ammoType].quantity != 0)
		{
			return;
		}
		NoammoTrigger[] array2 = UnityEngine.Object.FindObjectsOfType<NoammoTrigger>();
		NoammoTrigger noammoTrigger = null;
		NoammoTrigger[] array3 = array2;
		foreach (NoammoTrigger noammoTrigger2 in array3)
		{
			if (noammoTrigger == null || noammoTrigger2.prioirty < noammoTrigger.prioirty)
			{
				noammoTrigger = noammoTrigger2;
			}
		}
		if (noammoTrigger != null)
		{
			noammoTrigger.Trigger(curWeapon);
		}
	}

	private IEnumerator Reload()
	{
		if (!(curItem.durability < (float)weapons[curWeapon].magSize) || ammoBox.types[weapons[curWeapon].ammoType].quantity <= 0)
		{
			yield break;
		}
		if (TutorialManager.status)
		{
			FindAnyObjectByType<TutorialManager>().Reload();
		}
		CmdDoAnimation("Reload");
		int startWeapon = curWeapon;
		weapons[curWeapon].cooldown = weapons[curWeapon].reloadingTime + 0.3f;
		weapons[curWeapon].model.SetBool("Reloading", true);
		weapons[curWeapon].model.GetComponent<AudioSource>().PlayOneShot(weapons[curWeapon].reloadAudio);
		CmdDoAudio(weapons[curWeapon].reloadAudio_aac, true);
		yield return new WaitForSeconds(weapons[curWeapon].reloadingTime);
		weapons[startWeapon].model.SetBool("Reloading", false);
		if (startWeapon == curWeapon)
		{
			while (curItem.durability < (float)weapons[curWeapon].magSize && ammoBox.types[weapons[curWeapon].ammoType].quantity > 0)
			{
				curItem.durability += 1f;
				ammoBox.types[weapons[curWeapon].ammoType].quantity--;
			}
		}
	}

	private void FixedUpdate()
	{
		CalculateCurWeapon();
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (invDis.rootObject.activeSelf || CursorManager.consoleOpen || CursorManager.pauseOpen || CursorManager.scp106 || !Application.isFocused)
		{
			inventoryCooldown = 0.4f;
		}
		if (inventoryCooldown > 0f)
		{
			inventoryCooldown -= 0.02f;
		}
		CalculateZoomStatus();
		CalculateMouseSensitivity();
		FixUpdate();
		if (curWeapon >= 0 && allowShoot)
		{
			if (prevWeapon != curWeapon)
			{
				prevWeapon = curWeapon;
			}
			bool flag = weapons[curWeapon].model.GetBool("Zoomed");
			if (weapons[curWeapon].cooldown < 0f)
			{
				weapons[curWeapon].model.SetBool("Zoomed", Input.GetButton("Zoom"));
			}
			plyCam.fieldOfView = ((!flag) ? normalFov : (normalFov + weapons[curWeapon].zoomFOV));
			if (weapons[curWeapon].cooldown < 0f)
			{
				if (Input.GetButton("Reload") && !flag)
				{
					StartCoroutine(Reload());
				}
				if (flag != Input.GetButton("Zoom"))
				{
					weapons[curWeapon].cooldown = 0.3f;
				}
			}
			else
			{
				weapons[curWeapon].cooldown -= Time.deltaTime;
			}
		}
		else
		{
			prevWeapon = curWeapon;
			plyCam.fieldOfView = normalFov;
		}
		allowShoot = !fpc.lockMovement;
	}

	private void Update()
	{
		if (base.name == "Host")
		{
			friendlyFire = syncFF;
		}
		if (base.isLocalPlayer && curWeapon >= 0 && ((!weapons[curWeapon].useFullauto) ? Input.GetButtonDown("Fire1") : Input.GetButton("Fire1")) && allowShoot && weapons[curWeapon].cooldown < 0f && inventoryCooldown <= 0f)
		{
			Shoot();
		}
	}

	public bool GetShootPermission(Team target)
	{
		if (friendlyFire)
		{
			return true;
		}
		Team team = ccm.klasy[ccm.curClass].team;
		if ((team == Team.MTF || team == Team.RSC) && (target == Team.MTF || target == Team.RSC))
		{
			return false;
		}
		if ((team == Team.CDP || team == Team.CHI) && (target == Team.CDP || target == Team.CHI))
		{
			return false;
		}
		return true;
	}

	public bool GetShootPermission(CharacterClassManager c)
	{
		return GetShootPermission(c.klasy[c.curClass].team);
	}

	private void CalculateZoomStatus()
	{
		if (curWeapon >= 0)
		{
			timeToZoom += Time.deltaTime * (float)(weapons[curWeapon].model.GetBool("Zoomed") ? 1 : (-1));
		}
		else
		{
			timeToZoom = 0f;
		}
		timeToZoom = Mathf.Clamp(timeToZoom, 0f, 0.3f);
		if (timeToZoom == 0.3f && curWeapon >= 0)
		{
			unfocusedWeaponCamera.SetActive(false);
			weapons[curWeapon].weaponCamera.SetActive(true);
		}
		else if (!unfocusedWeaponCamera.activeSelf)
		{
			DisableAllWeaponCameras();
		}
	}

	private void CalculateMouseSensitivity()
	{
		float sensitivityMultiplier = 1f;
		float num = 1f;
		if (curWeapon >= 0 && weapons[curWeapon].model.GetBool("Zoomed"))
		{
			sensitivityMultiplier = weapons[curWeapon].zoomMouseSensitivityMultiplier;
			num = weapons[curWeapon].zoomSlowdown;
		}
		fpc.m_MouseLook.sensitivityMultiplier = sensitivityMultiplier;
		if (Input.GetButton("Sneak") && num > sneakSpeed && ccm.curClass >= 0 && ccm.klasy[ccm.curClass].team != Team.SCP)
		{
			num = sneakSpeed;
		}
		fpc.zoomSlowdown = num;
	}

	public void DisableAllWeaponCameras()
	{
		Weapon[] array = weapons;
		foreach (Weapon weapon in array)
		{
			weapon.weaponCamera.SetActive(false);
		}
		unfocusedWeaponCamera.SetActive(true);
	}

	public void SetRecoil(float f)
	{
		classRecoil = f;
	}

	[Command(channel = 10)]
	private void CmdMakeHole(int weapon, Vector3 point, Quaternion quat, string nam)
	{
		RpcMakeHole(weapon, point, quat, nam);
	}

	[ClientRpc(channel = 10)]
	private void RpcMakeHole(int weapon, Vector3 point, Quaternion quat, string nam)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(weapons[weapon].holes[UnityEngine.Random.Range(0, weapons[weapon].holes.Length)], point, quat);
		gameObject.GetComponentInChildren<MeshRenderer>().gameObject.transform.localRotation = Quaternion.Euler(Vector3.up * UnityEngine.Random.Range(0, 360));
		if (nam.ToLower().Contains("door"))
		{
			UnityEngine.Object.Destroy(gameObject, 1.4f);
			return;
		}
		holes.Add(gameObject);
		if (holes.Count > 80)
		{
			GameObject obj = holes[0];
			holes.RemoveAt(0);
			UnityEngine.Object.Destroy(obj);
		}
	}

	private void HitPlayer(GameObject ply, float amount, string attackerID, int weaponID)
	{
		GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(amount, attackerID, "Weapon:" + weaponID), ply);
	}

	[Command(channel = 1)]
	private void CmdDoAnimation(string triggername)
	{
		RpcSyncAnim(triggername);
	}

	[ClientRpc(channel = 1)]
	private void RpcSyncAnim(string triggername)
	{
		if (!base.isLocalPlayer)
		{
			GetComponent<AnimationController>().DoAnimation(triggername);
		}
	}

	[Command(channel = 10)]
	private void CmdDoAudio(string triggername, bool isReloading)
	{
		RpcSyncAudio(triggername, isReloading);
	}

	[ClientRpc(channel = 10)]
	private void RpcSyncAudio(string triggername, bool isReloading)
	{
		if (!base.isLocalPlayer)
		{
			GetComponent<AnimationController>().PlaySound(triggername, !isReloading);
		}
	}

	public void SetCurRecoil(float x, float y)
	{
		float num = ((!weapons[curWeapon].model.GetBool("Zoomed")) ? 1f : weapons[curWeapon].zoomRecoilCompression);
		x *= num * overallRecoilFactor;
		y *= num * overallRecoilFactor;
		rX = x;
		rY = y;
	}

	public void FixUpdate()
	{
		rX -= Time.deltaTime * 5f;
		rY -= Time.deltaTime * 5f;
		if (rX < 0f)
		{
			rX = 0f;
		}
		if (rY < 0f)
		{
			rY = 0f;
		}
		if (rY > 0f || rX > 0f)
		{
			GetComponent<FirstPersonController>().m_MouseLook.LookRotation(base.transform, plyCam.transform, 0f, rY, rX, true);
		}
	}
}
