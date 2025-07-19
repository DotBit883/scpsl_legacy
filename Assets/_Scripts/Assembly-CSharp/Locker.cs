using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class Locker : NetworkBehaviour
{
	public Vector3 localPos;

	public float searchTime;

	public int[] ids;

	[SyncVar]
	public bool isTaken;

	public int GetItem()
	{
		return (!isTaken) ? ids[Random.Range(0, ids.Length)] : (-1);
	}

	public void SetTaken(bool b)
	{
		isTaken = b;
	}

	public void SetupPos()
	{
		localPos = base.transform.localPosition;
	}

	public void Update()
	{
		base.transform.localPosition = localPos;
	}
}
