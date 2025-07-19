using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextLanguageReplacer : MonoBehaviour
{
	private MenuMusicManager mng;

	[SerializeField]
	[Multiline]
	public string polishVersion;

	[SerializeField]
	[Multiline]
	public string englishVersion;

	private void Awake()
	{
		if (GetComponent<TextMeshProUGUI>() != null)
		{
			GetComponent<TextMeshProUGUI>().text = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? englishVersion : polishVersion);
		}
		else
		{
			GetComponent<Text>().text = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? englishVersion : polishVersion);
		}
	}
}
