using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using GameConsole;
using UnityEngine;
using Mirror;

public class BanPlayer : NetworkBehaviour
{
	[Serializable]
	public class Ban
	{
		public string nick;

		public string hardware;

		public string ip;

		public string time;

		public Ban(string _nick, string _hardware, string _ip, string _time, bool _addToDatabase)
		{
			nick = _nick;
			hardware = _hardware;
			ip = _ip;
			time = _time;
			if (_addToDatabase)
			{
				try
				{
					string text = dbpath + "/" + _nick + Directory.GetFiles(dbpath, "*.ban", SearchOption.TopDirectoryOnly).Length + ".ban";
					MonoBehaviour.print(text);
					StreamWriter streamWriter = new StreamWriter(text);
					streamWriter.Write(_nick + Environment.NewLine + _hardware + Environment.NewLine + _ip + Environment.NewLine + _time);
					streamWriter.Close();
				}
				catch
				{
					GameConsole.Console.singleton.AddLog("Failed to ban: Database error - no such file.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
				}
			}
		}
	}

	[SyncVar]
	public string hardwareID;

	private static string dbpath;

	public static List<Ban> bans = new List<Ban>();

	private static int kCmdCmdSetHwId;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			if (base.isServer)
			{
				dbpath = ConfigFile.GetString("ban_database_folder", "[appdata]/SCP Secret Laboratory/Bans");
				dbpath = ((!dbpath.Contains("[appdata]")) ? dbpath : dbpath.Replace("[appdata]", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
				ReloadBans();
			}
			CmdSetHwId(SystemInfo.deviceUniqueIdentifier);
		}
	}

	[Command]
	private void CmdSetHwId(string s)
	{
		hardwareID = s;
	}

	public static void ReloadBans()
	{
		bans.Clear();
		try
		{
			if (!Directory.Exists(dbpath))
			{
				Directory.CreateDirectory(dbpath);
			}
			string[] files = Directory.GetFiles(dbpath, "*.ban", SearchOption.TopDirectoryOnly);
			string[] array = files;
			foreach (string path in array)
			{
				StreamReader streamReader = File.OpenText(path);
				string[] array2 = streamReader.ReadToEnd().Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
				streamReader.Close();
				bans.Add(new Ban(array2[0], array2[1], array2[2], array2[3], false));
			}
		}
		catch
		{
			GameConsole.Console.singleton.AddLog("Ban database directory incorrect.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
		}
	}

	public static void BanConnection(NetworkConnectionToClient conn, int duration)
	{
		GameObject gameObject = GameConsole.Console.FindConnectedRoot(conn);
		string nick = "MissingNick";
		string hardware = "MissingHardware";
		string address = conn.address;
		string time = DateTime.Now.AddMinutes(duration).ToString();
		if (gameObject != null)
		{
			nick = gameObject.GetComponent<NicknameSync>().myNick;
			hardware = gameObject.GetComponent<BanPlayer>().hardwareID;
		}
		bans.Add(new Ban(nick, hardware, address, time, true));
		conn.Disconnect();
	}

	public static bool NotExpired(string _time)
	{
		return Convert.ToDateTime(_time) > DateTime.Now;
	}
}
