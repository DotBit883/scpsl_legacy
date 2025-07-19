using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
	public static bool eqOpen;

	public static bool pauseOpen;

	public static bool isServerOnly;

	public static bool consoleOpen;

	public static bool is079;

	public static bool scp106;

	public static bool roundStarted;

	private void Start()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		UnsetAll();
	}

	private void LateUpdate()
	{
		bool flag = eqOpen | pauseOpen | isServerOnly | consoleOpen | is079 | scp106 | roundStarted;
		Cursor.lockState = ((!flag) ? CursorLockMode.Locked : CursorLockMode.None);
		Cursor.visible = flag;
	}

	public static void UnsetAll()
	{
		eqOpen = false;
		pauseOpen = false;
		isServerOnly = false;
		consoleOpen = false;
		is079 = false;
		scp106 = false;
		roundStarted = false;
	}
}
