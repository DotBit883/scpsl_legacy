using System;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class GFXSettings : MonoBehaviour
{
	[Serializable]
	public class SliderValue
	{
		public string pl;

		public string en;

		public string Return()
		{
			return (!(PlayerPrefs.GetString("langver", "en") == "pl")) ? en : pl;
		}
	}

	public SliderValue[] pxlc_v;

	public SliderValue[] shadows_v;

	public SliderValue[] shadres_v;

	public SliderValue[] shaddis_v;

	public SliderValue[] vsync_v;

	public SliderValue[] aa_v;

	public SliderValue[] aocc_v;

	public SliderValue[] cc_v;

	public SliderValue[] hp_v;

	public Slider pxlc_slider;

	public Slider shadows_slider;

	public Slider shadres_slider;

	public Slider shaddis_slider;

	public Slider vsync_slider;

	public Slider aa_slider;

	public Slider aocc_slider;

	public Slider cc_slider;

	public Slider hp_slider;

	public Text pxlc_txt;

	public Text shadows_txt;

	public Text shadres_txt;

	public Text shaddis_txt;

	public Text vsync_txt;

	public Text aa_txt;

	public Text aocc_txt;

	public Text cc_txt;

	public Text hp_txt;

	private void Start()
	{
		LoadSavedSettings();
	}

	public void RefreshGUI()
	{
		if (!(pxlc_slider == null))
		{
			pxlc_slider.value = QualitySettings.pixelLightCount - 6;
			shadows_slider.value = (float)QualitySettings.shadows;
			shadres_slider.value = PlayerPrefs.GetInt("gfxsets_shadres", 2);
			shaddis_slider.value = (int)QualitySettings.shadowDistance - 25;
			vsync_slider.value = QualitySettings.vSyncCount;
			aa_slider.value = PlayerPrefs.GetInt("gfxsets_aa", 1);
			aocc_slider.value = PlayerPrefs.GetInt("gfxsets_aocc", 1);
			cc_slider.value = PlayerPrefs.GetInt("gfxsets_cc", 1);
			hp_slider.value = PlayerPrefs.GetInt("gfxsets_hp", 0);
			RefreshValues();
		}
	}

	public void RefreshValues()
	{
		pxlc_txt.text = pxlc_v[Mathf.RoundToInt(pxlc_slider.value)].Return();
		shadows_txt.text = shadows_v[Mathf.RoundToInt(shadows_slider.value)].Return();
		shadres_txt.text = shadres_v[Mathf.RoundToInt(shadres_slider.value)].Return();
		shaddis_txt.text = shaddis_v[Mathf.RoundToInt(shaddis_slider.value)].Return();
		vsync_txt.text = vsync_v[Mathf.RoundToInt(vsync_slider.value)].Return();
		aa_txt.text = aa_v[Mathf.RoundToInt(aa_slider.value)].Return();
		aocc_txt.text = aocc_v[Mathf.RoundToInt(aocc_slider.value)].Return();
		cc_txt.text = cc_v[Mathf.RoundToInt(cc_slider.value)].Return();
		hp_txt.text = hp_v[Mathf.RoundToInt(hp_slider.value)].Return();
	}

	public void SaveSettings()
	{
		PlayerPrefs.SetInt("gfxsets_pxlc", (int)pxlc_slider.value);
		PlayerPrefs.SetInt("gfxsets_shadows", (int)shadows_slider.value);
		PlayerPrefs.SetInt("gfxsets_shadres", (int)shadres_slider.value);
		PlayerPrefs.SetInt("gfxsets_shaddis", (int)shaddis_slider.value);
		PlayerPrefs.SetInt("gfxsets_vsync", (int)vsync_slider.value);
		PlayerPrefs.SetInt("gfxsets_aa", (int)aa_slider.value);
		PlayerPrefs.SetInt("gfxsets_aocc", (int)aocc_slider.value);
		PlayerPrefs.SetInt("gfxsets_cc", (int)cc_slider.value);
		PlayerPrefs.SetInt("gfxsets_hp", (int)hp_slider.value);
		LoadSavedSettings();
	}

	public void LoadSavedSettings()
	{
		QualitySettings.pixelLightCount = Mathf.Clamp(PlayerPrefs.GetInt("gfxsets_pxlc", 4) + 6, 6, 12);
		QualitySettings.shadows = (ShadowQuality)Mathf.Clamp(PlayerPrefs.GetInt("gfxsets_shadows", 3), 0, 3);
		QualitySettings.shadowResolution = (ShadowResolution)Mathf.Clamp(PlayerPrefs.GetInt("gfxsets_shadres", 2), 0, 3);
		QualitySettings.shadowDistance = Mathf.Clamp(PlayerPrefs.GetInt("gfxsets_shaddis", 10) + 25, 25, 50);
		QualitySettings.vSyncCount = Mathf.Clamp(PlayerPrefs.GetInt("gfxsets_vsync", 1), 0, 1);
		RefreshPPB();
		RefreshGUI();
	}

	private void RefreshPPB()
	{
		PostProcessingBehaviour[] array = UnityEngine.Object.FindObjectsOfType<PostProcessingBehaviour>();
		foreach (PostProcessingBehaviour postProcessingBehaviour in array)
		{
			postProcessingBehaviour.profile.antialiasing.enabled = PlayerPrefs.GetInt("gfxsets_aa", 1) == 1;
			postProcessingBehaviour.profile.ambientOcclusion.enabled = PlayerPrefs.GetInt("gfxsets_aocc", 1) == 1;
			postProcessingBehaviour.profile.bloom.enabled = PlayerPrefs.GetInt("gfxsets_cc", 1) == 1;
			postProcessingBehaviour.profile.colorGrading.enabled = PlayerPrefs.GetInt("gfxsets_cc", 1) == 1;
		}
	}
}
