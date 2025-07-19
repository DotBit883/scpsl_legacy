using System;
using UnityEngine;
using UnityEngine.UI;

public class Interface079 : MonoBehaviour
{
	private float progress;

	public Image progress_img;

	public GameObject progress_obj;

	public Text infoText;

	public Image fovSlider;

	public GameObject console;

	public Text consoleScreen;

	public InputField field;

	public GameObject liftButton;

	public GameObject teslaButton;

	[HideInInspector]
	public GameObject localplayer;

	private int curScreen;

	public float ability;

	private void Update()
	{
		teslaButton.GetComponent<Button>().interactable = ability >= 40f;
		if (ability < 200f)
		{
			ability += Time.deltaTime;
		}
		DateTime now = DateTime.Now;
		infoText.text = "CAM ▪ " + Screen.width + "x" + Screen.height + " | " + now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + " ▪ " + now.Month + "." + now.Day + ".20▮▮\n" + ((PlayerPrefs.GetString("langver") == "pl") ? ((!(ability > 40f)) ? "UMIEJĘTNOŚĆ NIEGOTOWA" : "UMIEJĘTNOŚĆ GOTOWA") : ((!(ability > 40f)) ? "ABILITY NOT READY" : "ABILITY READY"));
	}

	public void SetProgress(float f)
	{
		progress = f;
		RefreshProgress();
	}

	public void ResetProgress()
	{
		progress = 0f;
		RefreshProgress();
	}

	public void AddProgress(float f)
	{
		progress += f;
		RefreshProgress();
	}

	public float GetProgress()
	{
		return ClampProgress();
	}

	public bool Action()
	{
		return ClampProgress() == 1f;
	}

	private float ClampProgress()
	{
		progress = Mathf.Clamp01(progress);
		return progress;
	}

	public void RefreshProgress()
	{
		progress_img.fillAmount = ClampProgress();
	}

	public void SetConsoleScreen(int id)
	{
		curScreen = id;
	}

	public void UseElevator()
	{
		localplayer.GetComponent<Scp079PlayerScript>().UseElevator();
	}

	public void UseTesla()
	{
		ability = 0f;
		localplayer.GetComponent<Scp079PlayerScript>().UseTesla();
	}
}
