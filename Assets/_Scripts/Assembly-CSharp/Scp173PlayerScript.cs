using UnityEngine;
using Mirror;
using UnityStandardAssets.ImageEffects;

public class Scp173PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public GameObject scpInstance;
	public bool iAm173;
	public bool sameClass;

	[Header("Raycasting")]
	public GameObject cam;

	public float range;

	public LayerMask layerMask;

	public LayerMask teleportMask;

	public LayerMask hurtLayer;

	[Header("Blinking")]
	public float minBlinkTime;

	public float maxBlinkTime;

	public float blinkDuration_notsee;

	public float blinkDuration_see;

	private float remainingTime;

	private VignetteAndChromaticAberration blinkCtrl;

	private FirstPersonController fpc;

	private PlyMovementSync pms;

	private CharacterClassManager public_ccm;

	private PlayerStats ps;

	public GameObject weaponCameras;

	public GameObject hitbox;

	public string[] necksnaps;

	[Header("Boosts")]
	public AnimationCurve boost_teleportDistance;

	public AnimationCurve boost_speed;

	private bool allowMove = true;

	private int cooldown;

	private void Start()
	{
		ps = GetComponent<PlayerStats>();
		public_ccm = GetComponent<CharacterClassManager>();
		if (base.isLocalPlayer)
		{
			blinkCtrl = GetComponentInChildren<VignetteAndChromaticAberration>();
			fpc = GetComponent<FirstPersonController>();
			pms = GetComponent<PlyMovementSync>();
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		if (base.isLocalPlayer)
		{
			fpc.lookingAtMe = false;
		}
		if (scpInstance != null && scpInstance.GetComponent<CharacterClassManager>().curClass != 0)
		{
			scpInstance = null;
		}
		PlyMovementSync component = GetComponent<PlyMovementSync>();
		Scp173PlayerScript[] array = Object.FindObjectsOfType<Scp173PlayerScript>();
		if (classID == 0)
		{
			iAm173 = true;
			Scp173PlayerScript[] array2 = array;
			foreach (Scp173PlayerScript scp173PlayerScript in array2)
			{
				scp173PlayerScript.scpInstance = base.gameObject;
			}
			component.iAm173 = true;
		}
		else
		{
			iAm173 = false;
			component.iAm173 = false;
		}
		if (base.isLocalPlayer)
		{
			Scp173PlayerScript[] array3 = array;
			foreach (Scp173PlayerScript scp173PlayerScript2 in array3)
			{
				scp173PlayerScript2.hitbox.SetActive(iAm173);
			}
		}
	}

	private void FixedUpdate()
	{
		DoBlinkingSequence();
		if (!base.isLocalPlayer || !iAm173)
		{
			return;
		}
		allowMove = true;
		Scp173PlayerScript[] array = Object.FindObjectsOfType<Scp173PlayerScript>();
		foreach (Scp173PlayerScript scp173PlayerScript in array)
		{
			if (!scp173PlayerScript.sameClass && scp173PlayerScript.LookFor173())
			{
				cooldown = 10;
				allowMove = false;
			}
		}
		CheckForInput();
		fpc.lookingAtMe = !allowMove && cooldown > 0;
		if (cooldown >= 0)
		{
			cooldown--;
		}
		float num = boost_speed.Evaluate(ps.GetHealthPercent());
		fpc.m_WalkSpeed = num;
		fpc.m_RunSpeed = num;
	}

	public bool LookFor173()
	{
		if (!scpInstance || public_ccm.curClass == 2)
		{
			return false;
		}
		RaycastHit hitInfo;
		if (Vector3.Dot(cam.transform.forward, (cam.transform.position - scpInstance.transform.position).normalized) < -0.65f && Physics.Raycast(cam.transform.position, (scpInstance.transform.position - cam.transform.position).normalized, out hitInfo, range, layerMask) && hitInfo.transform.name == scpInstance.name)
		{
			return true;
		}
		return false;
	}

	private void DoBlinkingSequence()
	{
		if (!base.isServer || !base.isLocalPlayer)
		{
			return;
		}
		remainingTime -= Time.fixedDeltaTime;
		if (remainingTime < 0f)
		{
			remainingTime = Random.Range(minBlinkTime, maxBlinkTime);
			Scp173PlayerScript[] array = Object.FindObjectsOfType<Scp173PlayerScript>();
			foreach (Scp173PlayerScript scp173PlayerScript in array)
			{
				scp173PlayerScript.CmdBlinkTime();
			}
		}
	}

	public void Boost()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		pms.SetRotation(base.transform.rotation.eulerAngles.y);
		if (fpc.lookingAtMe)
		{
			bool flag = false;
			RaycastHit hitInfo;
			if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 100f, teleportMask) && hitInfo.transform.GetComponent<CharacterClassManager>() != null && Input.GetAxisRaw("Vertical") > 0f && Input.GetAxisRaw("Horizontal") == 0f)
			{
				flag = true;
			}
			float num = boost_teleportDistance.Evaluate(ps.GetHealthPercent());
			Vector3 position = base.transform.position;
			if (flag)
			{
				Vector3 vector = hitInfo.transform.position - base.transform.position;
				vector = vector.normalized * Mathf.Clamp(vector.magnitude - 1f, 0f, num);
				base.transform.position += vector;
			}
			else
			{
				for (int i = 0; i < 1000; i++)
				{
					if (!(Vector3.Distance(position, base.transform.position) < num))
					{
						break;
					}
					Forward();
				}
			}
		}
		if (Input.GetButton("Fire1"))
		{
			Shoot();
		}
	}

	private void Forward()
	{
		fpc.blinkAddition = 0.8f;
		fpc.MotorPlayer();
		fpc.blinkAddition = 0f;
	}

	public void Blink()
	{
		if (base.isLocalPlayer)
		{
			bool flag = LookFor173();
			if (flag)
			{
				blinkCtrl.intensity = 1f;
				weaponCameras.SetActive(false);
			}
			Invoke("UnBlink", (!flag) ? blinkDuration_notsee : blinkDuration_see);
		}
	}

	private void UnBlink()
	{
		blinkCtrl.intensity = 0.036f;
		weaponCameras.SetActive(true);
	}

	private void CheckForInput()
	{
		if (Input.GetButtonDown("Fire1") && allowMove)
		{
			Shoot();
		}
	}

	private void Shoot()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 1.5f, hurtLayer))
		{
			CharacterClassManager component = hitInfo.transform.GetComponent<CharacterClassManager>();
			if (component != null && component.klasy[component.curClass].team != Team.SCP)
			{
				CmdDoAudio(necksnaps[Random.Range(0, necksnaps.Length)]);
				HurtPlayer(hitInfo.transform.gameObject, netId.ToString());
			}
		}
	}

	private void HurtPlayer(GameObject go, string plyID)
	{
		Hitmarker.Hit();
		GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(999999f, plyID, "SCP:173"), go);
	}

	[Command(channel = 0)]
	private void CmdBlinkTime()
	{
		RpcBlinkTime();
	}

	[ClientRpc(channel = 0)]
	private void RpcBlinkTime()
	{
		if (iAm173)
		{
			Boost();
		}
		if (!sameClass)
		{
			Blink();
		}
	}

	[Command(channel = 1)]
	private void CmdDoAudio(string s)
	{
		RpcSyncAudio(s);
	}

	[ClientRpc(channel = 1)]
	private void RpcSyncAudio(string s)
	{
		if (!base.isLocalPlayer)
		{
			GetComponent<AnimationController>().PlaySound(s, true);
		}
	}
}
