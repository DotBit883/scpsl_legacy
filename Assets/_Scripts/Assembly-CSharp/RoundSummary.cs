using System;
using TMPro;
using UnityEngine;
using Mirror;

public class RoundSummary : NetworkBehaviour
{
	[Serializable]
	public class Summary
	{
		public int classD_escaped;

		public int classD_start;

		public int scientists_escaped;

		public int scientists_start;

		public int scp_frags;

		public int scp_start;

		public int scp_alive;

		public int scp_nozombies;

		public bool warheadDetonated;
	}

	public bool debugMode;

	private bool roundHasEnded;

	private PlayerManager pm;

	private CharacterClassManager ccm;

	public static RoundSummary host;

	public Summary summary;

	private int _ClassDs;

	private int _ChaosInsurgency;

	private int _MobileForces;

	private int _Spectators;

	private int _Scientists;

	private int _SCPs;

	private int _SCPsNozombies;

	private void Awake()
	{
		Radio.roundEnded = false;
	}

	private void Start()
	{
		pm = PlayerManager.singleton;
		ccm = GetComponent<CharacterClassManager>();
		InvokeRepeating("CheckForEnding", 12f, 3f);
	}

	private void RoundRestart()
	{
		bool flag = false;
		GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject gameObject in array)
		{
			PlayerStats component = gameObject.GetComponent<PlayerStats>();
			if (component.isLocalPlayer && component.isServer)
			{
				flag = true;
				GameConsole.Console.singleton.AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				component.Roundrestart();
			}
		}
		if (!flag)
		{
			GameConsole.Console.singleton.AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
		}
	}

	public void CheckForEnding()
	{
		if (!base.isLocalPlayer || !base.isServer || roundHasEnded || !ccm.roundStarted)
		{
			return;
		}
		_ClassDs = 0;
		_ChaosInsurgency = 0;
		_MobileForces = 0;
		_Spectators = 0;
		_Scientists = 0;
		_SCPs = 0;
		_SCPsNozombies = 0;
		GameObject[] players = pm.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.curClass < 0)
			{
				continue;
			}
			switch (component.klasy[component.curClass].team)
			{
			case Team.CDP:
				_ClassDs++;
				break;
			case Team.CHI:
				_ChaosInsurgency++;
				break;
			case Team.MTF:
				_MobileForces++;
				break;
			case Team.RIP:
				_Spectators++;
				break;
			case Team.RSC:
				_Scientists++;
				break;
			case Team.SCP:
				_SCPs++;
				if (component.curClass != 10)
				{
					_SCPsNozombies++;
				}
				break;
			}
		}
		int num = 0;
		if (_ClassDs > 0)
		{
			num++;
		}
		if (_MobileForces > 0 || _Scientists > 0)
		{
			num++;
		}
		if (_SCPs > 0)
		{
			num++;
		}
		if (_ChaosInsurgency > 0 && (_MobileForces > 0 || _Scientists > 0))
		{
			num = 3;
		}
		if (num <= 1 && players.Length >= 2)
		{
			roundHasEnded = true;
		}
		if (debugMode)
		{
			roundHasEnded = false;
		}
		if (roundHasEnded)
		{
			summary.classD_escaped += _ClassDs;
			summary.scientists_escaped += _Scientists;
			summary.scp_alive = _SCPs;
			summary.scp_nozombies = _SCPsNozombies;
			int num2 = ConfigFile.GetInt("auto_round_restart_time", 10);
			CmdSetSummary(summary, num2);
			Invoke("RoundRestart", num2);
		}
	}

	private void Update()
	{
		if (host == null)
		{
			GameObject gameObject = GameObject.Find("Host");
			if (gameObject != null)
			{
				host = gameObject.GetComponent<RoundSummary>();
			}
		}
	}

	[Command(channel = 7)]
	private void CmdSetSummary(Summary sum, int posttime)
	{
		RpcSetSummary(sum, posttime);
	}

	[ClientRpc(channel = 7)]
	public void RpcSetSummary(Summary sum, int posttime)
	{
		Radio.roundEnded = true;
		string empty = string.Empty;
		if (PlayerPrefs.GetString("langver", "en") == "pl")
		{
			string text = empty;
			empty = text + "<color=#ff0000>" + sum.classD_escaped + "/" + sum.classD_start + "</color> Personelu Klasy D uciekło z placówki\n";
			text = empty;
			empty = text + "<color=#ff0000>" + sum.scientists_escaped + "/" + sum.scientists_start + "</color> Naukowców ocalało\n";
			text = empty;
			empty = text + "<color=#ff0000>" + sum.scp_frags + "</color> Zabitych przez SCP\n";
			text = empty;
			empty = text + "<color=#ff0000>" + (sum.scp_start - sum.scp_nozombies) + "/" + sum.scp_start + "</color> Unieszkodliwionych podmiotów SCP\n";
			empty = empty + "Głowica Alfa: <color=#ff0000>" + ((!sum.warheadDetonated) ? "Nie została użyta" : "Zdetonowana") + "</color>\n\n";
			text = empty;
			empty = text + "Następna runda rozpocznie się w ciągu " + posttime + " sekund.";
		}
		else
		{
			string text = empty;
			empty = text + "<color=#ff0000>" + sum.classD_escaped + "/" + sum.classD_start + "</color> Class-D Personnel escaped\n";
			text = empty;
			empty = text + "<color=#ff0000>" + sum.scientists_escaped + "/" + sum.scientists_start + "</color> Scientists survived\n";
			text = empty;
			empty = text + "<color=#ff0000>" + sum.scp_frags + "</color> Killed by SCP\n";
			text = empty;
			empty = text + "<color=#ff0000>" + (sum.scp_start - sum.scp_alive) + "/" + sum.scp_start + "</color> Terminated SCP subjects\n";
			empty = empty + "Alpha Warhead: <color=#ff0000>" + ((!sum.warheadDetonated) ? "Unused" : "Detonated") + "</color>\n\n";
			text = empty;
			empty = text + "The next round will start within " + posttime + " seconds.";
		}
		GameObject gameObject = UserMainInterface.singleton.summary;
		gameObject.SetActive(true);
		TextMeshProUGUI component = GameObject.FindGameObjectWithTag("Summary").GetComponent<TextMeshProUGUI>();
		component.text = empty;
	}
}
