using System;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
	[Serializable]
	public class ResolutionPreset
	{
		public int width;

		public int height;

		public ResolutionPreset(Resolution template)
		{
			width = template.width;
			height = template.height;
		}

		public void SetResolution()
		{
			Screen.SetResolution(width, height, fullscreen);
		}
	}

	public static bool fullscreen;

	public static int preset;

	public static List<ResolutionPreset> presets = new List<ResolutionPreset>();

	private bool FindResolution(Resolution res)
	{
		foreach (ResolutionPreset preset in presets)
		{
			if (preset.height == res.height && preset.width == res.width)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		presets.Clear();
		Resolution[] resolutions = Screen.resolutions;
		foreach (Resolution resolution in resolutions)
		{
			if (!FindResolution(resolution))
			{
				presets.Add(new ResolutionPreset(resolution));
			}
		}
		preset = PlayerPrefs.GetInt("SavedResolutionSet", presets.Count - 1);
		fullscreen = PlayerPrefs.GetInt("SavedFullscreen", 1) != 0;
		RefreshScreen();
	}

	public static void RefreshScreen()
	{
		presets[preset].SetResolution();
		FindAnyObjectByType<ResolutionText>().txt.text = presets[preset].width + " × " + presets[preset].height;
	}

	public static void ChangeResolution(int id)
	{
		if (id == 0)
		{
			fullscreen = !fullscreen;
			PlayerPrefs.SetInt("SavedFullscreen", fullscreen ? 1 : 0);
		}
		else
		{
			preset = Mathf.Clamp(preset + id, 0, presets.Count - 1);
			PlayerPrefs.SetInt("SavedResolutionSet", preset);
		}
		RefreshScreen();
	}
}
