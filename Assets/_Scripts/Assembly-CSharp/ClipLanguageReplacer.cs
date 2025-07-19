using UnityEngine;

public class ClipLanguageReplacer : MonoBehaviour
{
	[SerializeField]
	public AudioClip polishVersion;

	[SerializeField]
	public AudioClip englishVersion;

	private void Awake()
	{
		AudioSource component = GetComponent<AudioSource>();
		component.clip = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? englishVersion : polishVersion);
		if (component.playOnAwake)
		{
			component.Stop();
			component.Play();
		}
	}
}
