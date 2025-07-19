using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class RoundStart : NetworkBehaviour
{
	[SyncVar]
	public string info = string.Empty;

	public static RoundStart singleton;

	public GameObject window;

	public GameObject forceButton;

	public TextMeshProUGUI playersNumber;

	public Image loadingbar;

	private void Awake()
	{
		singleton = this;
	}

	private void Update()
	{
		window.SetActive(info != string.Empty && info != "started");
		float result = 0f;
		float.TryParse(info, out result);
		result -= 1f;
		result /= 19f;
		loadingbar.fillAmount = Mathf.Lerp(loadingbar.fillAmount, result, Time.deltaTime);
		playersNumber.text = PlayerManager.singleton.players.Length.ToString();
	}

	private void Start()
	{
		GetComponent<RectTransform>().localPosition = Vector3.zero;
	}

	public void ShowButton()
	{
		forceButton.SetActive(true);
	}

	public void UseButton()
	{
		forceButton.SetActive(false);
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.isLocalPlayer && gameObject.name == "Host")
			{
				component.ForceRoundStart();
			}
		}
	}
}
