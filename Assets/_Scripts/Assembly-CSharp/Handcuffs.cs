using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Handcuffs : NetworkBehaviour
{
	public TextMeshProUGUI distanceText;

	private Transform plyCam;

	private CharacterClassManager ccm;

	private Inventory inv;

	public LayerMask mask;

	public float maxDistance;

	private Image uncuffProgress;

	[SyncVar]
	public GameObject cuffTarget;

	private float progress;

	private float lostCooldown;

	private void Start()
	{
		uncuffProgress = GameObject.Find("UncuffProgress").GetComponent<Image>();
		inv = GetComponent<Inventory>();
		plyCam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		ccm = GetComponent<CharacterClassManager>();
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			CheckForInput();
			UpdateText();
		}
		if (cuffTarget != null)
		{
			cuffTarget.GetComponent<AnimationController>().cuffed = true;
		}
	}

	private void CheckForInput()
	{
		if (cuffTarget != null)
		{
			bool flag = false;
			foreach (Item item in inv.items)
			{
				if (item.id == 27)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				CmdTarget(null);
			}
		}
		if (!(GetComponent<WeaponManager>().inventoryCooldown <= 0f))
		{
			return;
		}
		if (inv.curItem == 27)
		{
			if (Input.GetButtonDown("Fire1") && cuffTarget == null)
			{
				CuffPlayer();
			}
			else if (Input.GetButtonDown("Zoom") && cuffTarget != null)
			{
				CmdTarget(null);
			}
		}
		if (ccm.curClass >= 0 && ccm.klasy[ccm.curClass].team != Team.SCP && Input.GetButton("Interact"))
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(plyCam.position, plyCam.forward, out hitInfo, maxDistance))
			{
				Handcuffs componentInParent = hitInfo.collider.GetComponentInParent<Handcuffs>();
				if (componentInParent != null && componentInParent.GetComponent<AnimationController>().handAnimator.GetBool("Cuffed"))
				{
					progress += Time.deltaTime;
					if (progress >= 5f)
					{
						progress = 0f;
						GameObject[] players = PlayerManager.singleton.players;
						foreach (GameObject gameObject in players)
						{
							if (gameObject.GetComponent<Handcuffs>().cuffTarget == componentInParent.gameObject)
							{
								CmdResetTarget(gameObject);
							}
						}
					}
				}
				else
				{
					progress = 0f;
				}
			}
			else
			{
				progress = 0f;
			}
		}
		else
		{
			progress = 0f;
		}
		if (ccm.curClass != 3)
		{
			uncuffProgress.fillAmount = Mathf.Clamp01(progress / 5f);
		}
	}

	private void CuffPlayer()
	{
		Ray ray = new Ray(plyCam.position, plyCam.forward);
		RaycastHit hitInfo;
		if (!Physics.Raycast(ray, out hitInfo, maxDistance, mask))
		{
			return;
		}
		CharacterClassManager componentInParent = hitInfo.collider.GetComponentInParent<CharacterClassManager>();
		if (componentInParent != null)
		{
			Class obj = ccm.klasy[componentInParent.curClass];
			if (obj.team != Team.SCP && (obj.team == Team.CDP || obj.team == Team.CHI) != (ccm.klasy[ccm.curClass].team == Team.CDP || ccm.klasy[ccm.curClass].team == Team.CHI) && componentInParent.GetComponent<AnimationController>().curAnim == 0 && componentInParent.GetComponent<AnimationController>().speed == Vector2.zero)
			{
				CmdTarget(componentInParent.gameObject);
			}
		}
	}

	[Command(channel = 2)]
	public void CmdTarget(GameObject t)
	{
		cuffTarget = t;
		RpcDropItems(t);
	}

	[Command(channel = 2)]
	public void CmdResetTarget(GameObject t)
	{
		t.GetComponent<Handcuffs>().cuffTarget = null;
	}

	[ClientRpc(channel = 2)]
	private void RpcDropItems(GameObject ply)
	{
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (ply == gameObject && gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				ply.GetComponent<Inventory>().DropAll();
				break;
			}
		}
	}

	private void UpdateText()
	{
		if (cuffTarget != null)
		{
			float num = Vector3.Distance(base.transform.position, cuffTarget.transform.position);
			if (num > 200f)
			{
				num = 200f;
				lostCooldown += Time.deltaTime;
				if (lostCooldown > 1f)
				{
					CmdTarget(null);
				}
			}
			else
			{
				lostCooldown = 0f;
			}
			distanceText.text = (num * 1.5f).ToString("0 m");
		}
		else
		{
			distanceText.text = "NONE";
		}
	}
}
