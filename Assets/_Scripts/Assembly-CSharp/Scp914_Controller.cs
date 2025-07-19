using System;
using System.Collections;
using UnityEngine;
using Mirror;

public class Scp914_Controller : NetworkBehaviour
{
	[Serializable]
	public class SCP914Output
	{
		public int[] output0;

		public int[] output1;

		public int[] output2;

		public int[] output3;

		public int[] output4;
	}

	public SCP914Output[] outputs;

	private Item[] avItems;

	public void Refine(string label)
	{
		CmdRefine914(label);
		StartCoroutine(SetRandomResults());
	}

	private void Start()
	{
		avItems = FindAnyObjectByType<Inventory>().availableItems;
	}

	private IEnumerator SetRandomResults()
	{
		int state = FindAnyObjectByType<Scp914>().state;
		FindAnyObjectByType<Scp914Grabber>().GetComponent<BoxCollider>().isTrigger = true;
		yield return new WaitForSeconds(11f);
		Collider[] colliders = FindAnyObjectByType<Scp914Grabber>().observes.ToArray();
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders.Length == 0)
			{
				break;
			}
			if (colliders[i] != null && colliders[i].GetComponentInParent<Pickup>() != null)
			{
				int num = 0;
				if (state == 0)
				{
					int[] output = outputs[colliders[i].gameObject.GetComponentInParent<Pickup>().id].output0;
					num = output[UnityEngine.Random.Range(0, output.Length)];
				}
				if (state == 1)
				{
					int[] output2 = outputs[colliders[i].gameObject.GetComponentInParent<Pickup>().id].output1;
					num = output2[UnityEngine.Random.Range(0, output2.Length)];
				}
				if (state == 2)
				{
					int[] output3 = outputs[colliders[i].gameObject.GetComponentInParent<Pickup>().id].output2;
					num = output3[UnityEngine.Random.Range(0, output3.Length)];
				}
				if (state == 3)
				{
					int[] output4 = outputs[colliders[i].gameObject.GetComponentInParent<Pickup>().id].output3;
					num = output4[UnityEngine.Random.Range(0, output4.Length)];
				}
				if (state == 4)
				{
					int[] output5 = outputs[colliders[i].gameObject.GetComponentInParent<Pickup>().id].output4;
					num = output5[UnityEngine.Random.Range(0, output5.Length)];
				}
				if (num < 0)
				{
					CmdDestroyItem(colliders[i].name);
				}
				else
				{
					Vector3 position = FindAnyObjectByType<Scp914>().outputPlace.transform.position;
					position += new Vector3(UnityEngine.Random.Range(-0.7f, 0.7f), 0f, UnityEngine.Random.Range(-0.7f, 0.7f));
					CmdSetupPickup(colliders[i].name, num, position);
				}
			}
			if (!(colliders[i] != null) || colliders[i].tag == "Player")
			{
			}
			yield return new WaitForEndOfFrame();
			FindAnyObjectByType<Scp914Grabber>().observes.Clear();
			FindAnyObjectByType<Scp914Grabber>().GetComponent<BoxCollider>().isTrigger = false;
		}
	}

	[Command(channel = 2)]
	private void CmdSetupPickup(string label, int result, Vector3 pos)
	{
		GameObject gameObject = GameObject.Find(label);
		gameObject.GetComponent<Pickup>().durability = avItems[result].durability;
		gameObject.GetComponentInParent<Pickup>().id = result;
		gameObject.GetComponentInParent<Pickup>().pos = pos;
	}

	[Command(channel = 2)]
	private void CmdDestroyItem(string label)
	{
		if (TutorialManager.status)
		{
			FindAnyObjectByType<TutorialManager>().Invoke(nameof(TutorialManager.Tutorial3_KeycardBurnt), 3f);
		}
		GameObject gameObject = GameObject.Find(label);
		gameObject.GetComponentInParent<Pickup>().PickupItem();
	}

	[Command(channel = 2)]
	private void CmdRefine914(string label)
	{
		GameObject gameObject = GameObject.Find(label);
		gameObject.GetComponentInParent<Scp914>().Refine();
	}
}
