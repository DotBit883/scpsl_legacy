using System;
using UnityEngine;
using Mirror;

public class Ragdoll : NetworkBehaviour
{
	[Serializable]
	public struct Info
	{
		public string ownerHLAPI_id;

		public string steamClientName;

		public PlayerStats.HitInfo deathCause;

		public int charclass;

		public Info(string owner, string nick, PlayerStats.HitInfo info, int cc)
		{
			ownerHLAPI_id = owner;
			steamClientName = nick;
			charclass = cc;
			deathCause = info;
		}
	}

	[SyncVar]
	public Info owner;

	[SyncVar]
	public bool allowRecall;

	private void Start()
	{
		Invoke(nameof(Unfr), 0.1f);
	}

	private void Unfr()
	{
		GetComponent<Rigidbody>().isKinematic = false;
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.isKinematic = false;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = true;
		}
	}
}
