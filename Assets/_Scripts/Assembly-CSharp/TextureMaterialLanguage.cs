using UnityEngine;

public class TextureMaterialLanguage : MonoBehaviour
{
	public Texture polishVersion;

	public Texture englishVersion;

	public Material mat;

	private void Start()
	{
		mat.mainTexture = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? englishVersion : polishVersion);
	}
}
