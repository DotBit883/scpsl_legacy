using System.Collections;
using TMPro;
using UnityEngine;

public class SignBlink : MonoBehaviour
{
	public bool verticalText;

	private string startText;

	private const string alphabet = "QWERTYUIOPASDFGHJKLZXCVBNM01234567890!@#$%^&*()-_=+[]{}/<>";

	public void Play(int duration)
	{
		if (startText == string.Empty)
		{
			startText = GetComponent<TextMeshProUGUI>().text;
		}
		else
		{
			GetComponent<TextMeshProUGUI>().text = startText;
		}
		StartCoroutine(Blink((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? GetComponent<TextLanguageReplacer>().englishVersion : GetComponent<TextLanguageReplacer>().polishVersion, duration));
	}

	private IEnumerator Blink(string text, int iterations)
	{
		for (int i = 0; i < iterations; i++)
		{
			string s = string.Empty;
			for (int j = 0; j < text.Length; j++)
			{
				s += "QWERTYUIOPASDFGHJKLZXCVBNM01234567890!@#$%^&*()-_=+[]{}/<>"[Random.Range(0, "QWERTYUIOPASDFGHJKLZXCVBNM01234567890!@#$%^&*()-_=+[]{}/<>".Length)];
				if (j != text.Length - 1 && verticalText)
				{
					s += "\n";
				}
			}
			GetComponent<TextMeshProUGUI>().text = s;
			yield return new WaitForSeconds(0.02f);
		}
		GetComponent<TextMeshProUGUI>().text = text;
	}
}
