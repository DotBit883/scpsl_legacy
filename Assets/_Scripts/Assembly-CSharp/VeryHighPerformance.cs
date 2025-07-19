using UnityEngine;

public class VeryHighPerformance : MonoBehaviour
{
	private void Start()
	{
		if (PlayerPrefs.GetInt("gfxsets_hp", 0) != 0)
		{
			Light[] array = Object.FindObjectsOfType<Light>();
			Light[] array2 = array;
			foreach (Light light in array2)
			{
				Object.Destroy(light.transform.gameObject);
			}
			RenderSettings.ambientEquatorColor = new Color(0.5f, 0.5f, 0.5f);
			RenderSettings.ambientGroundColor = new Color(0.5f, 0.5f, 0.5f);
			RenderSettings.ambientSkyColor = new Color(0.5f, 0.5f, 0.5f);
		}
	}
}
