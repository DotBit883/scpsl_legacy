using System;
using System.Collections;
using System.Net;
using Steamworks;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{
	[Serializable]
	public class DisconnectLog
	{
		[Serializable]
		public class LogButton
		{
			public ConnInfoButton[] actions;

			public string content_en;

			public string content_pl;

			public float size_en;

			public float size_pl;
		}

		[Multiline]
		public string msg_en;

		[Multiline]
		public string msg_pl;

		public Vector2 msgSize_en;

		public Vector2 msgSize_pl;

		public LogButton button;

		public bool autoHideOnSceneLoad;
	}

	public GameObject popup;

	public GameObject createpop;

	public RectTransform contSize;

	public TextMeshProUGUI content;

	public Button button;

	public DisconnectLog[] logs;

	private int curLogID;

	public bool reconnect;

	[Space(20f)]
	public string versionstring = "Open-Beta 2.0";

	private Callback<LobbyCreated_t> Callback_lobbyCreated;

	private Callback<LobbyEnter_t> Callback_lobbyEnter;

	private Callback<LobbyMatchList_t> Callback_lobbyList;

	private bool isHost;

	private GameConsole.Console console;

	public override void OnClientDisconnect()
	{
		ShowLog(1);
	}

	public override void OnClientError(TransportError error, string reason)
	{
		ShowLog(MapTransportErrorToLogId(error));
	}

	public override void OnServerConnect(NetworkConnectionToClient conn)
	{
		foreach (BanPlayer.Ban ban in BanPlayer.bans)
		{
			if (ban.ip == conn.address && BanPlayer.NotExpired(ban.time))
			{
				conn.Disconnect();
			}
		}
	}

	// private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	// {
	// 	if (reconnect)
	// 	{
	// 		ShowLog(14);
	// 		Invoke(nameof(Reconnect), 2f);
	// 	}
	// }

	public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
	{
		if (reconnect)
		{
			ShowLog(14);
			Invoke(nameof(Reconnect), 2f);
		}
		if (!reconnect && logs[curLogID].autoHideOnSceneLoad)
		{
			popup.SetActive(false);
		}
	}

	private void Reconnect()
	{
		if (reconnect)
		{
			StartClient();
			reconnect = false;
		}
	}

	public void StopReconnecting()
	{
		reconnect = false;
	}

	public void ShowLog(int id)
	{
		curLogID = id;
		bool flag = PlayerPrefs.GetString("langver", "en") == "pl";
		popup.SetActive(true);
		contSize.sizeDelta = ((!flag) ? logs[id].msgSize_en : logs[id].msgSize_pl);
		content.text = ((!flag) ? logs[id].msg_en : logs[id].msg_pl);
		button.GetComponentInChildren<Text>().text = ((!flag) ? logs[id].button.content_en : logs[id].button.content_pl);
		button.GetComponent<RectTransform>().sizeDelta = new Vector2((!flag) ? logs[id].button.size_en : logs[id].button.size_pl, 80f);
	}

	public void ClickButton()
	{
		ConnInfoButton[] actions = logs[curLogID].button.actions;
		foreach (ConnInfoButton connInfoButton in actions)
		{
			connInfoButton.UseButton();
		}
	}

	public override void Start()
	{
		base.Start();
		console = GameConsole.Console.singleton;
		Callback_lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
		Callback_lobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
		// SceneManager.sceneLoaded += OnLevelFinishedLoading;
		if (!SteamAPI.Init())
		{
			console.AddLog("Failed to init SteamAPI.", new Color32(128, 128, 128, byte.MaxValue));
		}
	}

	public void CreateMatch()
	{
		createpop.SetActive(false);
		if (ServerStatic.isDedicated)
		{
			if (SteamManager.Initialized)
			{
				SteamServerManager._instance.CreateServer();
			}
			else
			{
				NonsteamHost();
			}
			return;
		}
		ShowLog(13);
		if (Input.GetKey(KeyCode.Space))
		{
			NonsteamHost();
		}
		else
		{
			SteamServerManager._instance.CreateServer();
		}
	}

	private void NonsteamHost()
	{
		base.onlineScene = "Facility";
		base.maxConnections = 20;
		StartHost();
	}

	public void FindMatch()
	{
		SteamMatchmaking.RequestLobbyList();
	}

	private void OnLobbyCreated(LobbyCreated_t result)
	{
		if (result.m_eResult == EResult.k_EResultOK)
		{
			console.AddLog("Steam lobby created!", new Color32(128, 128, 128, byte.MaxValue));
			string text = SteamGameServer.GetPublicIP().ToString();
			console.AddLog("Your machine IP is " + text, new Color32(128, 128, 128, byte.MaxValue));
			ServerConsole.AddLog(text);
			string text2 = ConfigFile.GetString("server_ip", "auto");
			string text3 = ConfigFile.GetString("server_name", "[nick]'s game");
			text3 = ((!text3.Contains("[nick]")) ? text3 : text3.Replace("[nick]", SteamFriends.GetPersonaName()));
			if (text2 != "auto")
			{
				text = text2;
			}
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "ServerIP", text);
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "MOTD", text3);
			SteamMatchmaking.SetLobbyData((CSteamID)result.m_ulSteamIDLobby, "ver", versionstring);
			isHost = true;
			StartHost();
		}
		else
		{
			console.AddLog("Steam lobby not created. Error: " + result.m_eResult.ToString() + ".", new Color32(128, 128, 128, byte.MaxValue));
		}
	}

	private void OnGetLobbiesList(LobbyMatchList_t result)
	{
		StartCoroutine(ShowList(result));
	}

	private IEnumerator ShowList(LobbyMatchList_t result)
	{
		ServerListManager slm = ServerListManager.singleton;
		yield return new WaitForSeconds(0.5f);
		slm.resultRecieved = true;
		console.AddLog("Found lobbies: " + result.m_nLobbiesMatching, new Color32(128, 128, 128, byte.MaxValue));
		for (int i = 0; i < result.m_nLobbiesMatching; i++)
		{
			CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
			string ip = SteamMatchmaking.GetLobbyData(lobbyID, "ServerIP");
			if (ip != string.Empty)
			{
				slm.AddRecord(lobbyID, SteamMatchmaking.GetLobbyData(lobbyID, "MOTD"));
				yield return new WaitForEndOfFrame();
			}
		}
	}
	
	private int MapTransportErrorToLogId(TransportError error)
	{
		return error switch
		{
			TransportError.DnsResolve       => 16, // Invalid or unresolvable address
			TransportError.Timeout          => 7,  // Timed out
			TransportError.Congestion       => 11, // Server is full (approx.)
			TransportError.InvalidReceive   => 12, // Received malformed/wrong packet
			TransportError.ConnectionClosed => 1,  // Host not available / closed
			_                               => 15, // Unknown / unexpected
		};
	}
}
