using UnityEngine;
using Mirror;

public class PlyMovementSync : NetworkBehaviour
{
	public float positionLerpSpeed = 10f;

	public float rotationLerpSpeed = 15f;

	[SyncVar]
	public float rotation;

	[SyncVar]
	private Vector3 position;

	[SyncVar]
	public float rotX;

	public bool iAmScpAndSomeoneIsLookingAtMe;

	private float myRotation;

	private bool localPlayerTeamAccepted;

	public bool iAm173;

	private Vector3 prevPos;

	private void FixedUpdate()
	{
		if (base.isLocalPlayer && !iAm173)
		{
			myRotation = base.transform.rotation.eulerAngles.y;
		}
		TransmitData();
		RecieveData();
	}

	[ClientCallback]
	private void TransmitData()
	{
		if (NetworkClient.active && base.isLocalPlayer)
		{
			CmdSyncData(myRotation, base.transform.position, GetComponent<PlayerInteract>().playerCamera.transform.localRotation.eulerAngles.x);
		}
	}

	private void Start()
	{
		InvokeRepeating("LocalPlayerTeam", 1f, 10f);
	}

	private void LocalPlayerTeam()
	{
		GameObject[] players = PlayerManager.singleton.players;
		Team team = Team.RIP;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<CharacterClassManager>().isLocalPlayer && gameObject.GetComponent<CharacterClassManager>().curClass >= 0)
			{
				team = gameObject.GetComponent<CharacterClassManager>().klasy[gameObject.GetComponent<CharacterClassManager>().curClass].team;
			}
		}
		localPlayerTeamAccepted = team == Team.RIP || team == Team.SCP;
	}

	private void RecieveData()
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		if (!iAm173 || localPlayerTeamAccepted)
		{
			if (Vector3.Distance(base.transform.position, position) > 10f)
			{
				base.transform.position = position;
			}
			base.transform.position = Vector3.Lerp(base.transform.position, position, Time.deltaTime * positionLerpSpeed);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, rotation, 0f), Time.deltaTime * rotationLerpSpeed);
		}
		else
		{
			base.transform.position = position;
			base.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
		}
	}

	[Command(channel = 5)]
	private void CmdSyncData(float rot, Vector3 pos, float x)
	{
		rotation = rot;
		position = pos;
		rotX = x;
	}

	public void SetRotation(float rot)
	{
		myRotation = rot;
	}
}
