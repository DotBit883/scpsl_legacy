using UnityEngine;
using Mirror;

public class GameMenu : MonoBehaviour
{
	public GameObject background;

	public GameObject main;

	public GameObject[] minors;

	private void Update()
	{
		if (Input.GetButtonDown("Cancel") && !CursorManager.eqOpen)
		{
			ToggleMenu();
		}
	}

	public void ToggleMenu()
	{
		GameObject[] array = minors;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(false);
			}
		}
		background.SetActive(!background.activeSelf);
		CursorManager.pauseOpen = background.activeSelf;
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array2 = players;
		foreach (GameObject gameObject2 in array2)
		{
			if (gameObject2.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				gameObject2.GetComponent<FirstPersonController>().isPaused = background.activeSelf;
			}
		}
		main.SetActive(true);
	}

	public void Disconnect()
	{
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				if (gameObject.GetComponent<NetworkIdentity>().isServer)
				{
					FindAnyObjectByType<NetworkManager>().StopHost();
				}
				else
				{
					FindAnyObjectByType<NetworkManager>().StopClient();
				}
			}
		}
	}
}
