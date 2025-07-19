using UnityEngine;

public class Skybox : MonoBehaviour
{
	private Transform cam;

	private void Start()
	{
		cam = FindAnyObjectByType<SpectatorCamera>().cam.transform;
	}

	private void Update()
	{
		base.transform.position = new Vector3(cam.position.x, 1150f, cam.position.z);
	}
}
