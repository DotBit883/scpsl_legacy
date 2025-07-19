using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerStatic : MonoBehaviour
{
	public static bool isDedicated;

	public bool simulate;

	private bool processStarted;

	private void Awake()
	{
		processStarted = false;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string[] array = commandLineArgs;
		foreach (string text in array)
		{
			if (text == "-nographics" && !simulate)
			{
				simulate = true;
			}
		}
		if (simulate)
		{
			isDedicated = true;
			AudioListener.volume = 0f;
			string text2 = ConfigFile.GetString("console_panel_file", "DedicatedServer.exe");
			if (File.Exists(text2))
			{
				Process.Start(text2, "-unity");
			}
			else
			{
				Application.Quit();
			}
		}
	}

	private void Start()
	{
		if (isDedicated && SceneManager.GetActiveScene().buildIndex == 0)
		{
			GetComponent<CustomNetworkManager>().CreateMatch();
		}
	}
}
