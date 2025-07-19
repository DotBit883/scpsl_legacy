using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Searching : NetworkBehaviour
{
	private CharacterClassManager ccm;

	private Inventory inv;

	private bool isHuman;

	private GameObject pickup;

	private Transform cam;

	private FirstPersonController fpc;

	private AmmoBox ammobox;

	private float timeToPickUp;

	private float errorMsgDur;

	private GameObject overloaderror;

	private Slider progress;

	private GameObject progressGO;

	public float rayDistance;

	private void Start()
	{
		fpc = GetComponent<FirstPersonController>();
		cam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		ccm = GetComponent<CharacterClassManager>();
		inv = GetComponent<Inventory>();
		overloaderror = UserMainInterface.singleton.overloadMsg;
		progress = UserMainInterface.singleton.searchProgress;
		progressGO = UserMainInterface.singleton.searchOBJ;
		ammobox = GetComponent<AmmoBox>();
	}

	public void Init(bool isNotHuman)
	{
		isHuman = !isNotHuman;
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			Raycast();
			ContinuePickup();
			ErrorMessage();
		}
	}

	public void ShowErrorMessage()
	{
		errorMsgDur = 2f;
	}

	private void ErrorMessage()
	{
		if (errorMsgDur > 0f)
		{
			errorMsgDur -= Time.deltaTime;
		}
		overloaderror.SetActive(errorMsgDur > 0f);
	}

	private void ContinuePickup()
	{
		if (pickup != null)
		{
			if (!Input.GetButton("Interact"))
			{
				pickup = null;
				fpc.isSearching = false;
				progressGO.SetActive(false);
				return;
			}
			timeToPickUp -= Time.deltaTime;
			progressGO.SetActive(true);
			progress.value = progress.maxValue - timeToPickUp;
			if (timeToPickUp <= 0f)
			{
				progressGO.SetActive(false);
				CmdPickupItem(pickup, base.gameObject);
				fpc.isSearching = false;
				pickup = null;
			}
		}
		else
		{
			fpc.isSearching = false;
			progressGO.SetActive(false);
		}
	}

	private void Raycast()
	{
		RaycastHit hitInfo;
		if (!Input.GetButtonDown("Interact") || !AllowPickup() || !Physics.Raycast(new Ray(cam.position, cam.forward), out hitInfo, rayDistance, GetComponent<PlayerInteract>().mask))
		{
			return;
		}
		Pickup componentInParent = hitInfo.transform.GetComponentInParent<Pickup>();
		Locker componentInParent2 = hitInfo.transform.GetComponentInParent<Locker>();
		if (componentInParent != null)
		{
			if (inv.items.Count < 8 || inv.availableItems[componentInParent.id].noEquipable)
			{
				timeToPickUp = componentInParent.searchTime;
				progress.maxValue = componentInParent.searchTime;
				fpc.isSearching = true;
				pickup = componentInParent.gameObject;
			}
			else
			{
				ShowErrorMessage();
			}
		}
		if (componentInParent2 != null)
		{
			if (inv.items.Count < 8)
			{
				timeToPickUp = componentInParent2.searchTime;
				progress.maxValue = componentInParent2.searchTime;
				fpc.isSearching = true;
				pickup = componentInParent2.gameObject;
			}
			else
			{
				ShowErrorMessage();
			}
		}
	}

	private bool AllowPickup()
	{
		if (!isHuman)
		{
			return false;
		}
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<Handcuffs>().cuffTarget == base.gameObject)
			{
				return false;
			}
		}
		return true;
	}

	[Command(channel = 2)]
	private void CmdPickupItem(GameObject t, GameObject taker)
	{
		int id = 0;
		Pickup component = t.GetComponent<Pickup>();
		if (component != null)
		{
			id = component.id;
			component.PickupItem();
		}
		Locker component2 = t.GetComponent<Locker>();
		if (component2 != null)
		{
			id = component2.GetItem();
			component2.SetTaken(true);
		}
		RpcPickupItem(taker, id, (!(t.GetComponent<Pickup>() == null)) ? component.durability : (-1f));
	}

	[ClientRpc(channel = 2)]
	private void RpcPickupItem(GameObject who, int id, float dur)
	{
		if (!(who == null) && (!(who.GetComponent<Locker>() != null) || !who.GetComponent<Locker>().isTaken))
		{
			who.GetComponent<Searching>().AddItem(id, dur);
		}
	}

	public void AddItem(int id, float dur)
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (!inv.availableItems[id].noEquipable)
		{
			inv.AddItem(id, (dur != -1f) ? dur : inv.availableItems[id].durability);
			return;
		}
		AmmoBox.AmmoType[] types = ammobox.types;
		foreach (AmmoBox.AmmoType ammoType in types)
		{
			if (ammoType.inventoryID == id)
			{
				ammoType.quantity += (int)dur;
			}
		}
	}
}
