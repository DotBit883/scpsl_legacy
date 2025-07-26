using System.Collections;
using UnityEngine;
using Mirror;

public class TeslaGate : NetworkBehaviour
{
	public AudioClip windup;

	public AudioClip shock;

	public AudioClip hacksound;

	public GameObject effect;

	public AudioSource source;

	public LightBlink[] blinkers;

	private float lightIntensity;

	private bool trigger;

	private bool hack;

	public void Hack()
	{
		hack = true;
	}

	private void Update()
	{
		base.transform.localPosition = new Vector3(0f, 1.91f, 5.64f);
		base.transform.localRotation = Quaternion.Euler(new Vector3(0f, -180f, 0f));
	}

	private void Start()
	{
		StartCoroutine(IUpdate());
	}

	private IEnumerator IUpdate()
	{
		while (true)
		{
			if (hack)
			{
				hack = false;
				source.PlayOneShot(hacksound);
				yield return new WaitForSeconds(0.2f);
				effect.SetActive(true);
				LightBlink[] array = blinkers;
				foreach (LightBlink lightBlink in array)
				{
					lightBlink.disabled = false;
				}
				yield return new WaitForSeconds(4.8f);
				effect.SetActive(false);
				LightBlink[] array2 = blinkers;
				foreach (LightBlink lightBlink2 in array2)
				{
					lightBlink2.disabled = true;
				}
				yield return new WaitForSeconds(5f);
				trigger = false;
			}
			else if (trigger)
			{
				source.PlayOneShot(windup);
				yield return new WaitForSeconds(0.4f);
				effect.SetActive(true);
				LightBlink[] array3 = blinkers;
				foreach (LightBlink lightBlink3 in array3)
				{
					lightBlink3.disabled = false;
				}
				source.PlayOneShot(shock);
				yield return new WaitForSeconds(0.6f);
				effect.SetActive(false);
				LightBlink[] array4 = blinkers;
				foreach (LightBlink lightBlink4 in array4)
				{
					lightBlink4.disabled = true;
				}
				trigger = false;
				float f = 0f;
				while (f < 0.67f && !hack)
				{
					f += Time.deltaTime;
					yield return new WaitForEndOfFrame();
				}
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public void Trigger(bool isKiller, GameObject other)
	{
		if (isKiller)
		{
			if (other.GetComponentInParent<NetworkIdentity>().isLocalPlayer)
			{
				other.GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(Random.Range(700, 900), "WORLD", "TESLA"), other.gameObject);
			}
		}
		else
		{
			trigger = true;
		}
	}
}
