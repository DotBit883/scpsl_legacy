using System.Collections;
using UnityEngine;
using Mirror;

public class MicroHID_GFX : NetworkBehaviour
{
	public Light[] progress;

	public ParticleSystem teslaFX;

	public Animator anim;

	public AudioSource shotSource;

	public bool onFire;

	public float range;

	public GameObject cam;

	private PlayerManager pmng;

	private InventoryDisplay invdis;

	private float damageGiven;

	private void Start()
	{
		pmng = PlayerManager.singleton;
		invdis = FindAnyObjectByType<InventoryDisplay>();
	}

	private void Update()
	{
		if (base.isLocalPlayer && Input.GetButtonDown("Fire1") && GetComponent<Inventory>().curItem == 16 && !onFire && GetComponent<WeaponManager>().inventoryCooldown <= 0f && GetComponent<Inventory>().localInventoryItem.durability > 0f)
		{
			onFire = true;
			GetComponent<Inventory>().localInventoryItem.durability = 0f;
			CmdDoAnimation("Shoot");
			StartCoroutine(PlayAnimation());
		}
	}

	private IEnumerator PlayAnimation()
	{
		damageGiven = 0f;
		anim.SetTrigger("Shoot");
		shotSource.Play();
		Light[] array = progress;
		foreach (Light light in array)
		{
			light.intensity = 0f;
		}
		GlowLight(0);
		yield return new WaitForSeconds(2.2f);
		GlowLight(1);
		yield return new WaitForSeconds(2.2f);
		GlowLight(2);
		yield return new WaitForSeconds(2.2f);
		GlowLight(3);
		GlowLight(5);
		yield return new WaitForSeconds(2.2f);
		GlowLight(4);
		yield return new WaitForSeconds(0.6f);
		teslaFX.Play();
		for (int j = 0; j < 20; j++)
		{
			HurtPlayersInRange();
			yield return new WaitForSeconds(0.25f);
		}
		onFire = false;
		Light[] array2 = progress;
		foreach (Light light2 in array2)
		{
			light2.intensity = 0f;
		}
	}

	private void HurtPlayersInRange()
	{
		GameObject[] players = pmng.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			RaycastHit hitInfo;
			if (Vector3.Dot(cam.transform.forward, (cam.transform.position - gameObject.transform.position).normalized) < -0.92f && Physics.Raycast(cam.transform.position, (gameObject.transform.position - cam.transform.position).normalized, out hitInfo, range) && hitInfo.transform.name == gameObject.name)
			{
				Hitmarker.Hit(2.3f);
				GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(Random.Range(100, 200), netId.ToString(), "TESLA"), gameObject);
			}
		}
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
			GetComponent<AnimationController>().PlaySound("HID_Shoot", true);
			GetComponent<AnimationController>().DoAnimation(triggername);
		}
	}

	private void GlowLight(int id)
	{
		float targetIntensity;
		switch (id)
		{
		case 5:
			targetIntensity = 50f;
			break;
		case 4:
			targetIntensity = 6f;
			break;
		default:
			targetIntensity = 3f;
			break;
		}
		StartCoroutine(SetLightState(targetIntensity, progress[id], (id != 5) ? 2f : 50f));
	}

	private IEnumerator SetLightState(float targetIntensity, Light light, float speed)
	{
		while (light.intensity < targetIntensity)
		{
			light.intensity += Time.deltaTime * speed;
			yield return new WaitForEndOfFrame();
		}
	}
}
