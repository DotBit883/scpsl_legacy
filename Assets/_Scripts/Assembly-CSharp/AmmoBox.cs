using System;
using TMPro;
using UnityEngine;

public class AmmoBox : MonoBehaviour
{
	[Serializable]
	public class AmmoType
	{
		public string label;

		public int inventoryID;

		public int quantity;
	}

	private Inventory inv;

	private CharacterClassManager ccm;

	public TextMeshProUGUI text;

	public AmmoType[] types;

	public int greenValue;

	public int amountToDrop = 5;

	public int chosenID;

	private void Start()
	{
		inv = GetComponent<Inventory>();
		ccm = GetComponent<CharacterClassManager>();
	}

	private void Update()
	{
		UpdateText();
		if (inv.curItem != 19 || !(GetComponent<WeaponManager>().inventoryCooldown <= 0f))
		{
			return;
		}
		if (Input.GetButtonDown("Zoom"))
		{
			chosenID++;
			if (chosenID >= types.Length)
			{
				chosenID = 0;
			}
		}
		if (Input.GetButtonDown("Fire1") && types[chosenID].quantity >= amountToDrop && amountToDrop != 0)
		{
			types[chosenID].quantity -= amountToDrop;
			inv.CmdSetPickup(types[chosenID].inventoryID, amountToDrop, base.transform.position, inv.kamera.transform.rotation, base.transform.rotation);
		}
		if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
		{
			amountToDrop--;
		}
		if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
		{
			amountToDrop++;
		}
		int quantity = types[chosenID].quantity;
		if (quantity >= 15)
		{
			amountToDrop = Mathf.Clamp(amountToDrop, 15, quantity);
		}
		else
		{
			amountToDrop = 0;
		}
	}

	public void SetAmmoAmount()
	{
		for (int i = 0; i < 3; i++)
		{
			types[i].quantity = ccm.klasy[ccm.curClass].ammoTypes[i];
		}
	}

	private void UpdateText()
	{
		this.text.text = string.Empty;
		string text;
		for (int i = 0; i < types.Length; i++)
		{
			TextMeshProUGUI textMeshProUGUI = this.text;
			text = textMeshProUGUI.text;
			textMeshProUGUI.text = text + types[i].label + " " + ((types[i].quantity <= 999) ? ColorValue(types[i].quantity) : "999+") + ((i != chosenID) ? "\n" : " <\n");
		}
		TextMeshProUGUI textMeshProUGUI2 = this.text;
		text = textMeshProUGUI2.text;
		textMeshProUGUI2.text = text + "[" + amountToDrop + "]";
	}

	private string ColorValue(int ammo)
	{
		string text = "#ff0000";
		if (ammo > 0)
		{
			text = "#ffff00";
		}
		if (ammo > greenValue)
		{
			text = "#00ff00";
		}
		return "<color=" + text + ">" + ammo.ToString("000") + "</color>";
	}
}
