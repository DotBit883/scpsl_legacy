using System.Collections;
using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
	public GameObject kamera;

	public GameObject con1;

	public GameObject con2;

	public GameObject foc;

	public GameObject unfoc;

	private void Update()
	{
		bool flag = con1.activeSelf | con2.activeSelf | GetComponent<MainMenuScript>().submenus[6].activeSelf;
		kamera.transform.position = Vector3.Lerp(kamera.transform.position, (!flag) ? unfoc.transform.position : foc.transform.position, Time.deltaTime * 2f);
		kamera.transform.rotation = Quaternion.Lerp(kamera.transform.rotation, (!flag) ? unfoc.transform.rotation : foc.transform.rotation, Time.deltaTime);
	}

	private void Start()
	{
		StartCoroutine(Animate());
	}

	private IEnumerator Animate()
	{
		while (true)
		{
			int t = Random.Range(2, 5);
			SignBlink[] array = Object.FindObjectsOfType<SignBlink>();
			foreach (SignBlink signBlink in array)
			{
				signBlink.Play(t);
			}
			yield return new WaitForSeconds(Random.Range(3, 10));
		}
	}
}
