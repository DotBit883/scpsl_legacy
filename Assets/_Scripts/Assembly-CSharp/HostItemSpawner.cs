using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HostItemSpawner : NetworkBehaviour
{
	private RandomItemSpawner ris;

	private Item[] avItems;

	private static int kCmdCmdSetPos;

	private void Start()
	{
		avItems = FindAnyObjectByType<Inventory>().availableItems;
	}

	public void Spawn(int seed)
	{
		Random.InitState(seed);
		string text = string.Empty;
		try
		{
			ris = FindAnyObjectByType<RandomItemSpawner>();
			RandomItemSpawner.PickupPositionRelation[] pickups = ris.pickups;
			List<RandomItemSpawner.PositionPosIdRelation> list = new List<RandomItemSpawner.PositionPosIdRelation>();
			text = "Rozpoczynianie dodawania pozycji";
			RandomItemSpawner.PositionPosIdRelation[] posIds = ris.posIds;
			foreach (RandomItemSpawner.PositionPosIdRelation item in posIds)
			{
				list.Add(item);
			}
			int num = 0;
			RandomItemSpawner.PickupPositionRelation[] array = pickups;
			foreach (RandomItemSpawner.PickupPositionRelation pickupPositionRelation in array)
			{
				for (int k = 0; k < list.Count; k++)
				{
					list[k].index = k;
				}
				List<RandomItemSpawner.PositionPosIdRelation> list2 = new List<RandomItemSpawner.PositionPosIdRelation>();
				foreach (RandomItemSpawner.PositionPosIdRelation item2 in list)
				{
					if (item2.posID == pickupPositionRelation.posID)
					{
						list2.Add(item2);
					}
				}
				text = "Setowanie rzeczy " + num;
				int index = Random.Range(0, list2.Count);
				RandomItemSpawner.PositionPosIdRelation positionPosIdRelation = list2[index];
				int index2 = positionPosIdRelation.index;
				CmdSetPos(pickupPositionRelation.pickup.gameObject, positionPosIdRelation.position.position, pickupPositionRelation.itemID, positionPosIdRelation.position.rotation.eulerAngles, (pickupPositionRelation.duration != -1) ? ((float)pickupPositionRelation.duration) : avItems[pickupPositionRelation.itemID].durability);
				list.RemoveAt(index2);
				num++;
			}
		}
		catch
		{
			Debug.LogError("Nie działa cos, lel" + text);
		}
	}

	[Command(channel = 2)]
	private void CmdSetPos(GameObject obj, Vector3 pos, int item, Vector3 rot, float dur)
	{
		Pickup component = obj.GetComponent<Pickup>();
		component.durability = dur;
		component.id = item;
		component.KeepRotation(2);
		if (rot == Vector3.zero)
		{
			component.rotation = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
		}
		else
		{
			component.rotation = rot;
		}
		component.pos = pos;
	}
}
