using System.Collections;
using UnityEngine;
using Mirror;

public class Scp049_2PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public Camera plyCam;

	public Animator animator;

	public bool iAm049_2;

	public bool sameClass;

	[Header("Attack")]
	public float distance = 2.4f;

	public int damage = 60;

	[Header("Boosts")]
	public AnimationCurve multiplier;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			StartCoroutine(UpdateInput());
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		iAm049_2 = classID == 10;
		animator.gameObject.SetActive(base.isLocalPlayer && iAm049_2);
	}

	private IEnumerator UpdateInput()
	{
		while (true)
		{
			if (Input.GetButton("Fire1") && iAm049_2)
			{
				float mt = multiplier.Evaluate(GetComponent<PlayerStats>().GetHealthPercent());
				CmdShootAnim();
				animator.SetTrigger("Shoot");
				animator.speed = mt;
				yield return new WaitForSeconds(0.65f / mt);
				Attack();
				yield return new WaitForSeconds(1f / mt);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private void Attack()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, distance))
		{
			Scp049_2PlayerScript scp049_2PlayerScript = hitInfo.transform.GetComponent<Scp049_2PlayerScript>();
			if (scp049_2PlayerScript == null)
			{
				scp049_2PlayerScript = hitInfo.transform.GetComponentInParent<Scp049_2PlayerScript>();
			}
			if (scp049_2PlayerScript != null && !scp049_2PlayerScript.sameClass)
			{
				HurtPlayer(hitInfo.transform.gameObject, damage, netId.ToString());
			}
		}
	}

	private void HurtPlayer(GameObject ply, float amount, string id)
	{
		Hitmarker.Hit();
		GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(amount, id, "SCP:0492"), ply);
	}

	[Command(channel = 1)]
	private void CmdShootAnim()
	{
		RpcShootAnim();
	}

	[ClientRpc]
	private void RpcShootAnim()
	{
		GetComponent<AnimationController>().DoAnimation("Shoot");
	}
}
