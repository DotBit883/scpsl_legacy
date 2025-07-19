using System;
using UnityEngine;

[Serializable]
public class Item
{
	public string label;

	public string labelPL;

	[Multiline]
	public string description;

	[Multiline]
	public string descriptionPL;

	public Texture2D icon;

	public GameObject prefab;

	public float pickingtime = 1f;

	public Door.RoomAccessConditions optionalKeycardSetup;

	public GameObject firstpersonModel;

	public float durability;

	public bool noEquipable;

	public Texture crosshair;

	public Color crosshairColor;

	[HideInInspector]
	public int id;

	public Item(Item item)
	{
		label = item.label;
		labelPL = item.labelPL;
		description = item.description;
		descriptionPL = item.descriptionPL;
		icon = item.icon;
		prefab = item.prefab;
		pickingtime = item.pickingtime;
		optionalKeycardSetup = item.optionalKeycardSetup;
		firstpersonModel = item.firstpersonModel;
		durability = item.durability;
		id = item.id;
		crosshair = item.crosshair;
		crosshairColor = item.crosshairColor;
	}
}
