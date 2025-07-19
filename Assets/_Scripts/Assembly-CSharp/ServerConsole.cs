using System.Collections;
using System.IO;
using GameConsole;
using UnityEngine;

public class ServerConsole : MonoBehaviour
{
	public static int logID;

	private static IEnumerator CheckLog()
	{
		while (true)
		{
			string[] tasks = Directory.GetFiles("SCPSL_Data/Dedicated", "cs*.mapi", SearchOption.TopDirectoryOnly);
			string[] array = tasks;
			foreach (string text in array)
			{
				string text2 = string.Empty;
				string text3 = text.Remove(0, text.LastIndexOf("cs"));
				try
				{
					StreamReader streamReader = new StreamReader("SCPSL_Data/Dedicated/" + text3);
					string text4 = streamReader.ReadToEnd();
					if (text4.Contains("terminator"))
					{
						text4 = text4.Remove(text4.IndexOf("terminator"));
					}
					text2 = EnterCommand(text4);
					streamReader.Close();
					File.Delete("SCPSL_Data/Dedicated/" + text3);
				}
				catch
				{
					try
					{
						File.Delete("SCPSL_Data/Dedicated/" + text3);
					}
					catch
					{
						text2 += " | File not deleted!";
					}
				}
				if (string.IsNullOrEmpty(text2))
				{
					text2 = "ServerConsole reader - unknown error!";
				}
				AddLog(text2);
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public static void AddLog(string q)
	{
		StreamWriter streamWriter = new StreamWriter("SCPSL_Data/Dedicated/sl" + logID + ".mapi");
		logID++;
		streamWriter.WriteLine(q);
		streamWriter.Close();
	}

	private static string EnterCommand(string cmd)
	{
		string result = "Command accepted.";
		try
		{
			string[] array = cmd.ToUpper().Split(' ');
			if (array.Length > 0)
			{
				cmd = array[0];
				switch (cmd)
				{
				case "CONSOLE":
					if (array.Length > 1)
					{
						string text = string.Empty;
						for (int i = 0; i < array.Length; i++)
						{
							if (i != 0)
							{
								text = text + array[i] + " ";
							}
						}
						text = text.Remove(text.Length - 1);
						result = Console.singleton.TypeCommand(text);
					}
					else
					{
						result = "Please enter console command.";
					}
					break;
				case "STOP":
					result = "Server Stopped.";
					TerminateProcess();
					break;
				case "FORCESTART":
				{
					bool flag = false;
					GameObject gameObject = GameObject.Find("Host");
					if (gameObject != null)
					{
						CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
						if (component != null && component.isLocalPlayer && component.isServer && !component.roundStarted)
						{
							component.ForceRoundStart();
							flag = true;
						}
					}
					result = ((!flag) ? "Failed to force start" : "Forced round start.");
					break;
				}
				case "CONFIG":
					if (File.Exists(ConfigFile.path))
					{
						Application.OpenURL(ConfigFile.path);
					}
					else
					{
						result = "Config file not found!";
					}
					break;
				default:
					result = "Unknown command";
					break;
				}
			}
			else
			{
				result = "Syntax - cmd [command]";
			}
		}
		catch
		{
			result = "Command refused - unknown error.";
		}
		return result;
	}

	private void Start()
	{
		if (ServerStatic.isDedicated)
		{
			logID = 0;
			StartCoroutine(CheckLog());
		}
	}

	private static void TerminateProcess()
	{
		ServerStatic.isDedicated = false;
		Application.Quit();
	}
}
