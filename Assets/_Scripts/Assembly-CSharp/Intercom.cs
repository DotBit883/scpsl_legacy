using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Intercom : NetworkBehaviour
{
	private CharacterClassManager ccm;

	private Transform area;

	public float triggerDistance;

	private float speechTime;

	private float cooldownAfter;

	public float speechRemainingTime;

	public float remainingCooldown;

	public Text txt;

	[SyncVar]
	public GameObject speaker;

	private Intercom host;

	public GameObject start_sound;

	public GameObject stop_sound;

	private string content = string.Empty;

	private bool inUse;

	private bool isTransmitting;

	private IEnumerator StartTransmitting(GameObject sp)
	{
		RpcPlaySound(true);
		yield return new WaitForSeconds(2f);
		speaker = sp;
		speechRemainingTime = speechTime;
		while (speechRemainingTime > 0f && speaker != null)
		{
			speechRemainingTime -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		if (speaker != null)
		{
			speaker = null;
		}
		RpcPlaySound(false);
		remainingCooldown = cooldownAfter;
		while (remainingCooldown >= 0f)
		{
			remainingCooldown -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		inUse = false;
	}

	private void Start()
	{
		if (!TutorialManager.status)
		{
			txt = GameObject.Find("IntercomMonitor").GetComponent<Text>();
			ccm = GetComponent<CharacterClassManager>();
			area = GameObject.Find("IntercomSpeakingZone").transform;
			speechTime = ConfigFile.GetInt("intercom_max_speech_time", 20);
			cooldownAfter = ConfigFile.GetInt("intercom_cooldown", 180);
			StartCoroutine(FindHost());
			StartCoroutine(CheckForInput());
			if (base.isLocalPlayer && base.isServer)
			{
				InvokeRepeating("RefreshText", 5f, 7f);
			}
		}
	}

	private void RefreshText()
	{
		CmdUpdateText(content);
	}

	private IEnumerator FindHost()
	{
		while (host == null)
		{
			GameObject h = GameObject.Find("Host");
			if (h != null)
			{
				host = h.GetComponent<Intercom>();
			}
			yield return new WaitForFixedUpdate();
		}
	}

	[ClientRpc]
	public void RpcPlaySound(bool start)
	{
		GameObject obj = Object.Instantiate((!start) ? stop_sound : start_sound);
		Object.Destroy(obj, 10f);
	}

	private void Update()
	{
		if (!TutorialManager.status && base.isLocalPlayer && base.isServer)
		{
			UpdateText();
		}
	}

	private void UpdateText()
	{
		if (remainingCooldown > 0f)
		{
			content = "RESTARTING\n" + Mathf.CeilToInt(remainingCooldown);
		}
		else if (speaker != null)
		{
			content = "TRANSMITTING...\nTIME LEFT - " + Mathf.CeilToInt(speechRemainingTime);
		}
		else
		{
			content = "READY";
		}
		if (content != txt.text)
		{
			CmdUpdateText(content);
		}
	}

	[Command(channel = 2)]
	private void CmdUpdateText(string t)
	{
		RpcUpdateText(t);
	}

	[ClientRpc(channel = 2)]
	private void RpcUpdateText(string t)
	{
		txt.text = t;
	}

	public void RequestTransmission(GameObject spk)
	{
		if (spk == null)
		{
			speaker = null;
		}
		else if (remainingCooldown <= 0f && !inUse)
		{
			inUse = true;
			StartCoroutine(StartTransmitting(spk));
		}
	}

	private IEnumerator CheckForInput()
	{
		if (base.isLocalPlayer)
		{
			while (true)
			{
				if (host != null)
				{
					if (AllowToSpeak() && host.speaker == null)
					{
						CmdSetTransmit(base.gameObject);
					}
					if (!AllowToSpeak() && host.speaker == base.gameObject)
					{
						yield return new WaitForSeconds(1f);
						if (!AllowToSpeak())
						{
							CmdSetTransmit(null);
						}
					}
				}
				yield return new WaitForEndOfFrame();
			}
		}
		yield return new WaitForEndOfFrame();
	}

	private bool AllowToSpeak()
	{
		return Vector3.Distance(base.transform.position, area.position) < triggerDistance && Input.GetButton("VoiceChat") && ccm.klasy[ccm.curClass].team != Team.SCP;
	}

	[Command(channel = 2)]
	private void CmdSetTransmit(GameObject player)
	{
		GameObject.Find("Host").GetComponent<Intercom>().RequestTransmission(player);
	}
}
