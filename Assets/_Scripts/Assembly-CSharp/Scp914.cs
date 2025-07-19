using System.Collections;
using UnityEngine;
using Mirror;

public class Scp914 : NetworkBehaviour
{
	public GameObject knob;

	public GameObject outputPlace;

	public GameObject doors;

	public AudioSource source;

	[SyncVar(hook = nameof(ChangeState))]
	public int state;

	private float cooldown;

	[SyncVar(hook = nameof(SetProcessing))]
	public bool isProcessing;

	public void SetProcessing(bool oldProcessing, bool newProcessing)
	{
		if (newProcessing)
		{
			StartCoroutine(StartProcessing());
		}
	}

	public void ChangeState(int oldState, int newState)
	{
		knob.GetComponent<AudioSource>().Play();
	}

	public void Refine()
	{
		if (!isProcessing)
		{
			isProcessing = true;
		}
	}

	private IEnumerator StartProcessing()
	{
		source.Play();
		yield return new WaitForSeconds(1f);
		while (doors.transform.localPosition.x > 0f)
		{
			doors.transform.localPosition -= Vector3.right * Time.deltaTime * 1.8f * ((!base.isServer) ? 1f : 0.5f);
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(11.7f);
		while (doors.transform.localPosition.x < 1.74f)
		{
			doors.transform.localPosition += Vector3.right * Time.deltaTime * 1.5f * ((!base.isServer) ? 1f : 0.5f);
			yield return new WaitForEndOfFrame();
		}
		isProcessing = false;
	}

	public void IncrementState()
	{
		if (!(cooldown > 0f) && !isProcessing)
		{
			if (state + 1 > 4)
			{
				state = 0;
			}
			else
			{
				state = state + 1;
			}
			cooldown = 0.2f;
		}
	}

	private void Update()
	{
		if (cooldown > 0f)
		{
			cooldown -= Time.deltaTime;
		}
		knob.transform.localRotation = Quaternion.LerpUnclamped(knob.transform.localRotation, Quaternion.Euler(0f, 0f, 45 * state - 90), Time.deltaTime * 5f);
	}
}
