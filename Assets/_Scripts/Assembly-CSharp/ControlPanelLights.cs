using System.Collections;
using UnityEngine;

public class ControlPanelLights : MonoBehaviour
{
	public Texture[] emissions;

	public Material targetMat;

	private void Start()
	{
		StartCoroutine(Animate());
	}

	private IEnumerator Animate()
	{
		int l = emissions.Length;
		while (true)
		{
			targetMat.SetTexture("_EmissionMap", emissions[Random.Range(0, l)]);
			yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
		}
	}
}
