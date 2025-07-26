using UnityEngine;

public class TeslaTrigger : MonoBehaviour
{
	public float activeDistance = 5f;

	private PlayerManager pmng;

	private void Start()
	{
		pmng = PlayerManager.singleton;
	}

	private void Update()
	{
		GameObject[] players = pmng.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (Vector3.Distance(base.transform.position, gameObject.transform.position) <= activeDistance && gameObject.GetComponent<CharacterClassManager>().curClass != 2)
			{
				GetComponentInParent<TeslaGate>().Trigger(false, gameObject);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		GetComponentInParent<TeslaGate>().Trigger(true, other.gameObject);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
		Gizmos.DrawSphere(base.transform.position, activeDistance);
	}
}
