using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public AudioSource main;

	public AudioSource queue;

	public AudioClip lower;

	public AudioClip upper;

	public AudioClip outside;

	public string level;

	private string prevLvl;

	private GameObject ply;

	public void SetPlayer(GameObject go)
	{
		ply = go;
	}

	private IEnumerator LoadNextTrack(AudioClip newClip)
	{
		float speed = 0.5f;
		queue.Stop();
		queue.clip = newClip;
		queue.volume = 0f;
		queue.Play();
		main.volume = 1f;
		while (main.volume != 0f)
		{
			main.volume -= Time.deltaTime * speed;
			queue.volume += Time.deltaTime * speed;
			yield return new WaitForEndOfFrame();
		}
		main.Stop();
		queue.Stop();
		main.clip = newClip;
		main.volume = 1f;
		queue.volume = 0f;
		main.Play();
		main.time = 1f / speed;
	}

	private void Update()
	{
		if (!(ply == null) && !TutorialManager.status)
		{
			AudioClip newClip;
			if (ply.transform.position.y < -500f)
			{
				newClip = lower;
				level = "LOWER";
			}
			else if (ply.transform.position.y < 500f)
			{
				newClip = upper;
				level = "UPPER";
			}
			else
			{
				newClip = outside;
				level = "OUTSIDE";
			}
			if (prevLvl != level)
			{
				prevLvl = level;
				StartCoroutine(LoadNextTrack(newClip));
			}
		}
	}
}
