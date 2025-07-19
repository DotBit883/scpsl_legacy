using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class ServerListManager : MonoBehaviour
{
	public class ServerRecord
	{
		public CSteamID ip;

		public string motd;

		public string players;

		public string config;

		public ServerRecord(CSteamID iprotocol, string msg)
		{
			ip = iprotocol;
			motd = msg;
		}
	}

	public RectTransform contentParent;

	public RectTransform element;

	public Text loadingText;

	public static ServerListManager singleton;

	public bool resultRecieved;

	private List<ServerRecord> records = new List<ServerRecord>();

	private List<GameObject> spawns = new List<GameObject>();

	private void Awake()
	{
		singleton = this;
	}

	public void AddRecord(CSteamID iprotocol, string msg)
	{
		records.Add(new ServerRecord(iprotocol, msg));
		RectTransform rectTransform = Object.Instantiate(element);
		rectTransform.SetParent(contentParent);
		rectTransform.localScale = Vector3.one;
		rectTransform.localPosition = Vector3.zero;
		rectTransform.GetComponentInChildren<PlayButton>().ip = iprotocol;
		spawns.Add(rectTransform.gameObject);
		Text[] componentsInChildren = rectTransform.GetComponentsInChildren<Text>();
		foreach (Text text in componentsInChildren)
		{
			text.text = Replace(text.text, "[MOTD]", msg);
		}
		contentParent.sizeDelta = Vector2.up * 150f * records.Count;
	}

	private void OnEnable()
	{
		Refresh();
	}

	public void Refresh()
	{
		resultRecieved = false;
		ResetList();
		FindAnyObjectByType<CustomNetworkManager>().FindMatch();
	}

	public void ResetList()
	{
		foreach (GameObject spawn in spawns)
		{
			Object.Destroy(spawn);
		}
		records.Clear();
		spawns.Clear();
	}

	private void Update()
	{
		if (resultRecieved)
		{
			loadingText.text = ((records.Count > 0) ? string.Empty : ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? "NO SERVERS AVAILABLE" : "BRAK AKTYWNYCH SERWERÓW"));
		}
		else
		{
			loadingText.text = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? "DOWNLOADING DATA" : "POBIERANIE DANYCH");
		}
	}

	private string Replace(string oldString, string tag, string value)
	{
		if (oldString.Contains(tag))
		{
			return oldString.Replace(tag, value);
		}
		return oldString;
	}
}
