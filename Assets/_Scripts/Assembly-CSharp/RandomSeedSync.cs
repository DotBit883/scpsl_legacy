using System.Collections;
using GameConsole;
using UnityEngine;
using Mirror;

public class RandomSeedSync : NetworkBehaviour
{
	[SyncVar]
	public int seed = -1;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			seed = ConfigFile.GetInt("map_seed", -1);
			while (seed == -1)
			{
				seed = Random.Range(-999999999, 999999999);
			}
		}
		StartCoroutine(Generate());
	}

	private IEnumerator Generate()
	{
		while (!FindAnyObjectByType<RoomManager>().isGenerated)
		{
			if (base.name == "Host")
			{
				Console console = FindAnyObjectByType<Console>();
				console.AddLog("Initializing generator...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				FindAnyObjectByType<RoomManager>().GenerateMap(seed);
				if (!FindAnyObjectByType<RoomManager>().isGenerated)
				{
					console.AddLog("Map generator failure!", new Color32(byte.MaxValue, 0, 0, byte.MaxValue));
				}
				yield return new WaitForSeconds(1f);
				console.AddLog("Spawning items...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				if (base.isLocalPlayer)
				{
					GetComponent<HostItemSpawner>().Spawn(seed);
				}
				console.AddLog("The scene is ready! Good luck!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			}
			yield return new WaitForEndOfFrame();
		}
	}
}
