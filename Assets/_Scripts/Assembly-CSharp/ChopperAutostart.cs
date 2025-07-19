using UnityEngine;
using Mirror;

public class ChopperAutostart : NetworkBehaviour
{
	[SyncVar(hook = nameof(SetState))]
	public bool isLanded = true;

	public void SetState(bool oldValue, bool newValue)
	{
		RefreshState();
	}

	private void Start()
	{
		RefreshState();
	}

	private void RefreshState()
	{
		GetComponent<Animator>().SetBool("IsLanded", isLanded);
	}
}
