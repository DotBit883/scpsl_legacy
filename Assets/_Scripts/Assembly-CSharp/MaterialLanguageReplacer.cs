using UnityEngine;

public class MaterialLanguageReplacer : MonoBehaviour
{
	public Material polishVersion;

	public Material englishVersion;

	private void Start()
	{
		GetComponent<Renderer>().material = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? englishVersion : polishVersion);
	}
}
