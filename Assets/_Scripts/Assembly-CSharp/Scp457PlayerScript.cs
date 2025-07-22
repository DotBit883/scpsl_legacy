using System.Collections;
using UnityEngine;
using Mirror;

public class Scp457PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public Camera plyCam;
	public bool iAm457;
	public bool sameClass;
	public float ultimatePoints;
	public float burnTime = 5f;
	private float curBurn;
	private GameObject[] players;

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		if (classID == 9)
		{
			iAm457 = true;
		}
		else
		{
			iAm457 = false;
		}
	}

	private void Start()
	{
		StartCoroutine(DeductFireHP());
		InvokeRepeating(nameof(DetectPlayersInRange), 1f, 0.2f);
		InvokeRepeating(nameof(RefreshPlayerList), 1f, 5f);
	}

	private void RefreshPlayerList()
	{
		players = PlayerManager.singleton.players;
	}

	private void DetectPlayersInRange()
	{
		if (isLocalPlayer && iAm457)
		{
			foreach (GameObject player in players)
			{
				if (player != null && !player.GetComponent<Scp457PlayerScript>().sameClass &&
				    Vector3.Distance(transform.position, player.transform.position) < 2f)
				{
					CmdBurnPlayer(player.transform.gameObject);
				}
			}
		}
	}

	private IEnumerator DeductFireHP()
	{
		if (!isLocalPlayer)
		{
			yield break;
		}
		while (true)
		{
			if (curBurn > 0f)
			{
				curBurn -= 0.2f;
				CmdSelfDeduct(gameObject, 2f);
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

	public void Burn()
	{
		curBurn = burnTime;
	}

	[Command(channel = 2)]
	private void CmdSelfDeduct(GameObject go, float am)
	{
		go.GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(am, "NONE", "NONE"), go);
	}

	[ClientRpc(channel = 2)]
	private void RpcBurnPlayer(GameObject go)
	{
		go.GetComponent<Scp457PlayerScript>().Burn();
	}

	[Command(channel = 2)]
	private void CmdBurnPlayer(GameObject go)
	{
		RpcBurnPlayer(go);
	}
}
