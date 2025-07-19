using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class LureSubjectContainer : NetworkBehaviour
{
	private Vector3 position = new Vector3(-1471f, 160.5f, -3426.9f);

	private Vector3 rotation = new Vector3(0f, 180f, 0f);

	public float range;

	[SyncVar(hook = nameof(SetState))]
	public bool allowContain;

	private Transform localPlayer;

	private CharacterClassManager ccm;

	private PlayerStats ps;

	[Space(10f)]
	public Transform hatch;

	public Vector3 closedPos;

	public Vector3 openPosition;

	public void SetState(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			hatch.GetComponent<AudioSource>().Play();
		}
	}

	private void Start()
	{
		base.transform.localPosition = position;
		base.transform.localRotation = Quaternion.Euler(rotation);
	}

	private void Update()
	{
		CheckForLure();
		hatch.localPosition = Vector3.Slerp(hatch.localPosition, (!allowContain) ? openPosition : closedPos, Time.deltaTime * 3f);
	}

	private void CheckForLure()
	{
		if (localPlayer == null)
		{
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject in players)
			{
				if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					localPlayer = gameObject.transform;
					ps = gameObject.GetComponent<PlayerStats>();
					ccm = gameObject.GetComponent<CharacterClassManager>();
				}
			}
		}
		else
		{
			if (ccm.curClass < 0)
			{
				return;
			}
			Team team = ccm.klasy[ccm.curClass].team;
			if (team != Team.RIP)
			{
				GetComponent<BoxCollider>().enabled = team == Team.SCP;
				if (Vector3.Distance(localPlayer.position, base.transform.position) < range)
				{
					ps.CmdHurtPlayer(new PlayerStats.HitInfo(999999f, "WORLD", "LURE"), localPlayer.gameObject);
					ps.CmdAllowContain();
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, range);
	}
}
