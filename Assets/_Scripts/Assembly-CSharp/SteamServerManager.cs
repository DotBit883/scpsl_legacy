using System;
using System.Collections;
using System.Net;
using GameConsole;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamServerManager : MonoBehaviour
{
	public static SteamServerManager _instance;

	private bool gs_Initialized;

	private Callback<SteamServersConnected_t> Callback_ServerConnected;

	private GameConsole.Console console;

	private void Start()
	{
		console = GameConsole.Console.singleton;
		_instance = this;
		SceneManager.sceneLoaded += OnSceneLoaded;
		Callback_ServerConnected = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServerConnected);
	}

	public void CreateServer()
	{
		gs_Initialized = GameServer.Init(0u, 7777, 7777, EServerMode.eServerModeNoAuthentication, "SCPSL");
		if (!gs_Initialized)
		{
			console.AddLog("SteamGameServer_Init call failed!", new Color32(128, 128, 128, byte.MaxValue));
		}
		else
		{
			SteamGameServer.LogOnAnonymous();
		}
	}

	private void OnSteamServerConnected(SteamServersConnected_t pLogonSuccess)
	{
		console.AddLog("Secret Laboratory connected to Steam successfully.", new Color32(128, 128, 128, byte.MaxValue));
		StartCoroutine(CreateLobby());
	}

	private IEnumerator CreateLobby()
	{
		yield return new WaitForEndOfFrame();
		ELobbyType type = ELobbyType.k_ELobbyTypeFriendsOnly;
		string ip = SteamGameServer.GetPublicIP().ToString();
		WWWForm wwwform = new WWWForm();
		wwwform.AddField("ip", ip);
		WWW www = new WWW("https://hubertmoszka.pl/server_authenticator.php", wwwform);
		yield return www;
		if (!string.IsNullOrEmpty(www.text) && www.text.Contains("YES"))
		{
			console.AddLog("Your public server is now ready for everyone.", new Color32(128, 128, 128, byte.MaxValue));
			type = ELobbyType.k_ELobbyTypePublic;
		}
		else
		{
			console.AddLog("Your server is now ready for your friends.", new Color32(128, 128, 128, byte.MaxValue));
		}
		SteamMatchmaking.CreateLobby(type, 20);
	}

	private void OnDisable()
	{
		Shutdown();
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 0 && !FindAnyObjectByType<CustomNetworkManager>().reconnect)
		{
			Shutdown();
		}
	}

	private void Shutdown()
	{
		if (gs_Initialized)
		{
			gs_Initialized = false;
			SteamGameServer.LogOff();
			GameServer.Shutdown();
			console.AddLog("Secret Laboratory server stopped", new Color32(128, 128, 128, byte.MaxValue));
		}
	}

	private void Update()
	{
		if (gs_Initialized)
		{
			GameServer.RunCallbacks();
		}
	}
}
