using TMPro;
using UnityEngine;
using Mirror;

public class RagdollManager : NetworkBehaviour
{
	public LayerMask inspectionMask;

	private Transform cam;

	private CharacterClassManager ccm;

	private TextMeshProUGUI txt;

	private bool isEN = true;

	public void SpawnRagdoll(Vector3 pos, Quaternion rot, int classID, PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick)
	{
		Class obj = GetComponent<CharacterClassManager>().klasy[classID];
		GameObject gameObject = Object.Instantiate(obj.model_ragdoll, pos + obj.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + obj.ragdoll_offset.rotation));
		NetworkServer.Spawn(gameObject);
		gameObject.GetComponent<Ragdoll>().owner = new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, classID);
		gameObject.GetComponent<Ragdoll>().allowRecall = allowRecall;
		if (ragdollInfo.tool.Contains("SCP") || ragdollInfo.tool == "POCKET")
		{
			CmdRegisterScpFrag();
		}
	}

	private void Start()
	{
		isEN = PlayerPrefs.GetString("langver", "en") != "pl";
		txt = GameObject.Find("BodyInspection").GetComponentInChildren<TextMeshProUGUI>();
		cam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		ccm = GetComponent<CharacterClassManager>();
	}

	public void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		string text = string.Empty;
		RaycastHit hitInfo;
		if (Physics.Raycast(new Ray(cam.position, cam.forward), out hitInfo, 3f, inspectionMask))
		{
			Ragdoll componentInParent = hitInfo.transform.GetComponentInParent<Ragdoll>();
			if (componentInParent != null)
			{
				text = ((!isEN) ? "To ciało <b>[user]</b>, to [class]!\n\nPrzyczyna śmierci: [cause]" : "It's <b>[user]</b>'s body, he was [class]!\n\nCause of death: [cause]");
				text = text.Replace("[user]", componentInParent.owner.steamClientName);
				text = text.Replace("[cause]", GetCause(componentInParent.owner.deathCause, false));
				text = text.Replace("[class]", "<color=" + GetColor(ccm.klasy[componentInParent.owner.charclass].classColor) + ">" + ((!isEN) ? ccm.klasy[componentInParent.owner.charclass].fullName_pl : ccm.klasy[componentInParent.owner.charclass].fullName) + "</color>");
			}
		}
		txt.text = text;
	}

	public string GetColor(Color c)
	{
		Color32 color = new Color32((byte)(c.r * 255f), (byte)(c.g * 255f), (byte)(c.b * 255f), byte.MaxValue);
		return "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}

	[Command(channel = 2)]
	public void CmdRegisterScpFrag()
	{
		if (RoundSummary.host != null)
		{
			RoundSummary.host.summary.scp_frags++;
		}
	}

	public static string GetCause(PlayerStats.HitInfo info, bool ragdoll)
	{
		bool flag = PlayerPrefs.GetString("langver", "en") != "pl";
		string result = ((!flag) ? "Nieznana przyczyna śmierci." : "Unknown cause of death.");
		int result2 = -1;
		if (info.tool == "NUKE")
		{
			result = ((!flag) ? "Zgon od eksplozji." : "Died of an explosion.");
		}
		else if (info.tool == "FALLDOWN")
		{
			result = ((!flag) ? "Upadek zakończył jego żywot." : "The fall has ended his life.");
		}
		else if (info.tool == "LURE")
		{
			result = ((!flag) ? "Poświęcenie w celu zamknięcia SCP-106." : "Died to re-contain SCP-106.");
		}
		else if (info.tool == "POCKET")
		{
			result = ((!flag) ? "Zaginął w Wymiarze Łuzowym." : "Lost in the Pocket Dimension.");
		}
		else if (info.tool == "CONTAIN")
		{
			result = ((!flag) ? "To jest pozostałość po SCP-106." : "It's a remnant of SCP-106.");
		}
		else if (info.tool == "TESLA")
		{
			result = ((!flag) ? "Porażenie prądem o wysokim napięciu." : "High-voltage electric shock.");
		}
		else if (info.tool == "WALL")
		{
			result = ((!flag) ? "Zmiażdżony przez ciężki obiekt." : "Crushed by a heavy structure.");
		}
		else if (info.tool.Length > 7 && info.tool.Substring(0, 7) == "Weapon:" && int.TryParse(info.tool.Remove(0, 7), out result2) && result2 != -1)
		{
			GameObject gameObject = GameObject.Find("Host");
			WeaponManager.Weapon weapon = gameObject.GetComponent<WeaponManager>().weapons[result2];
			AmmoBox component = gameObject.GetComponent<AmmoBox>();
			result = ((!flag) ? "Na ciele widać wiele ran postrzałowych. Broń z której zostały oddane strzały najprawdopodobniej korzysta z amunicji " : "There are many gunshot wounds on the body. Most likely, a ") + component.types[weapon.ammoType].label + ((!flag) ? "." : " ammo type was used.");
		}
		else if (info.tool.Length > 4 && info.tool.Substring(0, 4) == "SCP:" && int.TryParse(info.tool.Remove(0, 4), out result2))
		{
			switch (result2)
			{
			case 173:
				result = ((!flag) ? "Skręcenie karku u podstawy czaszki." : "Snapped neck at the base of the skull.");
				break;
			case 106:
				result = ((!flag) ? "Nie przetrwał spotkania z SCP-106." : "Did not survive the meeting with SCP-106.");
				break;
			case 49:
			case 492:
				result = ((!flag) ? "Został wyleczony z pomoru." : "He was cured of a plague.");
				break;
			}
		}
		return result;
	}
}
