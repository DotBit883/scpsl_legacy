using System.Collections;
using TMPro;
using UnityEngine;

public class BreakingCardSFX : MonoBehaviour
{
	public string[] texts;

	private TextMeshProUGUI text;

	private void OnEnable()
	{
		StopAllCoroutines();
		StartCoroutine(DoAnimation());
	}

	private IEnumerator DoAnimation()
	{
		text = GetComponent<TextMeshProUGUI>();
		while (true)
		{
			string[] array = texts;
			foreach (string item in array)
			{
				try
				{
					text.text = item;
				}
				catch
				{
				}
				yield return new WaitForSeconds(1.3f);
			}
		}
	}
}
