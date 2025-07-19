using UnityEngine;
using Mirror;

public class ElevatorController : NetworkBehaviour
{
	public bool Teleport(string id, bool onlyCheck = false)
	{
		LiftIdentity liftIdentity = null;
		LiftIdentity liftIdentity2 = null;
		LiftIdentity[] array = Object.FindObjectsOfType<LiftIdentity>();
		LiftIdentity[] array2 = array;
		foreach (LiftIdentity liftIdentity3 in array2)
		{
			if (liftIdentity3.InArea(base.transform.position) && liftIdentity3.identity == id)
			{
				liftIdentity = liftIdentity3;
			}
		}
		if (liftIdentity != null)
		{
			if (!onlyCheck)
			{
				LiftIdentity[] array3 = array;
				foreach (LiftIdentity liftIdentity4 in array3)
				{
					if (liftIdentity4.identity == liftIdentity.identity && liftIdentity4.isSecond != liftIdentity.isSecond)
					{
						liftIdentity2 = liftIdentity4;
					}
				}
				base.transform.SetParent(liftIdentity.GetComponentInParent<MeshCollider>().transform);
				Vector3 localPosition = base.transform.localPosition;
				Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
				base.transform.SetParent(liftIdentity2.GetComponentInParent<MeshCollider>().transform);
				base.transform.localPosition = localPosition;
				base.transform.GetComponentInParent<FirstPersonController>().m_MouseLook.SetRotation(Quaternion.Euler(new Vector3(0f, liftIdentity2.GetComponentInParent<MeshCollider>().transform.rotation.eulerAngles.y - liftIdentity.GetComponentInParent<MeshCollider>().transform.rotation.eulerAngles.y, 0f)));
				base.transform.parent = null;
			}
			else
			{
				LiftIdentity liftIdentity5 = null;
				LiftIdentity[] array4 = array;
				foreach (LiftIdentity liftIdentity6 in array4)
				{
					if (liftIdentity6.identity == liftIdentity.identity && liftIdentity6.isSecond)
					{
						liftIdentity5 = liftIdentity6;
					}
				}
				if (liftIdentity.isSecond != liftIdentity.isUp)
				{
					return false;
				}
			}
		}
		return true;
	}
}
