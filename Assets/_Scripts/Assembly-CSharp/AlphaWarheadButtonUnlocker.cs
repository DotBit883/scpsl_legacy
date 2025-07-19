using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class AlphaWarheadButtonUnlocker : NetworkBehaviour
{
	public GameObject leftButton;

	public GameObject rightButton;

	public Vector3 closedRot;

	public Vector3 openedRot;

	public GameObject glass;

	public float cooldown;

	[SyncVar(hook = nameof(SetLeft))]
	private bool lockL;

	[SyncVar(hook = nameof(SetRight))]
	private bool lockR;

	private void SetLeft(bool oldValue, bool newValue)
	{
		leftButton.GetComponentInChildren<Light>().enabled = newValue;
	}

	private void SetRight(bool oldValue, bool newValue)
	{
		rightButton.GetComponentInChildren<Light>().enabled = newValue;
	}

	private void Start()
	{
		leftButton.name = "AW_KEYCARD_LEFT";
		rightButton.name = "AW_KEYCARD_RIGHT";
	}

	private void Update()
	{
		if (cooldown > 0f)
		{
			cooldown -= Time.deltaTime;
		}
		glass.transform.localRotation = Quaternion.LerpUnclamped(glass.transform.localRotation, Quaternion.Euler((!(lockL & lockR)) ? closedRot : openedRot), Time.deltaTime * 2.5f);
	}

	public void ChangeButtonStage(string bid)
	{
		if (!(cooldown > 0f))
		{
			cooldown = 0.7f;
			if (bid == "DENIED")
			{
				GetComponent<AudioSource>().Play();
			}
			else if (bid == "AW_KEYCARD_LEFT")
			{
				lockL = !lockL;
				leftButton.GetComponent<AudioSource>().Play();
			}
			else
			{
				lockR = !lockR;
				rightButton.GetComponent<AudioSource>().Play();
			}
		}
	}
}
