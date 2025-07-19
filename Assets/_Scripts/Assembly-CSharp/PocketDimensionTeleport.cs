using UnityEngine;
using Mirror;

public class PocketDimensionTeleport : MonoBehaviour
{
	public enum PDTeleportType
	{
		Killer = 0,
		Exit = 1
	}

	private PDTeleportType type;

	public void SetType(PDTeleportType t)
	{
		type = t;
	}

	private void OnTriggerEnter(Collider other)
	{
		NetworkIdentity component = other.GetComponent<NetworkIdentity>();
		if (component != null && component.isLocalPlayer)
		{
			if ((type == PDTeleportType.Killer || FindAnyObjectByType<BlastDoor>().isClosed) && !Input.GetKey(KeyCode.P))
			{
				component.GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(999999f, "WORLD", "POCKET"), component.gameObject);
			}
			else if (type == PDTeleportType.Exit)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("PD_EXIT");
				component.transform.position = array[Random.Range(0, array.Length)].transform.position;
			}
		}
	}
}
