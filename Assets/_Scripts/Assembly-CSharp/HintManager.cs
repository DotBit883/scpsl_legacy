using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class HintManager : MonoBehaviour
{
	[Serializable]
	public class Hint
	{
		public Vector2 size_en;

		public Vector2 size_pl;

		[Multiline]
		public string content_en;

		[Multiline]
		public string content_pl;

		public string keyName;

		public float duration;
	}

	public static HintManager singleton;

	[SerializeField]
	private Image box;

	public Hint[] hints;

	public List<Hint> hintQueue = new List<Hint>();

	private void Awake()
	{
		singleton = this;
	}

	private void Start()
	{
		box.canvasRenderer.SetAlpha(0f);
		StartCoroutine(ShowHints());
	}

	private IEnumerator ShowHints()
	{
		bool usePL = PlayerPrefs.GetString("langver", "en") == "pl";
		while (true)
		{
			if (hintQueue.Count > 0)
			{
				box.GetComponentInChildren<Text>().text = ((!usePL) ? hintQueue[0].content_en : hintQueue[0].content_pl);
				box.GetComponent<RectTransform>().sizeDelta = ((!usePL) ? hintQueue[0].size_en : hintQueue[0].size_pl);
				CanvasRenderer cr = box.canvasRenderer;
				GetComponent<AudioSource>().Play();
				while (cr.GetAlpha() < 1f)
				{
					cr.SetAlpha(cr.GetAlpha() + Time.deltaTime * 5f);
					cr.GetComponentInChildren<Text>().canvasRenderer.SetAlpha(cr.GetAlpha());
					yield return new WaitForEndOfFrame();
				}
				float dur = hintQueue[0].duration;
				while (dur > 0f)
				{
					dur -= Time.deltaTime;
					float v3 = Mathf.Sin(dur * 4f) / 15f + 0.07f;
					box.color = new Color(box.color.r, v3, box.color.b, box.color.a);
					yield return new WaitForEndOfFrame();
				}
				while (cr.GetAlpha() > 0f)
				{
					cr.SetAlpha(cr.GetAlpha() - Time.deltaTime * 5f);
					cr.GetComponentInChildren<Text>().canvasRenderer.SetAlpha(cr.GetAlpha());
					yield return new WaitForEndOfFrame();
				}
				yield return new WaitForSeconds(0.3f);
				hintQueue.RemoveAt(0);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public void AddHint(int hintID)
	{
		if (!TutorialManager.status && PlayerPrefs.GetInt(hints[hintID].keyName, 0) == 0)
		{
			hintQueue.Add(hints[hintID]);
			PlayerPrefs.SetInt(hints[hintID].keyName, 1);
		}
	}
}
