using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicSlider : MonoBehaviour
{
	public AudioMixer master;

	public Slider slider;

	public Text optionalValueText;

	public string keyName = "Volume";

	private void Start()
	{
		OnValueChanged(PlayerPrefs.GetInt(keyName, 0));
		slider.value = PlayerPrefs.GetInt(keyName, 0);
		master.SetFloat(keyName, PlayerPrefs.GetInt(keyName, 0));
		if (optionalValueText != null)
		{
			optionalValueText.text = PlayerPrefs.GetInt(keyName, 0) + " dB";
		}
	}

	public void OnValueChanged(float vol)
	{
		master.SetFloat(keyName, vol);
		PlayerPrefs.SetInt(keyName, (int)vol);
		if (optionalValueText != null)
		{
			optionalValueText.text = (int)vol/*cast due to .constrained prefix*/ + " dB";
		}
	}
}
