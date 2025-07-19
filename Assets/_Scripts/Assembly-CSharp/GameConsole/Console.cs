using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameConsole
{
	public class Console : MonoBehaviour
	{
		[Serializable]
		public class CommandHint
		{
			public string name;

			public string shortDesc;

			[Multiline]
			public string fullDesc;
		}

		[Serializable]
		public class Value
		{
			public string key;

			public string value;

			public Value(string k, string v)
			{
				key = k;
				value = v;
			}
		}

		[Serializable]
		public class Log
		{
			public string text;

			public Color32 color;

			public bool nospace;

			public Log(string t, Color32 c, bool b)
			{
				text = t;
				color = c;
				nospace = b;
			}
		}

		private bool allwaysRefreshing;

		private List<Log> logs = new List<Log>();

		private List<Value> values = new List<Value>();

		public CommandHint[] hints;

		public Text txt;

		public InputField cmdField;

		public GameObject console;

		public static Console singleton;

		private int scrollup;

		private int previous_scrlup;

		private string loadedLevel;

		private string response = string.Empty;

		public List<Log> GetAllLogs()
		{
			return logs;
		}

		public void UpdateValue(string key, string value)
		{
			bool flag = false;
			key = key.ToUpper();
			foreach (Value value2 in values)
			{
				if (value2.key == key)
				{
					value2.value = value;
					flag = true;
				}
			}
			if (!flag)
			{
				values.Add(new Value(key, value));
			}
		}

		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			if (singleton == null)
			{
				singleton = this;
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
			}
		}

		private void Start()
		{
			AddLog("Hi there! Initializing console...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			AddLog("Done! Type 'help' to print the list of available commands.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			RefreshConsoleScreen();
		}

		private void RefreshConsoleScreen()
		{
			bool flag = false;
			if (txt.text.Length > 15000)
			{
				logs.RemoveAt(0);
				flag = true;
			}
			if (txt == null)
			{
				return;
			}
			txt.text = string.Empty;
			if (logs.Count > 0)
			{
				for (int i = 0; i < logs.Count - scrollup; i++)
				{
					string text = ((!logs[i].nospace) ? "\n\n" : "\n") + "<color=" + ColorToHex(logs[i].color) + ">" + logs[i].text + "</color>";
					if (text.Contains("@#{["))
					{
						string text2 = text.Remove(text.IndexOf("@#{["));
						string text3 = text.Remove(0, text.IndexOf("@#{[") + 4);
						text3 = text3.Remove(text3.Length - 12);
						foreach (Value value in values)
						{
							if (value.key == text3)
							{
								text = text2 + value.value + "</color>";
							}
						}
					}
					txt.text += text;
				}
			}
			if (flag)
			{
				RefreshConsoleScreen();
			}
		}

		public void AddLog(string text, Color32 c, bool nospace = false)
		{
			response = response + text + Environment.NewLine;
			if (!nospace)
			{
				response += Environment.NewLine;
			}
			scrollup = 0;
			logs.Add(new Log(text, c, nospace));
			RefreshConsoleScreen();
		}

		private string ColorToHex(Color32 color)
		{
			string text = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return "#" + text;
		}

		public static GameObject FindConnectedRoot(NetworkConnectionToClient conn)
		{
			try
			{
				if (conn.identity != null && conn.identity.gameObject.CompareTag("Player"))
				{
					return conn.identity.gameObject;
				}
			}
			catch
			{
				// ignored
			}

			return null;
		}

		public string TypeCommand(string cmd)
		{
			try
			{
				if (!GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					AddLog("Console commands are disabled for the clients.", new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
					return "not owner";
				}
			}
			catch
			{
				return "not owner";
			}
			response = string.Empty;
			string[] array = cmd.ToUpper().Split(' ');
			cmd = array[0];
			switch (cmd)
			{
			case "HELLO":
				AddLog("Hello World!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				break;
			case "LENNY":
				AddLog("<size=450>( \u0361° \u035cʖ \u0361°)</size>\n\n", new Color32(byte.MaxValue, 180, 180, byte.MaxValue));
				break;
			case "GIVE":
			{
				int result2 = 0;
				if (array.Length >= 2 && int.TryParse(array[1], out result2))
				{
					string text2 = "offline";
					GameObject[] array4 = GameObject.FindGameObjectsWithTag("Player");
					GameObject[] array5 = array4;
					foreach (GameObject gameObject4 in array5)
					{
						if (!gameObject4.GetComponent<NetworkIdentity>().isLocalPlayer)
						{
							continue;
						}
						text2 = "online";
						Inventory component = gameObject4.GetComponent<Inventory>();
						if (component != null)
						{
							if (component.availableItems.Length > result2)
							{
								component.AddItem(result2);
								text2 = "none";
							}
							else
							{
								AddLog("Failed to add ITEM#" + result2.ToString("000") + " - item does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
						}
					}
					if (text2 == "offline" || text2 == "online")
					{
						AddLog((!(text2 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else
					{
						AddLog("ITEM#" + result2.ToString("000") + " has been added!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					}
				}
				else
				{
					AddLog("Second argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "ITEMLIST":
			{
				string text5 = "offline";
				GameObject[] array12 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array13 = array12;
				foreach (GameObject gameObject9 in array13)
				{
					int result4 = 1;
					if (array.Length >= 2 && !int.TryParse(array[1], out result4))
					{
						AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						return response;
					}
					if (!gameObject9.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						continue;
					}
					text5 = "online";
					Inventory component3 = gameObject9.GetComponent<Inventory>();
					if (!(component3 != null))
					{
						continue;
					}
					text5 = "none";
					if (result4 < 1)
					{
						AddLog("Page '" + result4 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						RefreshConsoleScreen();
						return response;
					}
					Item[] availableItems = component3.availableItems;
					for (int num2 = 10 * (result4 - 1); num2 < 10 * result4; num2++)
					{
						if (10 * (result4 - 1) > availableItems.Length)
						{
							AddLog("Page '" + result4 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							break;
						}
						if (num2 >= availableItems.Length)
						{
							break;
						}
						AddLog("ITEM#" + num2.ToString("000") + " : " + availableItems[num2].label, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
				}
				if (text5 != "none")
				{
					AddLog((!(text5 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "CLS":
			case "CLEAR":
				logs.Clear();
				RefreshConsoleScreen();
				break;
			case "QUIT":
			case "EXIT":
				logs.Clear();
				RefreshConsoleScreen();
				AddLog("<size=50>GOODBYE!</size>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				RefreshConsoleScreen();
				Invoke("QuitGame", 1f);
				break;
			case "HELP":
			{
				if (array.Length > 1)
				{
					string text7 = array[1];
					CommandHint[] array16 = hints;
					foreach (CommandHint commandHint in array16)
					{
						if (commandHint.name == text7)
						{
							AddLog(commandHint.name + " - " + commandHint.fullDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
							RefreshConsoleScreen();
							return response;
						}
					}
					AddLog("Help for command '" + array[1] + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					RefreshConsoleScreen();
					return response;
				}
				AddLog("List of available commands:\n", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				CommandHint[] array17 = hints;
				foreach (CommandHint commandHint2 in array17)
				{
					AddLog(commandHint2.name + " - " + commandHint2.shortDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), true);
				}
				AddLog("Type 'HELP [COMMAND]' to print a full description of the chosen command.", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				RefreshConsoleScreen();
				break;
			}
			case "REFRESHFIX":
				allwaysRefreshing = !allwaysRefreshing;
				AddLog("Console log refresh mode: " + ((!allwaysRefreshing) ? "OPTIMIZED" : "FIXED"), new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				break;
			case "VALUE":
			{
				if (array.Length < 2)
				{
					AddLog("The second argument cannot be <i>null</i>!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				}
				bool flag4 = false;
				string text8 = array[1];
				foreach (Value value in values)
				{
					if (value.key == text8)
					{
						flag4 = true;
						AddLog("The value of " + text8 + " is: @#{[" + text8 + "}]#@", new Color32(50, 70, 100, byte.MaxValue));
					}
				}
				if (!flag4)
				{
					AddLog("Key " + text8 + " not found!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "SEED":
			{
				GameObject gameObject13 = GameObject.Find("Host");
				int num8 = -1;
				if (gameObject13 != null)
				{
					num8 = gameObject13.GetComponent<RandomSeedSync>().seed;
				}
				AddLog("Map seed is: <b>" + ((num8 != -1) ? num8.ToString() : "NONE") + "</b>", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				break;
			}
			case "NOCLIP":
			{
				bool flag = true;
				GameObject[] array2 = GameObject.FindGameObjectsWithTag("Player");
				Noclip noclip = null;
				GameObject[] array3 = array2;
				foreach (GameObject gameObject in array3)
				{
					if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						noclip = gameObject.GetComponent<Noclip>();
						if (noclip != null)
						{
							noclip.Switch();
							flag = false;
						}
					}
				}
				if (flag)
				{
					AddLog("Noclip couldn't be activated!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				else
				{
					AddLog((!noclip.Get()) ? "Noclip <b>disabled!</b>" : "Noclip <b>activated!</b> <b>Press 'N'</b> to toggle.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				}
				break;
			}
			case "SHOWRIDS":
			{
				GameObject[] array18 = GameObject.FindGameObjectsWithTag("RoomID");
				GameObject[] array19 = array18;
				foreach (GameObject gameObject11 in array19)
				{
					gameObject11.GetComponentsInChildren<MeshRenderer>()[0].enabled = !gameObject11.GetComponentsInChildren<MeshRenderer>()[0].enabled;
					gameObject11.GetComponentsInChildren<MeshRenderer>()[1].enabled = !gameObject11.GetComponentsInChildren<MeshRenderer>()[1].enabled;
				}
				if (array18.Length > 0)
				{
					AddLog("Show RIDS: " + array18[0].GetComponentInChildren<MeshRenderer>().enabled, new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				}
				else
				{
					AddLog("There are no RIDS!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "FORCECLASS":
			case "FC":
			{
				int result5 = 0;
				if (array.Length >= 2 && int.TryParse(array[1], out result5))
				{
					string text6 = "offline";
					GameObject[] array14 = GameObject.FindGameObjectsWithTag("Player");
					GameObject[] array15 = array14;
					foreach (GameObject gameObject10 in array15)
					{
						if (!gameObject10.GetComponent<NetworkIdentity>().isLocalPlayer)
						{
							continue;
						}
						text6 = "online";
						CharacterClassManager component4 = gameObject10.GetComponent<CharacterClassManager>();
						if (component4 != null)
						{
							if (component4.klasy.Length > result5)
							{
								component4.CmdForceClass(result5, gameObject10);
								text6 = "none";
							}
							else
							{
								AddLog("Failed to force CLASS#" + result5.ToString("000") + " - class does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
						}
					}
					if (text6 == "offline" || text6 == "online")
					{
						AddLog((!(text6 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else
					{
						AddLog("Switched to CLASS#" + result5.ToString("000"), new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					}
				}
				else
				{
					AddLog("Second argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "CLASSLIST":
			{
				string text3 = "offline";
				GameObject[] array6 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array7 = array6;
				foreach (GameObject gameObject5 in array7)
				{
					int result3 = 1;
					if (array.Length >= 2 && !int.TryParse(array[1], out result3))
					{
						AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						return response;
					}
					if (!gameObject5.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						continue;
					}
					text3 = "online";
					CharacterClassManager component2 = gameObject5.GetComponent<CharacterClassManager>();
					if (!(component2 != null))
					{
						continue;
					}
					text3 = "none";
					if (result3 < 1)
					{
						AddLog("Page '" + result3 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						RefreshConsoleScreen();
						return response;
					}
					Class[] klasy = component2.klasy;
					for (int l = 10 * (result3 - 1); l < 10 * result3; l++)
					{
						if (10 * (result3 - 1) > klasy.Length)
						{
							AddLog("Page '" + result3 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							break;
						}
						if (l >= klasy.Length)
						{
							break;
						}
						AddLog("CLASS#" + l.ToString("000") + " : " + klasy[l].fullName, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
				}
				if (text3 != "none")
				{
					AddLog((!(text3 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "GOTO":
				if (array.Length >= 2)
				{
					GameObject gameObject6 = null;
					GameObject[] array8 = GameObject.FindGameObjectsWithTag("RoomID");
					GameObject[] array9 = array8;
					foreach (GameObject gameObject7 in array9)
					{
						if (gameObject7.GetComponent<Rid>().id.ToUpper() == array[1].ToUpper())
						{
							gameObject6 = gameObject7;
						}
					}
					string text4 = "offline";
					if (gameObject6 != null)
					{
						GameObject[] array10 = GameObject.FindGameObjectsWithTag("Player");
						GameObject[] array11 = array10;
						foreach (GameObject gameObject8 in array11)
						{
							if (gameObject8.GetComponent<NetworkIdentity>().isLocalPlayer)
							{
								if (array[1].ToUpper() == "RANGE" && !gameObject8.GetComponent<ShootingRange>().isOnRange)
								{
									text4 = "range";
									continue;
								}
								text4 = "none";
								gameObject8.transform.position = gameObject6.transform.position;
							}
						}
						if (text4 == "range")
						{
							AddLog("<b>Shooting range is disabled!</b>", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						}
						else if (text4 == "offline")
						{
							AddLog("You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						}
						else
						{
							AddLog("Teleported!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						}
					}
					else
					{
						AddLog("Room: <i>" + array[1].ToUpper() + "</i> not found!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
				}
				else
				{
					AddLog("Second argument is missing!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				RefreshConsoleScreen();
				break;
			case "RANGE":
			{
				string text9 = "offline";
				GameObject[] array21 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array22 = array21;
				foreach (GameObject gameObject14 in array22)
				{
					if (gameObject14.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						text9 = "online";
						ShootingRange component6 = gameObject14.GetComponent<ShootingRange>();
						if (component6 != null)
						{
							text9 = "none";
							component6.isOnRange = true;
						}
					}
				}
				if (text9 == "offline" || text9 == "online")
				{
					AddLog((!(text9 == "offline")) ? "Player range script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				else
				{
					AddLog("<b>Shooting range</b> is now available!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				}
				break;
			}
			case "ROUNDRESTART":
			{
				bool flag3 = false;
				GameObject[] array20 = GameObject.FindGameObjectsWithTag("Player");
				foreach (GameObject gameObject12 in array20)
				{
					PlayerStats component5 = gameObject12.GetComponent<PlayerStats>();
					if (component5.isLocalPlayer && component5.isServer)
					{
						flag3 = true;
						AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						component5.Roundrestart();
					}
				}
				if (!flag3)
				{
					AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "CONFIG":
				if (array.Length < 2)
				{
					TypeCommand("HELP CONFIG");
					break;
				}
				switch (array[1])
				{
				case "RELOAD":
				case "R":
				case "RLD":
					if (ConfigFile.singleton.ReloadConfig())
					{
						AddLog("Configuration file <b>successfully reloaded</b>. New settings will be applied on <b>your</b> server in <b>next</b> round.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						break;
					}
					AddLog("Configuration file reload <b>failed</b> - no such file - '<i>" + ConfigFile.path + "</i>'. Loading defult settings..", new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
					AddLog("Default settings have been loaded.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
					break;
				case "PATH":
					AddLog("Configuration file path: <i>" + ConfigFile.path + "</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					AddLog("<i>No visible drive letter means the root game directory.</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					break;
				case "VALUE":
					if (array.Length < 3)
					{
						AddLog("Please enter key name in the third argument. (CONFIG VALUE <i>KEYNAME</i>)", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
					}
					else
					{
						AddLog("The value of <i>'" + array[2] + "'</i> is: " + ConfigFile.GetString(array[2], "<color=ff0>DENIED: Entered key does not exists</color>"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
					break;
				}
				break;
			case "BAN":
				if (GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					if (array.Length < 3)
					{
						AddLog("Syntax: BAN [player kick / ip] [minutes]", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
						foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
						{
							string text = string.Empty;
							GameObject gameObject2 = FindConnectedRoot(connection);
							if (gameObject2 != null)
							{
								text = gameObject2.GetComponent<NicknameSync>().myNick;
							}
							if (text == string.Empty)
							{
								AddLog("Player :: " + connection.address, new Color32(160, 128, 128, byte.MaxValue), true);
							}
							else
							{
								AddLog("Player :: " + text + " :: " + connection.address, new Color32(128, 160, 128, byte.MaxValue), true);
							}
						}
						break;
					}
					int result = 0;
					if (int.TryParse(array[2], out result))
					{
						bool flag2 = false;
						foreach (NetworkConnectionToClient connection2 in NetworkServer.connections.Values)
						{
							GameObject gameObject3 = FindConnectedRoot(connection2);
							if (connection2.address.ToUpper().Contains(array[1]) || (gameObject3 != null && gameObject3.GetComponent<NicknameSync>().myNick.ToUpper().Contains(array[1])))
							{
								flag2 = true;
								BanPlayer.BanConnection(connection2, result);
								AddLog("Player banned.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
							}
						}
						if (!flag2)
						{
							AddLog("Player not found.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
						}
					}
					else
					{
						AddLog("Parse error: [minutes] - has to be an integer.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
					}
				}
				else
				{
					AddLog("You are not the owner!.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
				}
				break;
			case "BANREFRESH":
				if (GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					BanPlayer.ReloadBans();
				}
				else
				{
					AddLog("You are not the owner!.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
				}
				break;
			default:
				AddLog("Command " + cmd + " does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				break;
			}
			return response;
		}

		public void ProceedButton()
		{
			if (cmdField.text != string.Empty)
			{
				TypeCommand(cmdField.text);
			}
			cmdField.text = string.Empty;
			EventSystem.current.SetSelectedGameObject(cmdField.gameObject);
		}

		private void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				ProceedButton();
			}
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				ToggleConsole();
			}
			scrollup += Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel") * 10f);
			if (logs.Count > 0)
			{
				scrollup = Mathf.Clamp(scrollup, 0, logs.Count - 1);
			}
			else
			{
				scrollup = 0;
			}
			if (previous_scrlup != scrollup)
			{
				previous_scrlup = scrollup;
				RefreshConsoleScreen();
			}
			Scene activeScene = SceneManager.GetActiveScene();
			if (activeScene.name != loadedLevel)
			{
				loadedLevel = activeScene.name;
				AddLog("Scene Manager: Loaded scene '" + activeScene.name + "' [" + activeScene.path + "]", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				RefreshConsoleScreen();
			}
			if (allwaysRefreshing)
			{
				RefreshConsoleScreen();
			}
		}

		public void ToggleConsole()
		{
			CursorManager.consoleOpen = !console.activeSelf;
			cmdField.text = string.Empty;
			console.SetActive(!console.activeSelf);
			if (PlayerManager.singleton != null)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array2 = array;
				foreach (GameObject gameObject in array2)
				{
					if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						FirstPersonController component = gameObject.GetComponent<FirstPersonController>();
						if (component != null)
						{
							component.usingConsole = console.activeSelf;
						}
					}
				}
			}
			if (console.activeSelf)
			{
				EventSystem.current.SetSelectedGameObject(cmdField.gameObject);
			}
		}

		private void QuitGame()
		{
			Application.Quit();
		}
	}
}
