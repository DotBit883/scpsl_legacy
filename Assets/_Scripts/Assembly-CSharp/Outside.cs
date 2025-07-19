using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Outside : MonoBehaviour
{
	private bool isOutside = true;

	private Transform listenerPos;

	private void Update()
	{
		if (listenerPos == null)
		{
			SpectatorCamera spectatorCamera = FindAnyObjectByType<SpectatorCamera>();
			if (spectatorCamera != null)
			{
				listenerPos = spectatorCamera.cam.transform;
			}
		}
		if (listenerPos.position.y > 900f && !isOutside)
		{
			isOutside = true;
			SetOutside(true);
		}
		if (listenerPos.position.y < 900f && isOutside)
		{
			isOutside = false;
			SetOutside(false);
		}
	}

	private void SetOutside(bool b)
	{
		if (GameObject.Find("Directional light") != null)
		{
			GameObject.Find("Directional light").GetComponent<Light>().enabled = b;
		}
		Camera[] componentsInChildren = GetComponentsInChildren<Camera>(true);
		foreach (Camera camera in componentsInChildren)
		{
			if (camera.farClipPlane == 600f || camera.farClipPlane == 47f)
			{
				camera.farClipPlane = ((!b) ? 47 : 600);
				camera.clearFlags = (b ? CameraClearFlags.Skybox : CameraClearFlags.Color);
			}
		}
		GlobalFog[] componentsInChildren2 = GetComponentsInChildren<GlobalFog>(true);
		foreach (GlobalFog globalFog in componentsInChildren2)
		{
			globalFog.startDistance = (b ? 50 : 0);
		}
	}
}
