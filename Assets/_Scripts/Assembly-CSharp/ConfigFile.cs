using System;
using System.IO;
using GameConsole;
using UnityEngine;

public class ConfigFile : MonoBehaviour
{
	public static ConfigFile singleton;

	public static string path;

	public string cfg;

	private void Awake()
	{
		singleton = this;
		path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory";
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
		catch
		{
			GameConsole.Console.singleton.AddLog("Configuration file directory creation failed.", new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
		}
		path += "/config.txt";
	}

	private void Start()
	{
		if (!ReloadConfig())
		{
			GameConsole.Console.singleton.AddLog("Configuration file could not be loaded - template not found! Loading default settings..", new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
			GameConsole.Console.singleton.AddLog("Default settings have been loaded.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
		}
	}

	public bool ReloadConfig()
	{
		if (!File.Exists(path))
		{
			try
			{
				File.Copy("config_template.txt", path);
			}
			catch
			{
				return false;
			}
		}
		StreamReader streamReader = new StreamReader(path);
		cfg = streamReader.ReadToEnd();
		streamReader.Close();
		return true;
	}

	public static string GetString(string key, string defaultValue = "")
	{
		string text = singleton.cfg;
		try
		{
			text = text.Remove(0, text.IndexOf(key));
			text = text.Remove(0, text.IndexOf("=") + 1);
			text = RemoveSpacesBefore(text);
			return text.Remove(text.IndexOf(";"));
		}
		catch
		{
			return defaultValue;
		}
	}

	private static string RemoveSpacesBefore(string s)
	{
		if (s[0].ToString() == " ")
		{
			s = s.Remove(0, 1);
			RemoveSpacesBefore(s);
		}
		return s;
	}

	public static int GetInt(string key, int defaultValue = 0)
	{
		int result = 0;
		if (int.TryParse(GetString(key, "errorInConverting"), out result))
		{
			return result;
		}
		return defaultValue;
	}
}
