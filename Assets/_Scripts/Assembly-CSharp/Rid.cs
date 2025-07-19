using UnityEngine;

public class Rid : MonoBehaviour
{
	public string id;

	private void Start()
	{
		id = GetComponentInChildren<MeshRenderer>().material.mainTexture.name;
	}
}
