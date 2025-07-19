using UnityEngine;

public class RangeAudioSource : MonoBehaviour
{
	public int radius = 50;

	public float lerpSpeed = 1f;

	public AudioSource audioSource;

	private bool isInRange;

	private void Update()
	{
		if (Camera.current != null)
		{
			isInRange = Vector3.Distance(Camera.current.transform.position, base.transform.position) < (float)radius;
		}
		audioSource.volume += Time.deltaTime * lerpSpeed * (float)(isInRange ? 1 : (-1));
		audioSource.volume = Mathf.Clamp01(audioSource.volume);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, radius);
	}
}
