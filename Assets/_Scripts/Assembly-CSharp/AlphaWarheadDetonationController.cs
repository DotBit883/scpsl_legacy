using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class AlphaWarheadDetonationController : NetworkBehaviour
{
	[SyncVar]
	public float detonationTime;

	private bool detonationInProgress;

	private bool detonated;

	private bool doorsOpen;

	private bool blastDoors;

	private GameObject host;

	private bool lightStatus;

	private AWSoundController awsc;

	private AlphaWarheadDetonationController awdc;

	private ToggleableLight[] lights;

	public void StartDetonation()
	{
		if (!detonationInProgress)
		{
			detonationInProgress = true;
			detonationTime = 90f;
			doorsOpen = false;
		}
	}

	public void CancelDetonation()
	{
		if (detonationInProgress && detonationTime > 2f)
		{
			detonationInProgress = false;
			detonationTime = 0f;
			doorsOpen = false;
		}
	}

	private void FixedUpdate()
	{
		if (base.isLocalPlayer && awdc != null && lightStatus != (awdc.detonationTime != 0f))
		{
			lightStatus = awdc.detonationTime != 0f;
			SetLights(lightStatus);
		}
		if (base.name == "Host")
		{
			if (detonationTime > 0f)
			{
				detonationTime = detonationTime - Time.deltaTime;
				if (!GameObject.Find("Lever_Alpha_Controller").GetComponent<LeverButton>().GetState())
				{
					CancelDetonation();
				}
				if (detonationTime < 83f && !doorsOpen && base.isLocalPlayer)
				{
					doorsOpen = true;
					CmdOpenDoors();
				}
				if (detonationTime < 2f && !blastDoors && detonationInProgress && base.isLocalPlayer)
				{
					blastDoors = true;
					CmdCloseBlastDoors();
				}
			}
			else
			{
				if (detonationTime < 0f)
				{
					GetComponent<RoundSummary>().summary.warheadDetonated = true;
					Explode();
				}
				detonationTime = 0f;
			}
		}
		if (base.isLocalPlayer && base.isServer)
		{
			TransmitData(detonationTime);
		}
		if (awsc == null || awdc == null)
		{
			awsc = FindAnyObjectByType<AWSoundController>();
			if (host == null)
			{
				host = GameObject.Find("Host");
			}
			if (host != null)
			{
				awdc = host.GetComponent<AlphaWarheadDetonationController>();
			}
		}
		else
		{
			awsc.UpdateSound(90f - awdc.detonationTime, detonated);
		}
	}

	private void Explode()
	{
		detonated = true;
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject ply in players)
		{
			CmdExplodePlayer(ply);
		}
	}

	[Command(channel = 2)]
	private void CmdOpenDoors()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Door");
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject.GetComponent<Door>().isAbleToEmergencyOpen())
			{
				gameObject.GetComponent<Door>().isOpen = true;
			}
		}
	}

	[Command(channel = 2)]
	private void CmdExplodePlayer(GameObject ply)
	{
		ply.GetComponent<PlayerStats>().Explode();
	}

	[Command(channel = 7)]
	private void CmdCloseBlastDoors()
	{
		BlastDoor[] array = Object.FindObjectsOfType<BlastDoor>();
		foreach (BlastDoor blastDoor in array)
		{
			blastDoor.isClosed = true;
		}
	}

	[ClientCallback]
	private void TransmitData(float t)
	{
		if (NetworkClient.active)
		{
			CmdSyncData(t);
		}
	}

	[Command(channel = 5)]
	private void CmdSyncData(float t)
	{
		detonationTime = t;
	}

	private void Start()
	{
		lights = Object.FindObjectsOfType<ToggleableLight>();
	}

	private void SetLights(bool b)
	{
		ToggleableLight[] array = lights;
		foreach (ToggleableLight toggleableLight in array)
		{
			toggleableLight.SetLights(b);
		}
	}
}
