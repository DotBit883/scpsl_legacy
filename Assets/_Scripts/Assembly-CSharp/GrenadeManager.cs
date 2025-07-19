using System;
using System.Collections;
using UnityEngine;
using Mirror;

public class GrenadeManager : NetworkBehaviour
{
	[Serializable]
	public struct Grenade
	{
		public int inventoryID;

		public GameObject instance;

		public float timeToExplode;

		public float throwAnimationTime;
	}

	[Serializable]
	public struct GrenadeSpawnInfo
	{
		public int grenadeID;

		public Vector3 spawnPosition;

		public Vector3 velocity;

		public float timeToExplode;

		public GrenadeSpawnInfo(int id, Vector3 spawnPos, Vector3 vel, float t)
		{
			grenadeID = id;
			spawnPosition = spawnPos;
			velocity = vel;
			timeToExplode = t;
		}
	}

	private Transform plyCam;

	public float throwSpeed;

	private float inventoryCooldown;

	private Inventory inv;

	private PlayerStats ps;

	public Grenade[] grenades;

	private static int kCmdCmdThrowGrenade;

	private static int kRpcRpcThrowGrenade;

	private static int kCmdCmdExplodeGrenade;

	private static int kRpcRpcExplodeGrenade;

	private void Start()
	{
		plyCam = FindAnyObjectByType<SpectatorCamera>().cam.transform;
		inv = GetComponent<Inventory>();
		ps = GetComponent<PlayerStats>();
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		inventoryCooldown -= Time.deltaTime;
		if (Cursor.lockState != CursorLockMode.Locked)
		{
			inventoryCooldown = 0.2f;
		}
		if (!(inventoryCooldown <= 0f) || !Input.GetButtonDown("Fire1"))
		{
			return;
		}
		for (int i = 0; i < grenades.Length; i++)
		{
			try
			{
				if (inv.curItem == grenades[i].inventoryID)
				{
					inv.items.Remove(inv.localInventoryItem);
					if (!GetComponent<MicroHID_GFX>().onFire)
					{
						StartCoroutine(Throw(i));
					}
				}
			}
			catch
			{
				MonoBehaviour.print("Zatrzymano: " + i);
			}
		}
	}

	[Command(channel = 2)]
	private void CmdThrowGrenade(GrenadeSpawnInfo g)
	{
		RpcThrowGrenade(g);
	}

	[ClientRpc(channel = 2)]
	private void RpcThrowGrenade(GrenadeSpawnInfo g)
	{
		Rigidbody component = UnityEngine.Object.Instantiate(grenades[g.grenadeID].instance, g.spawnPosition, Quaternion.Euler(Vector3.up * 45f)).GetComponent<Rigidbody>();
		component.linearVelocity = g.velocity;
		component.angularVelocity = Vector3.one * 45f;
		component.name = string.Concat("GRENADE#", g.grenadeID, "#", g.spawnPosition, "#", g.velocity, ":");
		UnityEngine.Object.Destroy(component.gameObject, g.timeToExplode + 2f);
		if (base.isLocalPlayer)
		{
			StartCoroutine(CountDown(component.name, g.timeToExplode));
		}
	}

	private IEnumerator CountDown(string grenadeID, float time)
	{
		yield return new WaitForSeconds(time);
		CmdExplodeGrenade(grenadeID, netId.ToString());
	}

	private IEnumerator Throw(int i)
	{
		inv.localInventoryItem.firstpersonModel.GetComponent<Animator>().SetTrigger("Throw");
		GetComponent<MicroHID_GFX>().onFire = true;
		yield return new WaitForSeconds(grenades[i].throwAnimationTime);
		Vector3 pos = plyCam.position + plyCam.forward * 0.2f + plyCam.right * 0.2f;
		GrenadeSpawnInfo g = new GrenadeSpawnInfo(i, pos, plyCam.forward * throwSpeed, grenades[i].timeToExplode);
		CmdThrowGrenade(g);
		inv.curItem = -1;
		GetComponent<MicroHID_GFX>().onFire = false;
	}

	[Command(channel = 2)]
	private void CmdExplodeGrenade(string _name, string _th)
	{
		RpcExplodeGrenade(_name, _th);
	}

	[ClientRpc(channel = 2)]
	private void RpcExplodeGrenade(string _name, string _th)
	{
		GameObject.Find(_name).GetComponent<GrenadeInstance>().Explode(_th);
	}
}
