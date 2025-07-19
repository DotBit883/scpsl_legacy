using Steamworks;
using TMPro;
using UnityEngine;

public class PlayButton : MonoBehaviour
{
	public CSteamID ip;

	public void Click()
	{
		CustomNetworkManager customNetworkManager = FindAnyObjectByType<CustomNetworkManager>();
		string lobbyData = SteamMatchmaking.GetLobbyData(ip, "ver");
		if (customNetworkManager.versionstring == lobbyData)
		{
			customNetworkManager.ShowLog(13);
			customNetworkManager.networkAddress = SteamMatchmaking.GetLobbyData(ip, "ServerIP");
			customNetworkManager.StartClient();
		}
		else
		{
			lobbyData = ((!(lobbyData == string.Empty)) ? lobbyData : "[unknown]");
			customNetworkManager.ShowLog(16);
			TextMeshProUGUI component = GameObject.Find("ReasonContent").GetComponent<TextMeshProUGUI>();
			component.text = component.text.Replace("[your]", customNetworkManager.versionstring);
			component.text = component.text.Replace("[server]", lobbyData);
		}
	}
}
