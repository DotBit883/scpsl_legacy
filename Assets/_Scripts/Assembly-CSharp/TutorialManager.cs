using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
	[Serializable]
	public class TutorialScene
	{
		public List<Log> logs = new List<Log>();
	}

	[Serializable]
	public class Log
	{
		[Multiline]
		public string content_pl;

		[Multiline]
		public string content_en;

		public AudioClip clip_pl;

		public AudioClip clip_en;

		public float duration_pl;

		public float duration_en;

		public bool jumpforward;

		public bool stopPlayer;

		public string alias;
	}

	public static bool status;

	public static int levelID;

	private FirstPersonController fpc;

	private TextMeshProUGUI txt;

	public TutorialScene[] tutorials;

	private List<Log> logs = new List<Log>();

	private AudioSource src;

	public static int curlog = -1;

	private float timeToNext;

	private int npcKills;

	private int reloads;

	private int burns;

	private void Awake()
	{
		string text = SceneManager.GetActiveScene().name;
		status = text.Contains("Tutorial");
		if (status)
		{
			levelID = int.Parse(text.Remove(0, text.IndexOf("0")));
			logs = tutorials[levelID - 1].logs;
		}
	}

	private void Start()
	{
		if (status)
		{
			curlog = -1;
			fpc = GetComponent<FirstPersonController>();
			src = GameObject.Find("Lector").GetComponent<AudioSource>();
			txt = GameObject.FindGameObjectWithTag("Respawn").GetComponent<TextMeshProUGUI>();
		}
	}

	private void LateUpdate()
	{
		if (!status)
		{
			return;
		}
		fpc.tutstop = false;
		if (curlog >= 0 && logs[curlog].stopPlayer)
		{
			fpc.tutstop = true;
		}
		if (timeToNext > 0f)
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				timeToNext = 0f;
				src.Stop();
			}
			timeToNext -= Time.deltaTime;
			if (timeToNext <= 0f && curlog != -1)
			{
				txt.text = string.Empty;
				if (logs[curlog].jumpforward)
				{
					Trigger(curlog + 1);
				}
				else
				{
					curlog = -1;
				}
			}
		}
		if (curlog != -1 && timeToNext <= 0f)
		{
			timeToNext = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? logs[curlog].duration_en : logs[curlog].duration_pl);
			if (logs[curlog].clip_pl != null)
			{
				src.PlayOneShot((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? logs[curlog].clip_en : logs[curlog].clip_pl);
			}
			if (logs[curlog].duration_en > 0f)
			{
				txt.text = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? logs[curlog].content_en : logs[curlog].content_pl);
			}
		}
	}

	public void Trigger(int id)
	{
		curlog = id;
		if (logs[id].duration_en == -100f)
		{
			PlayerPrefs.SetInt("TutorialProgress", levelID + 1);
			NetworkManager.singleton.StopHost();
		}
		if (logs[id].duration_en == -200f)
		{
			SendMessage(logs[id].content_en);
			if (logs[id].jumpforward)
			{
				Trigger(id + 1);
			}
		}
		else
		{
			src.Stop();
			txt.text = string.Empty;
			timeToNext = 0f;
		}
	}

	public void Trigger(string alias)
	{
		for (int i = 0; i < logs.Count; i++)
		{
			if (logs[i].alias == alias)
			{
				Trigger(i);
				break;
			}
		}
	}

	public void KillNPC()
	{
		npcKills++;
		KillTrigger[] array = UnityEngine.Object.FindObjectsOfType<KillTrigger>();
		KillTrigger killTrigger = null;
		KillTrigger[] array2 = array;
		foreach (KillTrigger killTrigger2 in array2)
		{
			if (killTrigger == null || killTrigger2.prioirty < killTrigger.prioirty)
			{
				killTrigger = killTrigger2;
			}
		}
		if (killTrigger != null)
		{
			killTrigger.Trigger(npcKills);
		}
	}

	public void Reload()
	{
		reloads++;
	}

	private void Tutorial2_GiveNTFRifle()
	{
		UnityEngine.Object.Destroy(FindAnyObjectByType<NoammoTrigger>().gameObject);
		GameObject.Find("Host").GetComponent<Inventory>().CmdSetPickup(20, 0f, GameObject.Find("ItemPos").transform.position, Quaternion.Euler(-90f, 0f, 0f), default(Quaternion));
		Invoke("Tutorial2_GiveSFA", 1f);
	}

	private void Tutorial2_GiveAmmo()
	{
		Pickup[] array = UnityEngine.Object.FindObjectsOfType<Pickup>();
		foreach (Pickup pickup in array)
		{
			if (pickup.id == 29)
			{
				return;
			}
		}
		GameObject.Find("Host").GetComponent<Inventory>().CmdSetPickup(29, 12f, GameObject.Find("ItemPos").transform.position, default(Quaternion), default(Quaternion));
	}

	private void Tutorial2_MoreAmmo()
	{
		Pickup[] array = UnityEngine.Object.FindObjectsOfType<Pickup>();
		foreach (Pickup pickup in array)
		{
			if (pickup.id == 29)
			{
				return;
			}
		}
		GameObject.Find("Host").GetComponent<Inventory>().CmdSetPickup(29, 12f, GameObject.Find("ItemPos").transform.position, default(Quaternion), default(Quaternion));
		Trigger(5);
	}

	private void Tutorial2_Jumpin()
	{
		Trigger("epsilon");
	}

	private void Tutorial2_Curtain()
	{
		GameObject.Find("Curtain").GetComponent<AudioSource>().Play();
		GameObject.Find("Curtain").GetComponent<Animator>().SetBool("Open", !GameObject.Find("Curtain").GetComponent<Animator>().GetBool("Open"));
	}

	private void Tutorial2_GiveSFA()
	{
		GameObject.Find("Host").GetComponent<Inventory>().CmdSetPickup(22, 100000000f, GameObject.Find("ItemPos").transform.position, default(Quaternion), default(Quaternion));
	}

	private void Tutorial2_ResultText()
	{
		GameObject.Find("ResultText").GetComponent<Text>().text = (npcKills - 9).ToString("00");
	}

	public void Tutorial2_Preset()
	{
		FindAnyObjectByType<MainMenuScript>().ChangeMenu(FindAnyObjectByType<MainMenuScript>().curMenu + 1);
	}

	public void Tutorial2_Result()
	{
		Tutorial2_Curtain();
		if (reloads == 1)
		{
			Trigger("result_good");
		}
		else if (reloads == 2)
		{
			Trigger("result_ok");
		}
		else
		{
			Trigger("result_bad");
		}
	}

	private void Tutorial3_GiveKeycard()
	{
		GameObject.Find("Host").GetComponent<Inventory>().CmdSetPickup(0, 0f, GameObject.Find("ItemPos").transform.position, default(Quaternion), default(Quaternion));
	}

	public void Tutorial3_KeycardBurnt()
	{
		burns++;
		if (burns == 1)
		{
			Trigger("bc1");
			Invoke("Tutorial3_GiveKeycard", 3f);
		}
		if (burns == 2)
		{
			Trigger("bc2");
			Invoke("Tutorial3_GiveKeycard", 5f);
		}
		if (burns == 3)
		{
			Trigger("bc3");
			for (int i = 1; i <= 5; i++)
			{
				Invoke("Tutorial3_GiveKeycard", 1 + i / 5);
			}
		}
		if (burns == 8)
		{
			Trigger("bc4");
		}
	}

	private void Tutorial3_Quit()
	{
		Application.Quit();
	}
}
