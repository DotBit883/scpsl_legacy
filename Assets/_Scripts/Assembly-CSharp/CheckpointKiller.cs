using UnityEngine;

public class CheckpointKiller : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		PlayerStats component = other.GetComponent<PlayerStats>();
		if (component != null && component.isLocalPlayer)
		{
			component.CmdHurtPlayer(new PlayerStats.HitInfo(999999f, "WORLD", "WALL"), component.gameObject);
		}
	}
}
