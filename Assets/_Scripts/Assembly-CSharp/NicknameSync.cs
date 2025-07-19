using System.Runtime.InteropServices;
using GameConsole;
using Steamworks;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class NicknameSync : NetworkBehaviour
{
	[SyncVar]
	public string myNick;

	public float viewRange;

	private Text n_text;

	private float transparency;

	public LayerMask raycastMask;

	private Transform spectCam;

	private void Start()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		string empty = string.Empty;
		if (SteamManager.Initialized)
		{
			empty = ((SteamFriends.GetPersonaName() != null) ? SteamFriends.GetPersonaName() : "Player");
		}
		else
		{
			Console.singleton.AddLog("Steam has been not initialized!", new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
			if (PlayerPrefs.HasKey("nickname"))
			{
				empty = PlayerPrefs.GetString("nickname");
			}
			else
			{
				string text = "Player " + SystemInfo.processorType + SystemInfo.operatingSystem;
				PlayerPrefs.SetString("nickname", text);
				empty = text;
			}
		}
		while (empty.Contains("<"))
		{
			empty = empty.Replace("<", "＜");
		}
		while (empty.Contains(">"))
		{
			empty = empty.Replace(">", "＞");
		}
		CmdSetNick(empty);
		spectCam = FindAnyObjectByType<SpectatorCamera>().cam.transform;
		n_text = GameObject.Find("Nickname Text").GetComponent<Text>();
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		bool flag = false;
		RaycastHit hitInfo;
		if (Physics.Raycast(new Ray(spectCam.position, spectCam.forward), out hitInfo, viewRange, raycastMask))
		{
			NicknameSync component = hitInfo.transform.GetComponent<NicknameSync>();
			if (component != null && !component.isLocalPlayer)
			{
				CharacterClassManager component2 = component.GetComponent<CharacterClassManager>();
				CharacterClassManager component3 = GetComponent<CharacterClassManager>();
				flag = true;
				n_text.color = component2.klasy[component2.curClass].classColor;
				n_text.text = string.Empty;
				n_text.text += component.myNick;
				Text text = n_text;
				text.text = text.text + "\n" + ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? component2.klasy[component2.curClass].fullName : component2.klasy[component2.curClass].fullName_pl);
				try
				{
					if (component2.klasy[component2.curClass].team == Team.MTF && component3.klasy[component3.curClass].team == Team.MTF)
					{
						int num = 0;
						int num2 = 0;
						if (component2.curClass == 4 || component2.curClass == 11)
						{
							num2 = 200;
						}
						else if (component2.curClass == 13)
						{
							num2 = 100;
						}
						else if (component2.curClass == 12)
						{
							num2 = 300;
						}
						if (component3.curClass == 4 || component3.curClass == 11)
						{
							num = 200;
						}
						else if (component3.curClass == 13)
						{
							num = 100;
						}
						else if (component3.curClass == 12)
						{
							num = 300;
						}
						Text text2 = n_text;
						text2.text = text2.text + " (" + GameObject.Find("Host").GetComponent<NineTailedFoxUnits>().GetNameById(component2.ntfUnit) + ")\n\n<b>";
						num -= component3.ntfUnit;
						num2 -= component2.ntfUnit;
						if (num > num2)
						{
							n_text.text += ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? "YOU CAN GIVE ORDERS" : "MOŻESZ DAWAĆ ROZKAZY");
						}
						else if (num2 > num)
						{
							n_text.text += ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? "FOLOW ORDERS" : "WYKONUJ ROZKAZY");
						}
						else if (num2 == num)
						{
							n_text.text += ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? "THE SAME PERMISSION LEVEL" : "RÓWNY POZIOM UPRAWNIEŃ");
						}
						n_text.text += "</b>";
					}
				}
				catch
				{
					MonoBehaviour.print("Error");
				}
			}
		}
		transparency += Time.deltaTime * (float)((!flag) ? (-3) : 3);
		if (flag)
		{
			float max = (viewRange - Vector3.Distance(base.transform.position, hitInfo.point)) / viewRange;
			transparency = Mathf.Clamp(transparency, 0f, max);
		}
		transparency = Mathf.Clamp01(transparency);
		CanvasRenderer component4 = n_text.GetComponent<CanvasRenderer>();
		component4.SetAlpha(transparency);
	}

	[Command(channel = 2)]
	private void CmdSetNick(string n)
	{
		myNick = n;
	}
}
