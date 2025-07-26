using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class DisableUselessComponents : NetworkBehaviour
{
	[SerializeField]
	private Behaviour[] uselessComponents;

	[SyncVar]
	private string label = "Player";

	[SyncVar]
	public bool isDedicated = true;

	private bool added;

	private void OnDestroy()
	{
		PlayerManager.singleton.RemovePlayer(base.gameObject);
	}

	private void Start()
	{
		if (!base.isLocalPlayer)
		{
			Object.DestroyImmediate(GetComponent<FirstPersonController>());
			Behaviour[] array = uselessComponents;
			foreach (Behaviour behaviour in array)
			{
				behaviour.enabled = false;
			}
			Object.Destroy(GetComponent<CharacterController>());
		}
		else
		{
			CmdSetName((!base.isServer) ? "Player" : "Host", ServerStatic.isDedicated);
			FindAnyObjectByType<MusicManager>().SetPlayer(base.gameObject);
			GetComponent<FirstPersonController>().enabled = false;
		}
	}

	private void FixedUpdate()
	{
		if (!isDedicated && !added && !string.IsNullOrEmpty(GetComponent<BanPlayer>().hardwareID))
		{
			added = true;
			try
			{
				if (GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					foreach (BanPlayer.Ban ban in BanPlayer.bans)
					{
						if (ban.hardware == GetComponent<BanPlayer>().hardwareID && BanPlayer.NotExpired(ban.time))
						{
							GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
							added = false;
							return;
						}
					}
				}
			}
			catch
			{
			}
			PlayerManager.singleton.AddPlayer(base.gameObject);
		}
		base.name = label;
	}

	[Command(channel = 2)]
	private void CmdSetName(string n, bool b)
	{
		label = n;
		isDedicated = b;
	}
}
