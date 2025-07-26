using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
	[HideInInspector]
	public GameObject localplayer;

	public GameObject rootObject;

	public Texture2D blackTexture;

	public TextMeshProUGUI description;

	public Image[] itemslots;

	private List<Item> items = new List<Item>();

	public int hoveredID;

	public bool isSCP;

	public void SetDescriptionByID(int id)
	{
		if (id == -1)
		{
			hoveredID = -1;
			description.text = string.Empty;
		}
		else if (items.Count > id)
		{
			bool flag = PlayerPrefs.GetString("langver", "en") == "pl";
			string text = ((!flag) ? items[id].description : items[id].descriptionPL);
			string text2 = ((!flag) ? items[id].label : items[id].labelPL);
			description.text = text;
			hoveredID = id;
		}
		else
		{
			hoveredID = -1;
			description.text = string.Empty;
		}
	}

	private void Update()
	{
		if (localplayer == null)
		{
			return;
		}
		if (!rootObject.activeSelf)
		{
			hoveredID = -1;
		}
		items = localplayer.GetComponent<Inventory>().items;
		if (Input.GetButtonDown("Cancel") || isSCP)
		{
			rootObject.SetActive(false);
			hoveredID = -1;
			localplayer.GetComponent<FirstPersonController>().m_MouseLook.isOpenEq = rootObject.activeSelf;
			CursorManager.eqOpen = rootObject.activeSelf;
		}
		if (!isSCP && Input.GetButtonDown("Inventory") && !localplayer.GetComponent<MicroHID_GFX>().onFire && !CursorManager.pauseOpen)
		{
			hoveredID = -1;
			rootObject.SetActive(!rootObject.activeSelf);
			localplayer.GetComponent<FirstPersonController>().m_MouseLook.isOpenEq = rootObject.activeSelf;
			CursorManager.eqOpen = rootObject.activeSelf;
		}
		if (Input.GetKeyDown(KeyCode.Mouse1) && hoveredID >= 0 && rootObject.activeSelf)
		{
			localplayer.GetComponent<Inventory>().DropItem(hoveredID);
		}
		if (Input.GetKeyDown(KeyCode.Mouse0) && rootObject.activeSelf)
		{
			localplayer.GetComponent<Inventory>().localInventoryItem = ((hoveredID < 0) ? null : localplayer.GetComponent<Inventory>().items[hoveredID]);
			localplayer.GetComponent<Inventory>().curItem = ((hoveredID < 0) ? hoveredID : items[hoveredID].id);
			localplayer.GetComponent<FirstPersonController>().m_MouseLook.isOpenEq = false;
			CursorManager.eqOpen = false;
			rootObject.SetActive(false);
		}
		Image[] array = itemslots;
		foreach (Image image in array)
		{
			image.GetComponentInChildren<RawImage>().texture = blackTexture;
		}
		for (int num = itemslots.Length - 1; num >= 0; num--)
		{
			if (num < items.Count)
			{
				itemslots[num].GetComponentInChildren<RawImage>().texture = items[num].icon;
			}
		}
	}
}
