using UnityEngine;
using Mirror;

public class FallDamage : NetworkBehaviour
{
	public bool isGrounded = true;

	public LayerMask groundMask;

	[SerializeField]
	private float groundMaxDistance = 1.3f;

	public AudioClip sound;

	public AudioSource sfxsrc;

	private float previousHeight;

	public AnimationCurve damageOverDistance;

	private CharacterClassManager ccm;

	private static int kCmdCmdDoSound;

	private static int kRpcRpcDoSound;

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
	}

	private void Update()
	{
		CalculateGround();
	}

	private void CalculateGround()
	{
		bool flag = Physics.Raycast(base.transform.position, Vector3.down, groundMaxDistance, groundMask);
		if (flag != isGrounded)
		{
			isGrounded = flag;
			if (isGrounded)
			{
				OnTouchdown();
			}
			else
			{
				OnLoseContactWithGround();
			}
		}
	}

	private void OnLoseContactWithGround()
	{
		previousHeight = base.transform.position.y;
	}

	private void OnTouchdown()
	{
		float num = damageOverDistance.Evaluate(previousHeight - base.transform.position.y);
		if (num > 5f && ccm.klasy[ccm.curClass].team != Team.SCP)
		{
			GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(num, "WORLD", "FALLDOWN"), base.gameObject);
			CmdDoSound();
		}
	}

	[Command]
	private void CmdDoSound()
	{
		RpcDoSound();
	}

	[ClientRpc]
	private void RpcDoSound()
	{
		sfxsrc.PlayOneShot(sound);
	}
}
