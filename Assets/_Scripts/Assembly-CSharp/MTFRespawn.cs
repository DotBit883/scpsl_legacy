using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using Mirror;

public class MTFRespawn : NetworkBehaviour
{
	public GameObject ciTheme;

	private ChopperAutostart mtf_a;

	[Range(30f, 1000f)]
	public int minMtfTimeToRespawn = 200;

	[Range(40f, 1200f)]
	public int maxMtfTimeToRespawn = 400;

	public float CI_Time_Multiplier = 2f;

	public float CI_Percent = 20f;

	[Range(2f, 15f)]
	[Space(10f)]
	public int maxRespawnAmount = 15;

	[Range(1f, 10f)]
	public int minRespawnAmount = 1;

	public float timeToNextRespawn;

	public bool nextWaveIsCI;

	private List<GameObject> playersReadyToSpawn = new List<GameObject>();

	public List<GameObject> playersToNTF = new List<GameObject>();

	private bool loaded;

	[SyncVar]
	private int serverTimeToSpawn;

	public static int spectatorTimeToSpawn;

	[Command(channel = 2)]
	private void CmdSetServerTime(int t)
	{
		serverTimeToSpawn = t;
	}

	private void Start()
	{
		minMtfTimeToRespawn = ConfigFile.GetInt("minimum_MTF_time_to_spawn", 200);
		maxMtfTimeToRespawn = ConfigFile.GetInt("maximum_MTF_time_to_spawn", 400);
		CI_Percent = ConfigFile.GetInt("ci_respawn_percent", 35);
	}

	private void Update()
	{
		if (base.name == "Host")
		{
			spectatorTimeToSpawn = serverTimeToSpawn;
		}
		if (base.name != "Host" || !base.isLocalPlayer)
		{
			return;
		}
		if (mtf_a == null)
		{
			mtf_a = FindAnyObjectByType<ChopperAutostart>();
		}
		timeToNextRespawn -= Time.deltaTime;
		if (timeToNextRespawn < 18f && !loaded)
		{
			playersReadyToSpawn.Clear();
			loaded = true;
			GameObject[] players = PlayerManager.singleton.players;
			GameObject[] array = players;
			foreach (GameObject gameObject in array)
			{
				if (gameObject.GetComponent<CharacterClassManager>().curClass == 2)
				{
					playersReadyToSpawn.Add(gameObject);
				}
			}
			if (playersReadyToSpawn.Count >= minRespawnAmount)
			{
				if (nextWaveIsCI)
				{
					CmdVan();
				}
				else
				{
					CmdChopper(true);
				}
			}
		}
		if (timeToNextRespawn < 0f)
		{
			loaded = false;
			if (GetComponent<CharacterClassManager>().roundStarted)
			{
				CmdChopper(false);
			}
			RespawnDeadPlayers();
			nextWaveIsCI = (float)Random.Range(0, 100) <= CI_Percent;
			timeToNextRespawn = (float)Random.Range(minMtfTimeToRespawn, maxMtfTimeToRespawn) * ((!nextWaveIsCI) ? 1f : (1f / CI_Time_Multiplier));
			if (nextWaveIsCI)
			{
				CmdSetServerTime(-(ServerTime.time + (int)timeToNextRespawn));
			}
			else
			{
				CmdSetServerTime(ServerTime.time + (int)timeToNextRespawn);
			}
		}
	}

	private void RespawnDeadPlayers()
	{
		if (playersReadyToSpawn.Count >= minRespawnAmount)
		{
			if (!nextWaveIsCI)
			{
				CmdAnnonc();
			}
			else
			{
				Invoke("CmdDelayCIAnnounc", 1f);
			}
			while (playersReadyToSpawn.Count > maxRespawnAmount)
			{
				playersReadyToSpawn.RemoveAt(Random.Range(0, playersReadyToSpawn.Count));
			}
			for (int i = 0; i < playersReadyToSpawn.Count; i++)
			{
				if (!(playersReadyToSpawn[i] != null))
				{
					continue;
				}
				CharacterClassManager component = playersReadyToSpawn[i].GetComponent<CharacterClassManager>();
				if (component.curClass == 2)
				{
					if (nextWaveIsCI)
					{
						CmdSetClass(component.gameObject, 8);
					}
					else
					{
						playersToNTF.Add(playersReadyToSpawn[i]);
					}
				}
			}
			SummonNTF();
		}
		playersReadyToSpawn.Clear();
		nextWaveIsCI = false;
	}

	public void SummonNTF()
	{
		if (playersToNTF.Count <= 0)
		{
			return;
		}
		CmdSetUnit(playersToNTF.ToArray());
		for (int i = 0; i < playersToNTF.Count; i++)
		{
			if (i == 0)
			{
				CmdSetClass(playersToNTF[i], 12);
			}
			else if (i <= 3)
			{
				CmdSetClass(playersToNTF[i], 11);
			}
			else
			{
				CmdSetClass(playersToNTF[i], 13);
			}
		}
		playersToNTF.Clear();
	}

	[Command(channel = 2)]
	private void CmdSetClass(GameObject ply, int id)
	{
		CharacterClassManager component = ply.GetComponent<CharacterClassManager>();
		component.curClass = id;
	}

	[Command(channel = 13)]
	private void CmdSetUnit(GameObject[] ply)
	{
		int unit = GetComponent<NineTailedFoxUnits>().NewName();
		foreach (GameObject gameObject in ply)
		{
			gameObject.GetComponent<CharacterClassManager>().ntfUnit = unit;
		}
	}

	[Command(channel = 2)]
	private void CmdChopper(bool state)
	{
		mtf_a.isLanded = state;
	}

	[Command(channel = 2)]
	private void CmdVan()
	{
		RpcVan();
	}

	[ClientRpc(channel = 2)]
	private void RpcVan()
	{
		GameObject.Find("CIVanArrive").GetComponent<Animator>().SetTrigger("Arrive");
	}

	private void CmdDelayCIAnnounc()
	{
		CmdAnnoncCI();
	}

	[Command(channel = 2)]
	private void CmdAnnonc()
	{
		RpcAnnounc();
	}

	[ClientRpc(channel = 2)]
	private void RpcAnnounc()
	{
		GameObject.Find("MTF_Announc").GetComponent<AudioSource>().Play();
	}

	[Command(channel = 2)]
	private void CmdAnnoncCI()
	{
		RpcAnnouncCI();
	}

	[ClientRpc(channel = 2)]
	private void RpcAnnouncCI()
	{
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.isLocalPlayer)
			{
				Team team = component.klasy[component.curClass].team;
				if (team == Team.CDP || team == Team.CHI)
				{
					Object.Instantiate(ciTheme);
				}
			}
		}
	}
}
