using System.Collections;
using TMPro;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Scp106PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public Camera plyCam;

	public bool iAm106;

	public bool sameClass;

	public float ultimatePoints;

	public float teleportSpeed;

	public GameObject containAnnouncePrefab;

	public GameObject screamsPrefab;

	[SyncVar(hook = nameof(SetPortalPosition))]
	[Header("Portal")]
	public Vector3 portalPosition;

	public GameObject portalPrefab;

	private Vector3 previousPortalPosition;

	private CharacterClassManager ccm;

	private FirstPersonController fpc;

	private GameObject popup106;

	private TextMeshProUGUI highlightedAbilityText;

	private Text pointsText;

	private string highlightedString;

	public int highlightID;

	private Image cooldownImg;

	private float attackCooldown;

	public bool goingViaThePortal;

	private bool isHighlightingPoints;

	private void Start()
	{
		cooldownImg = GameObject.Find("Cooldown106").GetComponent<Image>();
		ccm = GetComponent<CharacterClassManager>();
		fpc = GetComponent<FirstPersonController>();
		InvokeRepeating("HumanPocketLoss", 1f, 1f);
	}

	private void Update()
	{
		CheckForInventoryInput();
		CheckForShootInput();
		AnimateHighlightedText();
		UpdatePointText();
	}

	private void HumanPocketLoss()
	{
		if (base.isLocalPlayer && base.transform.position.y < -1500f)
		{
			GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(1f, "WORLD", "POCKET"), base.gameObject);
		}
	}

	private void CheckForShootInput()
	{
		if (base.isLocalPlayer && iAm106)
		{
			cooldownImg.fillAmount = Mathf.Clamp01((!(attackCooldown <= 0f)) ? (1f - attackCooldown * 2f) : 0f);
			if (attackCooldown > 0f)
			{
				attackCooldown -= Time.deltaTime;
			}
			if (Input.GetButtonDown("Fire1") && attackCooldown <= 0f && GetComponent<WeaponManager>().inventoryCooldown <= 0f)
			{
				attackCooldown = 0.5f;
				Shoot();
			}
		}
	}

	private void Shoot()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, 1.5f))
		{
			CharacterClassManager component = hitInfo.transform.GetComponent<CharacterClassManager>();
			if (component != null && component.klasy[component.curClass].team != Team.SCP)
			{
				CmdMovePlayer(hitInfo.transform.gameObject, ServerTime.time);
				HurtPlayer(hitInfo.transform.gameObject, netId.ToString());
			}
		}
	}

	private void UpdatePointText()
	{
		if (pointsText == null)
		{
			pointsText = FindAnyObjectByType<ScpInterfaces>().Scp106_ability_points;
			return;
		}
		ultimatePoints += Time.deltaTime * 6.66f * teleportSpeed;
		ultimatePoints = Mathf.Clamp(ultimatePoints, 0f, 100f);
		pointsText.text = ((!(PlayerPrefs.GetString("langver", "en") == "pl")) ? "SPECIAL ABILITY" : "UMIEJĘTNOŚĆ SPECJALNA");
	}

	private bool BuyAbility(int cost)
	{
		if ((float)cost <= ultimatePoints)
		{
			ultimatePoints -= cost;
			return true;
		}
		return false;
	}

	private void AnimateHighlightedText()
	{
		if (highlightedAbilityText == null)
		{
			highlightedAbilityText = FindAnyObjectByType<ScpInterfaces>().Scp106_ability_highlight;
			return;
		}
		highlightedString = string.Empty;
		if (PlayerPrefs.GetString("langver", "en") == "pl")
		{
			if (highlightID == 1)
			{
				highlightedString = "STWÓRZ PRZEJŚCIE";
			}
			if (highlightID == 2)
			{
				highlightedString = "UŻYJ PRZEJŚCIA";
			}
		}
		else
		{
			if (highlightID == 1)
			{
				highlightedString = "CREATE SINKHOLE";
			}
			if (highlightID == 2)
			{
				highlightedString = "USE SINKHOLE";
			}
		}
		if (highlightedString != highlightedAbilityText.text)
		{
			if (highlightedAbilityText.canvasRenderer.GetAlpha() > 0f)
			{
				highlightedAbilityText.canvasRenderer.SetAlpha(highlightedAbilityText.canvasRenderer.GetAlpha() - Time.deltaTime * 4f);
			}
			else
			{
				highlightedAbilityText.text = highlightedString;
			}
		}
		else if (highlightedAbilityText.canvasRenderer.GetAlpha() < 1f && highlightedString != string.Empty)
		{
			highlightedAbilityText.canvasRenderer.SetAlpha(highlightedAbilityText.canvasRenderer.GetAlpha() + Time.deltaTime * 4f);
		}
	}

	private void CheckForInventoryInput()
	{
		if (base.isLocalPlayer)
		{
			if (popup106 == null)
			{
				popup106 = FindAnyObjectByType<ScpInterfaces>().Scp106_eq;
				return;
			}
			bool flag = (CursorManager.scp106 = iAm106 & Input.GetButton("Inventory"));
			popup106.SetActive(flag);
			fpc.m_MouseLook.scp106_eq = flag;
		}
	}

	public void Init(int classID, Class c)
	{
		iAm106 = classID == 3;
		sameClass = c.team == Team.SCP;
		if (!base.isLocalPlayer)
		{
			return;
		}
		GameObject[] array = GameObject.FindGameObjectsWithTag("Door");
		GameObject[] array2 = GameObject.FindGameObjectsWithTag("SecondDoor");
		GameObject[] array3 = array;
		foreach (GameObject gameObject in array3)
		{
			if (gameObject.GetComponent<MeshCollider>() != null)
			{
				if (gameObject.GetComponent<MeshCollider>().convex)
				{
					gameObject.GetComponent<Collider>().isTrigger = iAm106;
				}
			}
			else
			{
				gameObject.GetComponent<Collider>().isTrigger = iAm106;
			}
		}
		GameObject[] array4 = array2;
		foreach (GameObject gameObject2 in array4)
		{
			if (gameObject2.GetComponent<MeshCollider>() != null)
			{
				if (gameObject2.GetComponent<MeshCollider>().convex)
				{
					gameObject2.GetComponent<Collider>().isTrigger = iAm106;
				}
				else
				{
					gameObject2.GetComponent<MeshCollider>().enabled = !iAm106;
				}
			}
			else
			{
				gameObject2.GetComponent<Collider>().isTrigger = iAm106;
			}
		}
	}

	public void Contain(bool isScp)
	{
		Object.Instantiate(screamsPrefab);
		if (base.isLocalPlayer && iAm106)
		{
			ultimatePoints = 0f;
			StopAllCoroutines();
			StartCoroutine(ContainAnimation(isScp));
		}
	}

	public void GotoPD()
	{
		if (base.isLocalPlayer)
		{
			base.transform.position = Vector3.down * 2000f;
		}
	}

	public void DeletePortal()
	{
		if (portalPosition.y < 900f)
		{
			portalPrefab = null;
			portalPosition = Vector3.zero;
		}
	}

	public void UseTeleport()
	{
		if (!(portalPrefab == null))
		{
			if (BuyAbility(100) && portalPosition != Vector3.zero)
			{
				StartCoroutine(DoTeleportAnimation());
			}
			else
			{
				StartCoroutine(HighlightPointsText());
			}
		}
	}

	private void SetPortalPosition(Vector3 oldPos, Vector3 newPos)
	{
		StartCoroutine(DoPortalSetupAnimation());
	}

	public void CreatePortalInCurrentPosition()
	{
		if (BuyAbility(100))
		{
			if (base.isLocalPlayer)
			{
				CmdMakePortal(base.gameObject);
			}
		}
		else
		{
			StartCoroutine(HighlightPointsText());
		}
	}

	private IEnumerator ContainAnimation(bool b)
	{
		portalPosition = Vector3.zero;
		Invoke("Kill", 22f);
		VignetteAndChromaticAberration vaca = GetComponentInChildren<VignetteAndChromaticAberration>();
		fpc.m_JumpSpeed = 0f;
		goingViaThePortal = true;
		yield return new WaitForSeconds(15f);
		float y = base.transform.position.y - 2.5f;
		fpc.noclip = true;
		while (base.transform.position.y > y && ccm.curClass != 2)
		{
			if (base.transform.position.y - 2f < y)
			{
				vaca.intensity += Time.deltaTime / 2f;
			}
			vaca.intensity = Mathf.Clamp(vaca.intensity, 0.036f, 1f);
			base.transform.position += Vector3.down * Time.deltaTime / 2f;
			yield return new WaitForEndOfFrame();
		}
		fpc.noclip = false;
		if (b)
		{
			CmdAnnounceContaining();
			Kill();
		}
		goingViaThePortal = false;
	}

	private void Kill()
	{
		if (ccm.curClass != 2)
		{
			GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(999999f, "WORLD", "CONTAIN"), base.gameObject);
		}
	}

	private IEnumerator HighlightPointsText()
	{
		if (!isHighlightingPoints)
		{
			isHighlightingPoints = true;
			while ((double)pointsText.color.g > 0.05)
			{
				pointsText.color = Color.Lerp(pointsText.color, Color.red, 10f * Time.deltaTime);
				yield return new WaitForEndOfFrame();
			}
			while ((double)pointsText.color.g < 0.95)
			{
				pointsText.color = Color.Lerp(pointsText.color, Color.white, 10f * Time.deltaTime);
				yield return new WaitForEndOfFrame();
			}
			isHighlightingPoints = false;
		}
	}

	private IEnumerator DoPortalSetupAnimation()
	{
		while (portalPrefab == null)
		{
			portalPrefab = GameObject.Find("SCP106_PORTAL");
			yield return new WaitForEndOfFrame();
		}
		if (portalPosition != portalPrefab.transform.position)
		{
			Animator portalAnim = portalPrefab.GetComponent<Animator>();
			portalAnim.SetBool("activated", false);
			yield return new WaitForSeconds(1f);
			portalPrefab.transform.position = portalPosition;
			portalAnim.SetBool("activated", true);
		}
	}

	private IEnumerator DoTeleportAnimation()
	{
		if (!(portalPrefab != null) || goingViaThePortal)
		{
			yield break;
		}
		goingViaThePortal = true;
		VignetteAndChromaticAberration vaca = GetComponentInChildren<VignetteAndChromaticAberration>();
		fpc.noclip = true;
		float y = base.transform.position.y - 2.5f;
		float duration = 0f;
		while (base.transform.position.y > y && duration < 10f)
		{
			duration += Time.deltaTime;
			if (base.transform.position.y - 2f < y)
			{
				vaca.intensity += Time.deltaTime / 2f * teleportSpeed;
			}
			vaca.intensity = Mathf.Clamp(vaca.intensity, 0.036f, 1f);
			base.transform.position += Vector3.down * Time.deltaTime / 2f * teleportSpeed;
			yield return new WaitForEndOfFrame();
		}
		if (portalPosition == Vector3.zero)
		{
			GetComponent<PlayerStats>().Explode();
		}
		base.transform.position = portalPrefab.transform.position - Vector3.up * 1.5f;
		y = base.transform.position.y + 3f;
		while (base.transform.position.y < y)
		{
			base.transform.position += Vector3.up * Time.deltaTime / 2f * teleportSpeed;
			if (base.transform.position.y + 2f > y)
			{
				vaca.intensity -= Time.deltaTime / 2f * teleportSpeed;
			}
			vaca.intensity = Mathf.Clamp(vaca.intensity, 0.036f, 1f);
			yield return new WaitForEndOfFrame();
		}
		fpc.noclip = false;
		goingViaThePortal = false;
	}

	[Command(channel = 4)]
	public void CmdMakePortal(GameObject go)
	{
		go.GetComponent<Scp106PlayerScript>().portalPosition = go.transform.position + Vector3.down * 1.23f;
	}

	[Command(channel = 2)]
	private void CmdMovePlayer(GameObject ply, int t)
	{
		if (ServerTime.CheckSynchronization(t))
		{
			RpcMovePlayer(ply);
		}
	}

	[ClientRpc]
	private void RpcMovePlayer(GameObject ply)
	{
		ply.GetComponent<Scp106PlayerScript>().GotoPD();
	}

	private void HurtPlayer(GameObject ply, string id)
	{
		Hitmarker.Hit(1.5f);
		GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(40f, id, "SCP:106"), ply);
	}

	[Command(channel = 2)]
	private void CmdAnnounceContaining()
	{
		RpcAnnounceContaining();
	}

	[ClientRpc(channel = 2)]
	private void RpcAnnounceContaining()
	{
		Object.Instantiate(containAnnouncePrefab);
	}

	private void OnTriggerStay(Collider other)
	{
		if (base.isLocalPlayer && ccm.curClass == 3 && (other.transform.tag == "Door" || other.transform.tag == "SecondDoor"))
		{
			fpc.m_WalkSpeed = 1f;
			fpc.m_RunSpeed = 1f;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (base.isLocalPlayer && ccm.curClass == 3)
		{
			fpc.m_WalkSpeed = ccm.klasy[ccm.curClass].walkSpeed;
			fpc.m_RunSpeed = ccm.klasy[ccm.curClass].runSpeed;
		}
	}
}
