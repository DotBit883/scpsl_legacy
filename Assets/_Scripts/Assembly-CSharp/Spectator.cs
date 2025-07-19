using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Spectator : NetworkBehaviour
{
	private CharacterClassManager ccm;

	public bool isSpect;

	private PlayerManager pm;

	public Camera mainCam;

	private SpectatorCamera spectCam;

	public GameObject weaponCams;

	public Transform spectCamPos;

	private List<GameObject> alivePlayers = new List<GameObject>();

	private int curFocus;

	private bool deadCam;

	private Text spectInfo;

	private Text respInfo;

	private bool isTracking;

	private void Start()
	{
		spectInfo = UserMainInterface.singleton.specatorInfo;
		respInfo = GameObject.Find("RespawnInfo").GetComponent<Text>();
		pm = PlayerManager.singleton;
		ccm = GetComponent<CharacterClassManager>();
	}

	public void Init()
	{
		if (TutorialManager.status)
		{
			return;
		}
		isSpect = ccm.curClass == 2;
		if (base.isLocalPlayer)
		{
			spectCam = FindAnyObjectByType<SpectatorCamera>();
			if (ccm.curClass != -1 && ccm.curClass != 2)
			{
				deadCam = true;
			}
			RefreshSpectList();
			ActivateCorrectCameras();
			if (isSpect)
			{
				base.transform.position = Vector3.up * 200f;
			}
		}
	}

	private void ActivateCorrectCameras()
	{
		if (spectCam == null)
		{
			spectCam = FindAnyObjectByType<SpectatorCamera>();
			return;
		}
		spectCam.freeCam.enabled = alivePlayers.Count == 0 && isSpect;
		spectCam.cam.enabled = alivePlayers.Count != 0 && isSpect;
		spectCam.cam079.enabled = ccm.curClass == 7;
		mainCam.enabled = !isSpect;
		weaponCams.SetActive(!isSpect && ccm.curClass != 7);
		mainCam.gameObject.SetActive(ccm.curClass != 7);
	}

	private void Update()
	{
		if (base.isLocalPlayer && isSpect)
		{
			WaitForInput();
		}
	}

	private void LateUpdate()
	{
		if (base.isLocalPlayer && !isSpect)
		{
			if (spectCam == null)
			{
				spectCam = FindAnyObjectByType<SpectatorCamera>();
				return;
			}
			if (ccm.curClass != 7)
			{
				spectCam.cam.transform.position = mainCam.transform.position;
				spectCam.cam.transform.rotation = mainCam.transform.rotation;
			}
		}
		if (base.isLocalPlayer)
		{
			if (isTracking)
			{
				isTracking = false;
				GameObject gameObject = alivePlayers[curFocus];
				spectInfo.color = ccm.klasy[gameObject.GetComponent<CharacterClassManager>().curClass].classColor;
				spectInfo.text = gameObject.GetComponent<NicknameSync>().myNick + " " + gameObject.GetComponent<PlayerStats>().health + "HP";
			}
			else
			{
				spectInfo.text = string.Empty;
			}
			if (isSpect)
			{
				respInfo.color = ((MTFRespawn.spectatorTimeToSpawn >= 0) ? new Color32(0, 150, byte.MaxValue, byte.MaxValue) : new Color32(0, 143, 30, byte.MaxValue));
			}
			else
			{
				respInfo.text = string.Empty;
			}
		}
	}

	private string GetRespawnTime()
	{
		int num = Mathf.Abs(MTFRespawn.spectatorTimeToSpawn) - ServerTime.time;
		int num2 = Mathf.FloorToInt(num / 60);
		int num3 = Mathf.CeilToInt(num - num2 * 60);
		bool flag = PlayerPrefs.GetString("langver", "en") == "pl";
		return ((!flag) ? "Respawn in " : "Odrodzenie za ") + num2 + " m, " + num3 + " s";
	}

	private void WaitForInput()
	{
		if (Input.anyKeyDown)
		{
			RefreshSpectList();
			ActivateCorrectCameras();
		}
		if (Input.GetButtonDown("Spectator: Next Player"))
		{
			curFocus++;
		}
		if (Input.GetButtonDown("Spectator: Previous Player"))
		{
			curFocus++;
		}
		if (alivePlayers.Count <= 0)
		{
			return;
		}
		if (deadCam && Input.anyKeyDown)
		{
			deadCam = false;
		}
		else if (!deadCam)
		{
			if (curFocus >= alivePlayers.Count)
			{
				curFocus = 0;
			}
			if (curFocus < 0)
			{
				curFocus = alivePlayers.Count - 1;
			}
			if (curFocus >= 0 && curFocus < alivePlayers.Count && alivePlayers[curFocus].GetComponent<CharacterClassManager>().curClass != 2)
			{
				TrackMovement(alivePlayers[curFocus]);
			}
		}
	}

	private void TrackMovement(GameObject ply)
	{
		isTracking = true;
		PlyMovementSync component = ply.GetComponent<PlyMovementSync>();
		spectCam.transform.position = ply.GetComponent<Spectator>().spectCamPos.position;
		spectCam.transform.rotation = Quaternion.Lerp(spectCam.transform.rotation, Quaternion.Euler(new Vector3(component.rotX, component.rotation, 0f)), Time.deltaTime * 15f);
	}

	private void RefreshSpectList()
	{
		alivePlayers.Clear();
		GameObject[] players = pm.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<CharacterClassManager>().curClass != 2)
			{
				alivePlayers.Add(gameObject);
			}
		}
	}
}
