using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Scp049PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public Camera plyCam;

	public bool iAm049;

	public bool sameClass;

	public GameObject scpInstance;

	[Header("Infection")]
	public float currentInfection;

	[Header("Attack & Recall")]
	public float distance = 2.4f;

	public float recallDistance = 3.5f;

	public float recallProgress;

	private GameObject recallingObject;

	private Ragdoll recallingRagdoll;

	private ScpInterfaces interfaces;

	private Image loadingCircle;

	private FirstPersonController fpc;

	[Header("Boosts")]
	public AnimationCurve boost_recallTime;

	public AnimationCurve boost_infectTime;

	private void Start()
	{
		interfaces = FindAnyObjectByType<ScpInterfaces>();
		loadingCircle = interfaces.Scp049_loading;
		if (base.isLocalPlayer)
		{
			fpc = GetComponent<FirstPersonController>();
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		if (scpInstance != null && scpInstance.GetComponent<CharacterClassManager>().curClass != 5)
		{
			scpInstance = null;
		}
		Scp049PlayerScript[] array = Object.FindObjectsOfType<Scp049PlayerScript>();
		if (classID == 5)
		{
			iAm049 = true;
			Scp049PlayerScript[] array2 = array;
			foreach (Scp049PlayerScript scp049PlayerScript in array2)
			{
				scp049PlayerScript.scpInstance = base.gameObject;
			}
		}
		else
		{
			iAm049 = false;
		}
		if (base.isLocalPlayer)
		{
			interfaces.Scp049_eq.SetActive(classID == 5);
		}
	}

	private void Update()
	{
		DeductInfection();
		UpdateInput();
	}

	private void DeductInfection()
	{
		if (currentInfection > 0f)
		{
			currentInfection -= Time.deltaTime;
		}
		if (currentInfection < 0f)
		{
			currentInfection = 0f;
		}
	}

	private void UpdateInput()
	{
		if (base.isLocalPlayer)
		{
			if (Input.GetButtonDown("Fire1"))
			{
				Attack();
			}
			if (Input.GetButtonDown("Interact"))
			{
				Surgery();
			}
			Recalling();
		}
	}

	private void Attack()
	{
		RaycastHit hitInfo;
		if (iAm049 && Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, distance))
		{
			Scp049PlayerScript component = hitInfo.transform.GetComponent<Scp049PlayerScript>();
			if (component != null && !component.sameClass)
			{
				InfectPlayer(component.gameObject, GetComponent<NetworkIdentity>().netId.ToString());
			}
		}
	}

	private void Surgery()
	{
		RaycastHit hitInfo;
		if (!iAm049 || !Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, recallDistance))
		{
			return;
		}
		Ragdoll componentInParent = hitInfo.transform.GetComponentInParent<Ragdoll>();
		if (!(componentInParent != null) || !componentInParent.allowRecall)
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<NetworkIdentity>().netId.ToString() == componentInParent.owner.ownerHLAPI_id && gameObject.GetComponent<Scp049PlayerScript>().currentInfection > 0f && componentInParent.allowRecall)
			{
				recallingObject = gameObject;
				recallingRagdoll = componentInParent;
			}
		}
	}

	[Command(channel = 2)]
	private void CmdDestroyPlayer(GameObject recallingRagdoll)
	{
		NetworkServer.Destroy(recallingRagdoll);
	}

	private void Recalling()
	{
		if (iAm049 && Input.GetButton("Interact") && recallingObject != null)
		{
			fpc.lookingAtMe = true;
			recallProgress += Time.deltaTime / boost_recallTime.Evaluate(GetComponent<PlayerStats>().GetHealthPercent());
			if (recallProgress >= 1f)
			{
				CmdRecallPlayer(recallingObject);
				CmdDestroyPlayer(recallingRagdoll.gameObject);
				recallProgress = 0f;
				recallingObject = null;
			}
		}
		else
		{
			recallingObject = null;
			recallProgress = 0f;
			if (iAm049)
			{
				fpc.lookingAtMe = false;
			}
		}
		loadingCircle.fillAmount = recallProgress;
	}

	private void InfectPlayer(GameObject target, string id)
	{
		CmdInfectPlayer(target, boost_infectTime.Evaluate(GetComponent<PlayerStats>().GetHealthPercent()));
		Hitmarker.Hit();
		GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(999999f, id, "SCP:049"), target);
	}

	[Command(channel = 2)]
	private void CmdInfectPlayer(GameObject target, float infTime)
	{
		RpcInfectPlayer(target, infTime);
	}

	[ClientRpc(channel = 2)]
	private void RpcInfectPlayer(GameObject target, float infTime)
	{
		target.GetComponent<Scp049PlayerScript>().currentInfection = infTime;
	}

	[Command(channel = 2)]
	private void CmdRecallPlayer(GameObject target)
	{
		target.GetComponent<CharacterClassManager>().curClass = 10;
	}
}
