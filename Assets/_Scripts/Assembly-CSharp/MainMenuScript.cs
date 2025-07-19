using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
	public GameObject[] submenus;

	private CustomNetworkManager mng;

	public int curMenu;

	private void Update()
	{
		if (SceneManager.GetActiveScene().buildIndex == 0)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public void SetLang(string lang)
	{
		string text = PlayerPrefs.GetString("langver");
		PlayerPrefs.SetString("langver", lang);
		if (lang != text)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
		}
		else
		{
			ChangeMenu(0);
		}
	}

	public void SetIP(string ip)
	{
		mng.networkAddress = ip;
	}

	public void ChangeMenu(int id)
	{
		curMenu = id;
		for (int i = 0; i < submenus.Length; i++)
		{
			submenus[i].SetActive(i == id);
		}
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	private void Start()
	{
		mng = FindAnyObjectByType<CustomNetworkManager>();
		CursorManager.UnsetAll();
		if (SteamManager.Initialized)
		{
			SteamUserStats.SetAchievement("TEST_1");
		}
		ChangeMenu(0);
	}

	public void StartServer()
	{
		mng.onlineScene = "Facility";
		mng.maxConnections = 20;
		mng.createpop.SetActive(true);
	}

	public void StartTutorial(string scene)
	{
		mng.onlineScene = scene;
		mng.maxConnections = 1;
		mng.ShowLog(15);
		mng.StartHost();
	}

	public void Connect()
	{
		mng.ShowLog(13);
		mng.StartClient();
	}
}
