using System.Collections.Generic;
using UnityEngine;

public class Scp914Grabber : MonoBehaviour
{
	public List<Collider> observes = new List<Collider>();

	private void OnTriggerEnter(Collider other)
	{
		observes.Add(other);
	}

	private void OnTriggerExit(Collider other)
	{
		observes.Remove(other);
	}
}
