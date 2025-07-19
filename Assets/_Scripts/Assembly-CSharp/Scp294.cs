using UnityEngine;
using Mirror;

public class Scp294 : NetworkBehaviour
{
	public Transform position;

	private static int kCmdCmdSetPickup;

	public void Buy()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Pickup");
		GameObject gameObject = null;
		GameObject[] array2 = array;
		foreach (GameObject gameObject2 in array2)
		{
		}
		if (gameObject != null)
		{
			CmdSetPickup(gameObject.name, 18);
		}
	}

	[Command(channel = 2)]
	public void CmdSetPickup(string objname, int dropedItemID)
	{
		GameObject gameObject = GameObject.Find(objname);
		gameObject.GetComponent<Pickup>().durability = 0f;
		gameObject.GetComponent<Pickup>().id = dropedItemID;
		gameObject.GetComponent<Pickup>().pos = position.position;
		gameObject.GetComponent<Pickup>().rotation = position.rotation.eulerAngles;
	}
}
