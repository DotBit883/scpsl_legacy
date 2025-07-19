using UnityEngine;
using Mirror;

public class FootstepSync : NetworkBehaviour
{
	private AnimationController controller;

	public string[] footstepSound;

	private void Start()
	{
		controller = GetComponent<AnimationController>();
	}

	public void SyncFoot()
	{
		if (base.isLocalPlayer)
		{
			CmdSyncFoot();
		}
	}

	[Command(channel = 1)]
	private void CmdSyncFoot()
	{
		RpcSyncFoot();
	}

	[ClientRpc(channel = 1)]
	private void RpcSyncFoot()
	{
		if (!base.isLocalPlayer)
		{
			controller.PlaySound(footstepSound[Random.Range(0, footstepSound.Length)], false);
		}
	}

	public void SyncJump(bool b)
	{
		if (base.isLocalPlayer)
		{
			CmdSyncJump(b);
		}
	}

	[Command(channel = 1)]
	public void CmdSyncJump(bool id)
	{
		RpcSyncJump(id);
	}

	[ClientRpc(channel = 1)]
	private void RpcSyncJump(bool id)
	{
		if (GetComponent<CharacterClassManager>().curClass != 2 && !base.isLocalPlayer)
		{
			controller.PlaySound((!id) ? 3 : 2, false);
		}
	}

	public void SyncNecksnap(bool b)
	{
		if (base.isLocalPlayer)
		{
			CmdSyncNecksnap();
		}
	}

	[Command(channel = 1)]
	public void CmdSyncNecksnap()
	{
		RpcSyncNecksnap();
	}

	[ClientRpc(channel = 1)]
	private void RpcSyncNecksnap()
	{
		if (!base.isLocalPlayer)
		{
			controller.PlaySound(4, false);
		}
	}
}
