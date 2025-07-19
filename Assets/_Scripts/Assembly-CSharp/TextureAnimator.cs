using System.Collections;
using UnityEngine;

public class TextureAnimator : MonoBehaviour
{
	public Material[] textures;

	public Renderer targetRenderer;

	public float cooldown;

	public Light optionalLight;

	public int lightRange;

	private void Start()
	{
		StartCoroutine(Animate());
	}

	private IEnumerator Animate()
	{
		while (true)
		{
			for (int i = 0; i < textures.Length; i++)
			{
				optionalLight.enabled = i < lightRange;
				targetRenderer.material = textures[i];
				yield return new WaitForSeconds(cooldown);
			}
		}
	}
}
