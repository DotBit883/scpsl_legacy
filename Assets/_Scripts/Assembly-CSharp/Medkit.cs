using System;
using UnityEngine;
using Mirror;

public class Medkit : NetworkBehaviour
{
	[Serializable]
	public struct MedkitProperties
	{
		public int inventoryID;

		public float hpRegeneration;
	}

	private float inventoryCooldown;

	private Inventory inv;

	private PlayerStats ps;

	public MedkitProperties[] medkits;

	private static int kCmdCmdSetHpAmount;

	private void Start()
	{
		inv = GetComponent<Inventory>();
		ps = GetComponent<PlayerStats>();
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		inventoryCooldown -= Time.deltaTime;
		if (Cursor.lockState != CursorLockMode.Locked)
		{
			inventoryCooldown = 0.2f;
		}
		if (!(inventoryCooldown <= 0f) || !Input.GetButtonDown("Fire1"))
		{
			return;
		}
		MedkitProperties[] array = medkits;
		for (int i = 0; i < array.Length; i++)
		{
			MedkitProperties medkitProperties = array[i];
			if (inv.curItem == medkitProperties.inventoryID && ps.maxHP > ps.health)
			{
				inv.items.Remove(inv.localInventoryItem);
				int value = Mathf.RoundToInt((float)ps.health + medkitProperties.hpRegeneration);
				value = Mathf.Clamp(value, -1000, ps.maxHP);
				CmdSetHpAmount(value);
				inv.curItem = -1;
			}
		}
	}

	[Command(channel = 2)]
	private void CmdSetHpAmount(int am)
	{
		ps.health = am;
	}
}
