using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Mirror;

public class AnimationController : NetworkBehaviour
{
	[Serializable]
	public class AnimAudioClip
	{
		public string clipName;

		public AudioClip audio;
	}

	public AnimAudioClip[] clips;

	public AudioSource footstepSource;

	public AudioSource gunSource;

	public Animator animator;

	public Animator handAnimator;

	[SyncVar]
	public int curAnim;

	[SyncVar]
	public Vector2 speed;

	[SyncVar]
	public int item;

	public bool cuffed;

	private FirstPersonController fpc;

	private Inventory inv;

	public static List<AnimationController> controllers = new List<AnimationController>();

	private int prevItem;

	public void SyncItem(int i)
	{
		item = i;
	}

	private void Start()
	{
		fpc = GetComponent<FirstPersonController>();
		inv = GetComponent<Inventory>();
		if (!base.isLocalPlayer)
		{
			controllers.Add(this);
			Invoke(nameof(RefreshItems), 6f);
		}
	}

	private void OnDestroy()
	{
		controllers.Remove(this);
	}

	private void LateUpdate()
	{
		if (!base.isLocalPlayer && handAnimator != null)
		{
			handAnimator.SetBool("Cuffed", cuffed);
		}
		cuffed = false;
	}

	public void PlaySound(int id, bool isGun)
	{
		if (!base.isLocalPlayer)
		{
			if (isGun)
			{
				gunSource.PlayOneShot(clips[id].audio);
			}
			else
			{
				footstepSource.PlayOneShot(clips[id].audio);
			}
		}
	}

	public void PlaySound(string label, bool isGun)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < clips.Length; i++)
		{
			if (clips[i].clipName == label)
			{
				num = i;
			}
		}
		if (isGun)
		{
			gunSource.PlayOneShot(clips[num].audio);
		}
		else
		{
			footstepSource.PlayOneShot(clips[num].audio);
		}
	}

	public void DoAnimation(string trigger)
	{
		if (!base.isLocalPlayer)
		{
			handAnimator.SetTrigger(trigger);
		}
	}

	private void FixedUpdate()
	{
		if (!base.isLocalPlayer)
		{
			if (prevItem != item)
			{
				prevItem = item;
				RefreshItems();
			}
			RecieveData();
		}
		else
		{
			TransmitData(fpc.animationID, item, fpc.plySpeed);
		}
	}

	private void RefreshItems()
	{
		HandPart[] componentsInChildren = GetComponentsInChildren<HandPart>();
		HandPart[] array = componentsInChildren;
		foreach (HandPart handPart in array)
		{
			handPart.Invoke(nameof(HandPart.UpdateItem), 0.3f);
		}
	}

	public void SetState(int i)
	{
		curAnim = i;
	}

	public void RecieveData()
	{
		if (!(animator != null))
		{
			return;
		}
		CalculateAnimation();
		if (handAnimator == null)
		{
			Animator[] componentsInChildren = animator.GetComponentsInChildren<Animator>();
			if (componentsInChildren.Length > 1)
			{
				handAnimator = componentsInChildren[1];
			}
		}
		else
		{
			handAnimator.SetInteger("CurItem", item);
			handAnimator.SetInteger("Running", (speed.x != 0f) ? ((curAnim != 1) ? 1 : 2) : 0);
		}
	}

	private void CalculateAnimation()
	{
		animator.SetBool("Stafe", (curAnim != 2) & ((Mathf.Abs(speed.y) > 0f) & ((speed.x == 0f) | ((speed.x > 0f) & (curAnim == 0)))));
		animator.SetBool("Jump", curAnim == 2);
		float value = 0f;
		float value2 = 0f;
		if (curAnim != 2)
		{
			if (speed.x != 0f)
			{
				value = ((curAnim != 1) ? 1 : 2);
				if (speed.x < 0f)
				{
					value = -1f;
				}
			}
			if (speed.y != 0f)
			{
				value2 = ((speed.y > 0f) ? 1 : (-1));
			}
		}
		animator.SetFloat("Speed", value, 0.1f, Time.deltaTime);
		animator.SetFloat("Direction", value2, 0.1f, Time.deltaTime);
	}

	[ClientCallback]
	private void TransmitData(int state, int it, Vector2 v2)
	{
		if (NetworkClient.active)
		{
			CmdSyncData(state, it, v2);
		}
	}

	[Command(channel = 3)]
	private void CmdSyncData(int state, int it, Vector2 v2)
	{
		curAnim = state;
		item = it;
		speed = v2;
		Color red = Color.red;
	}
}
