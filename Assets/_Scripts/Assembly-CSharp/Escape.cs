using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Escape : MonoBehaviour
{
	private CharacterClassManager ccm;

	private Text respawnText;

	private bool escaped;

	public Vector3 worldPosition;

	public int radius = 10;

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
		respawnText = GameObject.Find("Respawn Text").GetComponent<Text>();
	}

	private void Update()
	{
		if (Vector3.Distance(base.transform.position, worldPosition) < (float)radius)
		{
			EscapeFromFacility();
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(worldPosition, radius);
	}

	private void EscapeFromFacility()
	{
		if (escaped)
		{
			return;
		}
		if (ccm.klasy[ccm.curClass].team == Team.RSC)
		{
			escaped = true;
			ccm.CmdForceClass(4, base.gameObject);
			ccm.RegisterEscape(false);
			if (PlayerPrefs.GetString("langver", "en") == "pl")
			{
				StartCoroutine(EscapeAnim("Uciekłeś jako Naukowiec i wstąpiłeś do oddziałów MFO."));
			}
			else
			{
				StartCoroutine(EscapeAnim("You escaped as the Scientist and joined the MTF units."));
			}
		}
		if (ccm.klasy[ccm.curClass].team == Team.CDP)
		{
			escaped = true;
			ccm.CmdForceClass(2, base.gameObject);
			ccm.RegisterEscape(true);
			if (PlayerPrefs.GetString("langver", "en") == "pl")
			{
				StartCoroutine(EscapeAnim("Uciekłeś jako Klasa D i wszedłeś w tryb obserwatora."));
			}
			else
			{
				StartCoroutine(EscapeAnim("You escaped as the Class D and entered in spectator mode."));
			}
		}
	}

	private IEnumerator EscapeAnim(string txt)
	{
		CanvasRenderer cr = respawnText.GetComponent<CanvasRenderer>();
		cr.SetAlpha(0f);
		respawnText.text = txt;
		while (cr.GetAlpha() < 1f)
		{
			cr.SetAlpha(cr.GetAlpha() + 0.1f);
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(2f);
		escaped = false;
		yield return new WaitForSeconds(3f);
		while (cr.GetAlpha() > 0f)
		{
			cr.SetAlpha(cr.GetAlpha() - 0.1f);
			yield return new WaitForSeconds(0.1f);
		}
	}
}
