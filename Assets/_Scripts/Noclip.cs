using UnityEngine;

public class Noclip : MonoBehaviour
{
	private bool isOn;

	private bool activated;

	public GameObject cam;

	public void Switch()
	{
		isOn = !isOn;
	}

	private void Update()
	{
		if (isOn)
		{
			if (Input.GetKeyDown(KeyCode.N))
			{
				activated = !activated;
				GetComponent<FirstPersonController>().noclip = activated;
			}
			if (activated)
			{
				base.transform.position += cam.transform.forward * Input.GetAxis("Vertical") + cam.transform.right * Input.GetAxis("Horizontal");
			}
		}
	}

	public bool Get()
	{
		return isOn;
	}
}
