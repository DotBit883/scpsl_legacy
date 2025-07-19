using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class Pickup : NetworkBehaviour
{
	private Item[] avItems;

	[SyncVar(hook = nameof(SetID))]
	public int id;

	[SyncVar(hook = nameof(SetName))]
	public string myName;

	[SyncVar]
	public float durability;

	public float searchTime = 1f;

	[SyncVar(hook = nameof(SetPosition))]
	public Vector3 pos;

	[SyncVar(hook = nameof(SetRotation))]
	public Vector3 rotation;

	[HideInInspector]
	public bool iCanSeeThatAsHost;

	private GameObject myModel;

	public bool startItem = true;

	public void SetRotation(Vector3 oldRot, Vector3 newRot)
	{
		base.transform.rotation = Quaternion.Euler(newRot);
		RefreshPrefab();
	}

	public void KeepRotation(int seconds)
	{
		StartCoroutine(IKeepRotation(seconds));
	}

	public void KeepEverything()
	{
		StartCoroutine(IKeepEverything());
	}

	private IEnumerator IKeepRotation(int seconds)
	{
		for (int i = 0; i < seconds; i++)
		{
			base.transform.rotation = Quaternion.Euler(rotation);
			yield return new WaitForSeconds(1f);
		}
	}

	private IEnumerator IKeepEverything()
	{
		for (int i = 0; i < 10; i++)
		{
			base.transform.position = pos;
			base.transform.rotation = Quaternion.Euler(rotation);
			yield return new WaitForSeconds(1f);
		}
	}

	public void SetPosition(Vector3 oldPos, Vector3 newPos)
	{
		GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		base.transform.position = newPos;
		RefreshPrefab();
	}

	private void Start()
	{
		StartCoroutine(InstantiateScript());
		if (startItem)
		{
			StartCoroutine(IKeepEverything());
		}
	}

	private IEnumerator InstantiateScript()
	{
		while (!SetAvItems())
		{
			yield return new WaitForEndOfFrame();
		}
		InvokeRepeating("RefreshPrefab", 1f, 6f);
	}

	private bool SetAvItems()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("Player");
		if (gameObject != null)
		{
			avItems = gameObject.GetComponent<Inventory>().availableItems;
			return true;
		}
		return false;
	}

	private void RefreshPrefab()
	{
		SetAvItems();
		if (myModel != null)
		{
			Object.Destroy(myModel);
		}
		myModel = Object.Instantiate(avItems[id].prefab, base.transform);
		myModel.transform.localPosition = Vector3.zero;
		if (base.transform.position.y < -10000f)
		{
			base.transform.position = new Vector3(base.transform.position.x, pos.y, base.transform.position.z);
		}
		searchTime = avItems[id].pickingtime;
	}

	public void SetID(int oldId, int newId)
	{
		RefreshPrefab();
	}

	public void SetName(string oldValue, string newValue)
	{
		base.name = newValue;
	}

	public void PickupItem()
	{
		NetworkServer.Destroy(base.gameObject);
	}
}
