using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class LiftIdentity : NetworkBehaviour
{
	public Vector3 liftArea;

	public string identity;

	[Header("isSecond = top lift")]
	public bool isSecond;

	private bool isWorking;

	public ElevatorDoor up_d;

	public ElevatorDoor down_d;

	[SyncVar(hook = nameof(SetUp))]
	public bool isUp;

	private void Start()
	{
		if (!TutorialManager.status && isSecond)
		{
			StartCoroutine(Animation());
		}
	}

	public void SetUp(bool oldValue, bool newValue)
	{
		if (isSecond && !isWorking)
		{
			isWorking = true;
			StartCoroutine(Animation());
		}
		else
		{
			isUp = oldValue;
		}
	}

	public void Toggle()
	{
		isUp = !isUp;
	}

	private IEnumerator Animation()
	{
		up_d.SetOpen(false);
		down_d.SetOpen(false);
		yield return new WaitForSeconds(5f);
		ElevatorController[] ecs = Object.FindObjectsOfType<ElevatorController>();
		ElevatorController[] array = ecs;
		foreach (ElevatorController ec in array)
		{
			if (ec.isLocalPlayer)
			{
				ec.Teleport(identity);
				yield return new WaitForSeconds(1f);
				if (!ec.Teleport(identity, true))
				{
					ec.Teleport(identity);
				}
			}
		}
		up_d.SetOpen(isUp);
		down_d.SetOpen(!isUp);
		yield return new WaitForSeconds(2f);
		isWorking = false;
	}

	public bool InArea(Vector3 player)
	{
		Vector3 vector = player - GetComponentInParent<MeshCollider>().transform.position;
		if (Mathf.Abs(vector.x) < liftArea.x / 2f && Mathf.Abs(vector.z) < liftArea.z / 2f && Mathf.Abs(vector.y) < liftArea.y / 2f)
		{
			return true;
		}
		return false;
	}
}
