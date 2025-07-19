using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Inventory : NetworkBehaviour
{
	public Item[] availableItems;

	public List<Item> items = new List<Item>();

	private AnimationController ac;

	[SyncVar(hook = nameof(SetCurItem))]
	public int curItem;

	public GameObject kamera;

	public Item localInventoryItem;

	public GameObject pickupPrefab;

	private RawImage crosshair;

	private CharacterClassManager ccm;

	private int prevIt = -10;

	private void Awake()
	{
		for (int i = 0; i < availableItems.Length; i++)
		{
			availableItems[i].id = i;
		}
	}

	private void Log(string msg)
	{
	}

	public void SetCurItem(int oldCi, int newCi)
	{
		if (GetComponent<MicroHID_GFX>().onFire)
		{
			curItem = oldCi;
		}
	}

	private void Start()
	{
		if (base.isLocalPlayer && base.isServer)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("Pickup");
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				gameObject.GetComponent<Pickup>().iCanSeeThatAsHost = true;
			}
		}
		ccm = GetComponent<CharacterClassManager>();
		crosshair = GameObject.Find("CrosshairImage").GetComponent<RawImage>();
		ac = GetComponent<AnimationController>();
		if (base.isLocalPlayer)
		{
			FindAnyObjectByType<InventoryDisplay>().localplayer = base.gameObject;
		}
	}

	private void RefreshModels()
	{
		for (int i = 0; i < availableItems.Length; i++)
		{
			availableItems[i].firstpersonModel.SetActive(base.isLocalPlayer & (i == curItem));
		}
	}

	public void DropItem(int id)
	{
		if (base.isLocalPlayer)
		{
			if (items[id].id == curItem)
			{
				curItem = -1;
			}
			CmdSetPickup(items[id].id, items[id].durability, base.transform.position, kamera.transform.rotation, base.transform.rotation);
			items.RemoveAt(id);
		}
	}

	public void DropAll()
	{
		for (int i = 0; i < 20; i++)
		{
			if (items.Count > 0)
			{
				DropItem(0);
			}
		}
		AmmoBox component = GetComponent<AmmoBox>();
		for (int j = 0; j < component.types.Length; j++)
		{
			if (component.types[j].quantity > 0)
			{
				CmdSetPickup(component.types[j].inventoryID, component.types[j].quantity, base.transform.position, kamera.transform.rotation, base.transform.rotation);
				component.types[j].quantity = 0;
			}
		}
	}

	public void AddItem(int id, float dur = -4.6566467E+11f)
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (TutorialManager.status)
		{
			PickupTrigger[] array = Object.FindObjectsOfType<PickupTrigger>();
			PickupTrigger pickupTrigger = null;
			PickupTrigger[] array2 = array;
			foreach (PickupTrigger pickupTrigger2 in array2)
			{
				if ((pickupTrigger2.filter == -1 || pickupTrigger2.filter == id) && (pickupTrigger == null || pickupTrigger2.prioirty < pickupTrigger.prioirty))
				{
					pickupTrigger = pickupTrigger2;
				}
			}
			try
			{
				if (pickupTrigger != null)
				{
					pickupTrigger.Trigger(id);
				}
			}
			catch
			{
				MonoBehaviour.print("Error");
			}
		}
		Item item = new Item(availableItems[id]);
		if (GetComponent<Inventory>().items.Count < 8 || item.noEquipable)
		{
			if (dur != -4.6566467E+11f)
			{
				item.durability = dur;
			}
			items.Add(item);
		}
		else
		{
			GetComponent<Searching>().ShowErrorMessage();
		}
	}

	private void Update()
	{
		if (TutorialManager.status && !base.isLocalPlayer)
		{
			ac.SyncItem(curItem);
		}
		if (base.isLocalPlayer)
		{
			ac.SyncItem(curItem);
			int num = Mathf.Clamp(curItem, 0, availableItems.Length - 1);
			if (ccm.curClass >= 0 && ccm.klasy[ccm.curClass].forcedCrosshair != -1)
			{
				num = ccm.klasy[ccm.curClass].forcedCrosshair;
			}
			crosshair.texture = availableItems[num].crosshair;
			crosshair.color = availableItems[num].crosshairColor;
		}
		if (prevIt != curItem)
		{
			RefreshModels();
			prevIt = curItem;
		}
	}

	[Command(channel = 2)]
	public void CmdSetPickup(int dropedItemID, float dur, Vector3 pos, Quaternion camRot, Quaternion myRot)
	{
		GameObject gameObject = Object.Instantiate(pickupPrefab);
		NetworkServer.Spawn(gameObject);
		gameObject.GetComponent<Pickup>().durability = dur;
		gameObject.GetComponent<Pickup>().id = dropedItemID;
		gameObject.GetComponent<Pickup>().pos = pos + Vector3.up * 0.9f;
		gameObject.GetComponent<Pickup>().rotation = new Vector3(camRot.eulerAngles.x, myRot.eulerAngles.y, 0f);
		gameObject.GetComponent<Pickup>().myName = "PICKUP#" + dropedItemID + ":" + Random.Range(0f, 1E+10f).ToString("0000000000");
	}

	private void RegenerateHP(int count)
	{
		GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(-count, "WORLD", "MEDKIT"), base.gameObject);
	}
}
