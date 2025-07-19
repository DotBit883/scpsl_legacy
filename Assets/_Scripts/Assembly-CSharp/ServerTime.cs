using System.Runtime.InteropServices;
using GameConsole;
using UnityEngine;
using Mirror;

public class ServerTime : NetworkBehaviour
{
	[SyncVar]
	public int timeFromStartup;

	public static int time;

	private const int allowedDeviation = 2;

	private static int kCmdCmdSetTime;

	public int NetworktimeFromStartup
	{
		get
		{
			return timeFromStartup;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref timeFromStartup, 1u);
		}
	}

	public static bool CheckSynchronization(int myTime)
	{
		int num = Mathf.Abs(myTime - time);
		if (num > 2)
		{
			Console.singleton.AddLog("Damage sync error.", new Color32(byte.MaxValue, 200, 0, byte.MaxValue));
		}
		return num <= 2;
	}

	private void Update()
	{
		if (base.name == "Host")
		{
			time = timeFromStartup;
		}
	}

	private void Start()
	{
		if (base.isLocalPlayer && base.isServer)
		{
			InvokeRepeating("IncreaseTime", 1f, 1f);
		}
	}

	private void IncreaseTime()
	{
		TransmitData(timeFromStartup + 1);
	}

	[ClientCallback]
	private void TransmitData(int timeFromStartup)
	{
		if (NetworkClient.active)
		{
			CmdSetTime(timeFromStartup);
		}
	}

	[Command(channel = 12)]
	private void CmdSetTime(int t)
	{
		NetworktimeFromStartup = t;
	}
}
