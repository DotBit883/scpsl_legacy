using System;
using UnityEngine;
using Mirror;

public class PlayerStats : NetworkBehaviour
{
	[Serializable]
	public struct HitInfo
	{
		public float amount;

		public string tool;

		public int time;

		public HitInfo(float amnt, string plyID, string weapon)
		{
			amount = amnt;
			tool = weapon;
			time = ServerTime.time;
		}
	}

	public HitInfo lastHitInfo = new HitInfo(0f, "NONE", "NONE");

	[SyncVar]
	public int health;

	public int maxHP;

	private UserMainInterface ui;

	private CharacterClassManager ccm;

	public Vector3 deathPosition;

	private bool applayed;

	private bool ragdollset;

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
		ui = UserMainInterface.singleton;
	}

	public float GetHealthPercent()
	{
		return Mathf.Clamp01(1f - (float)health / (float)maxHP);
	}

	public void Explode()
	{
		bool flag = health >= 1 && base.transform.position.y < 900f;
		if (ccm.curClass == 3)
		{
			Scp106PlayerScript component = GetComponent<Scp106PlayerScript>();
			component.DeletePortal();
			if (component.goingViaThePortal)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			LiftIdentity[] array = UnityEngine.Object.FindObjectsOfType<LiftIdentity>();
			foreach (LiftIdentity liftIdentity in array)
			{
				if (liftIdentity.InArea(base.transform.position))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			DeductHealth(new HitInfo(999999f, "WORLD", "NUKE"));
		}
	}

	private void Update()
	{
		if (base.isLocalPlayer && ccm.curClass != 2)
		{
			ui.SetHP(health, maxHP);
			GameConsole.Console.singleton.UpdateValue("info", lastHitInfo.tool);
		}
		if ((base.isLocalPlayer || TutorialManager.status) && health < 1 && ccm.curClass != 2)
		{
			if (TutorialManager.status)
			{
				FindAnyObjectByType<TutorialManager>().KillNPC();
			}
			deathPosition = base.transform.position;
			GetComponent<WeaponManager>().DisableAllWeaponCameras();
			GetComponent<Inventory>().DropAll();
			SetRagdoll();
			CmdSetSelfID(2, base.gameObject);
		}
		if (base.isLocalPlayer)
		{
			ui.hpOBJ.SetActive(ccm.curClass != 2);
		}
	}

	public void SetRagdoll()
	{
		if (!ragdollset)
		{
			ragdollset = true;
			Invoke(nameof(UnsetRagdoll), 5f);
			CmdAddRagdoll(base.transform.position, base.transform.rotation, ccm.curClass, lastHitInfo, GameObject.Find("Host"), ccm.klasy[ccm.curClass].team != Team.SCP, netId.ToString(), GetComponent<NicknameSync>().myNick);
		}
	}

	private void UnsetRagdoll()
	{
		ragdollset = false;
	}

	public void DeductHealth(HitInfo info)
	{
		if (!ccm.SpawnProtection())
		{
			health = health - Mathf.RoundToInt(info.amount);
		}
	}

	[Command(channel = 11)]
	public void CmdHurtPlayer(HitInfo info, GameObject go)
	{
		if (ServerTime.CheckSynchronization(info.time))
		{
			RpcHurtPlayer(info, go);
		}
	}

	[ClientRpc(channel = 11)]
	public void RpcHurtPlayer(HitInfo info, GameObject go)
	{
		if (!TutorialManager.status)
		{
			go.GetComponent<PlayerStats>().lastHitInfo = info;
		}
		if (base.isServer)
		{
			go.GetComponent<PlayerStats>().DeductHealth(info);
		}
	}

	[Command(channel = 2)]
	public void CmdSetSelfID(int amount, GameObject go)
	{
		go.GetComponent<CharacterClassManager>().curClass = amount;
	}

	[Command(channel = 2)]
	private void CmdAddRagdoll(Vector3 pos, Quaternion rot, int id, HitInfo info, GameObject host, bool allowRecall, string ownerID, string ownerNick)
	{
		host.GetComponent<RagdollManager>().SpawnRagdoll(pos, rot, id, info, allowRecall, ownerID, ownerNick);
	}

	[Command(channel = 7)]
	private void CmdRoundrestart()
	{
		RpcRoundrestart();
	}

	[Command(channel = 2)]
	public void CmdAllowContain()
	{
		FindAnyObjectByType<LureSubjectContainer>().allowContain = true;
	}

	[ClientRpc(channel = 7)]
	private void RpcRoundrestart()
	{
		if (!base.isServer)
		{
			CustomNetworkManager customNetworkManager = FindAnyObjectByType<CustomNetworkManager>();
			customNetworkManager.reconnect = true;
			Invoke(nameof(ChangeLevel), 0.5f);
		}
	}

	public void Roundrestart()
	{
		CmdRoundrestart();
		Invoke(nameof(ChangeLevel), 2.5f);
	}

	private void ChangeLevel()
	{
		if (base.isServer)
		{
			NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
		}
		else
		{
			NetworkManager.singleton.StopClient();
		}
	}
}
