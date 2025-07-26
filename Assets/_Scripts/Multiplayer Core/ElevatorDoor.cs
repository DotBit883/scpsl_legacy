using UnityEngine;

public class ElevatorDoor : MonoBehaviour
{
	private bool isOpen;

	public Vector3 openPos;

	public Vector3 closePos;

	public void SetOpen(bool b)
	{
		if (b != isOpen)
		{
			GetComponent<AudioSource>().Play();
		}
		isOpen = b;
	}

	private void Update()
	{
		base.transform.localPosition = Vector3.LerpUnclamped(base.transform.localPosition, (!isOpen) ? closePos : openPos, Time.deltaTime * 3f);
	}
}
