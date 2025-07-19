using UnityEngine;
using Mirror;

public class AWSoundController : MonoBehaviour
{
	private AudioSource a_source;

	private AudioSource m_source;

	private AudioSource exp_src;

	public AudioClip inside;

	public AudioClip outside;

	private bool exploded;

	private void Awake()
	{
		a_source = GetComponents<AudioSource>()[0];
		m_source = GetComponents<AudioSource>()[1];
		exp_src = GetComponents<AudioSource>()[2];
	}

	public void UpdateSound(float t, bool p)
	{
		if (t == 90f)
		{
			if (p && !exploded)
			{
				exploded = true;
				GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
				foreach (GameObject gameObject in array)
				{
					if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						exp_src.PlayOneShot((!(gameObject.transform.position.y < 900f)) ? outside : inside);
						FindAnyObjectByType<ExplosionCameraShake>().Shake(2f);
					}
				}
			}
			if (a_source.isPlaying || m_source.isPlaying)
			{
				a_source.Stop();
				m_source.Stop();
			}
		}
		else
		{
			if (!a_source.isPlaying || !m_source.isPlaying)
			{
				a_source.Play();
				m_source.Play();
			}
			if (Mathf.Abs(a_source.time - t) > 0.5f)
			{
				a_source.time = t;
				m_source.time = t;
			}
		}
	}
}
