using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
	public GameObject[] players;

	public static PlayerManager singleton;

	public static int playerID;

	private void Awake()
	{
		singleton = this;
	}

	public void AddPlayer(GameObject player)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = players;
		foreach (GameObject item in array)
		{
			list.Add(item);
		}
		if (!list.Contains(player))
		{
			list.Add(player);
		}
		players = list.ToArray();
	}

	public void RemovePlayer(GameObject player)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = players;
		foreach (GameObject item in array)
		{
			list.Add(item);
		}
		if (list.Contains(player))
		{
			list.Remove(player);
		}
		players = list.ToArray();
	}
}
