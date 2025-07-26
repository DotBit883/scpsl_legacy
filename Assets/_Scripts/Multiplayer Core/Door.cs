using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class Door : NetworkBehaviour
{
	[Serializable]
	public class RoomAccessConditions
	{
		public bool safe_scp;

		public bool medium_scp;

		public bool violent_scp;

		public bool lowlvl_arm;

		public bool midlvl_arm;

		public bool highlvl_arm;

		public bool intercom_room;

		public bool check_point;

		public bool exit_gate;

		public bool allow_detonate;
	}

	public enum PermissionLevel
	{
		NoButtons = 0,
		AllwaysAccesible = 1,
		AccessDenied = 2,
		AccessGranted = 3
	}

	public AudioClip audio_denied;

	public AudioClip audio_granted;

	public Vector3 startPos = Vector3.zero;

	public Vector3 secondPos = Vector3.zero;

	public Transform secondDoor;

	public float cooldown;

	public AnimationCurve anim;

	public float animDuration = 2f;

	public AudioClip openSound;

	public AudioClip closeSound;

	public AudioClip optionalAudio;

	public AudioSource manualSourceOptional;

	public float closeTime;

	public GameObject[] buttons;

	public bool useZ;

	public bool invertSecondDoorPos;

	[SyncVar(hook = nameof(SetState))]
	public bool isOpen;

	public RoomAccessConditions conditions;

	[Header("Start Cooldown")]
	public float curCooldown;

	public void SetState(bool oldValue, bool newValue)
	{
		StopAllCoroutines();
		StartCoroutine(AnimateDoor());
	}

	public void InitState()
	{
		base.transform.localPosition = ((!isOpen) ? startPos : (startPos + ((!useZ) ? Vector3.right : Vector3.back) * anim.Evaluate(1f)));
		if (secondDoor != null)
		{
			secondDoor.transform.localPosition = ((!((!invertSecondDoorPos) ? isOpen : (!isOpen))) ? secondPos : (secondPos + ((!useZ) ? Vector3.left : Vector3.forward) * anim.Evaluate(1f)));
		}
	}

	public void PlaySound(bool isGranted)
	{
		if (curCooldown <= 0f)
		{
			curCooldown = 0.3f;
			if (GetComponent<AudioSource>() != null)
			{
				GetComponent<AudioSource>().PlayOneShot((!isGranted) ? audio_denied : audio_granted);
			}
			else if (GetComponentInChildren<AudioSource>() != null)
			{
				GetComponentInChildren<AudioSource>().PlayOneShot((!isGranted) ? audio_denied : audio_granted);
			}
			else
			{
				manualSourceOptional.PlayOneShot((!isGranted) ? audio_denied : audio_granted);
			}
			StartCoroutine(DeductCooldown());
		}
	}

	private GameObject GetNearestButton()
	{
		GameObject gameObject = null;
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject2 in players)
		{
			if (gameObject2.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				gameObject = gameObject2;
				break;
			}
		}
		GameObject result = null;
		float num = 10f;
		GameObject[] array = buttons;
		foreach (GameObject gameObject3 in array)
		{
			float num2 = Vector3.Distance(gameObject.transform.position, gameObject3.transform.position);
			if (num2 < num)
			{
				num = num2;
				result = gameObject3;
			}
		}
		return result;
	}

	private void SetButtonTexture(Material mat)
	{
		GameObject[] array = buttons;
		foreach (GameObject gameObject in array)
		{
			gameObject.GetComponent<Renderer>().material = mat;
		}
		Invoke("ResetButtonTextures", 1f);
	}

	public void ChangeState()
	{
		if (curCooldown <= 0f)
		{
			isOpen = !isOpen;
			curCooldown = cooldown;
		}
	}

	public bool isAbleToEmergencyOpen()
	{
		return curCooldown <= 0f && !isOpen && !conditions.allow_detonate && !conditions.violent_scp;
	}

	private IEnumerator AnimateDoor()
	{
		if (GetComponent<AudioSource>() != null)
		{
			GetComponent<AudioSource>().PlayOneShot((!isOpen) ? closeSound : openSound);
		}
		else if (GetComponentInChildren<AudioSource>() != null)
		{
			GetComponentInChildren<AudioSource>().PlayOneShot((!isOpen) ? closeSound : openSound);
		}
		else
		{
			manualSourceOptional.PlayOneShot((!isOpen) ? closeSound : openSound);
		}
		curCooldown = cooldown;
		float state = ((!isOpen) ? 1 : 0);
		while (curCooldown > 0f)
		{
			curCooldown -= Time.deltaTime;
			state += ((!isOpen) ? ((0f - Time.deltaTime) / animDuration) : (Time.deltaTime / animDuration));
			state = Mathf.Clamp(state, 0f, 1f);
			base.transform.localPosition = startPos + new Vector3((!useZ) ? anim.Evaluate(state) : 0f, 0f, (!useZ) ? 0f : anim.Evaluate(state));
			if (secondDoor != null)
			{
				if (invertSecondDoorPos)
				{
					secondDoor.transform.localPosition = secondPos + new Vector3((!useZ) ? anim.Evaluate(1f - state) : 0f, 0f, (!useZ) ? 0f : anim.Evaluate(1f - state));
				}
				else
				{
					secondDoor.transform.localPosition = secondPos + new Vector3((!useZ) ? (0f - anim.Evaluate(state)) : 0f, 0f, (!useZ) ? 0f : (0f - anim.Evaluate(state)));
				}
			}
			if (closeTime > 0f && isOpen && curCooldown <= 0f)
			{
				curCooldown = closeTime;
				yield return new WaitForSeconds(closeTime - 3f);
				manualSourceOptional.PlayOneShot(optionalAudio);
				yield return new WaitForSeconds(2f);
				isOpen = false;
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public void InvokeDeductCooldown()
	{
		StartCoroutine(DeductCooldown());
	}

	private IEnumerator DeductCooldown()
	{
		while (curCooldown > 0f)
		{
			curCooldown -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	private void Start()
	{
		StartCoroutine(DeductCooldown());
	}
}
