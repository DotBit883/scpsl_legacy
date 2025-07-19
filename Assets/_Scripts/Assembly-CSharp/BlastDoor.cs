using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class BlastDoor : NetworkBehaviour
{
	[SyncVar(hook = nameof(SetClosed))]
	public bool isClosed;

	public void SetClosed(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			GetComponent<Animator>().SetTrigger("Close");
		}
	}
}
