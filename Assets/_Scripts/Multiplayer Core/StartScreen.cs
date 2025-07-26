using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
	public GameObject popup;

	public Image black;

	public Text youare;

	public Text wmi;

	public Text wihtd;

	private bool isPL;

	private void Start()
	{
		isPL = PlayerPrefs.GetString("langver", "en") == "pl";
	}

	public void PlayAnimation(int classID)
	{
		StopAllCoroutines();
		StartCoroutine(Animate(classID));
	}

	private IEnumerator Animate(int classID)
	{
		black.gameObject.SetActive(true);
		GameObject host = GameObject.Find("Host");
		CharacterClassManager ccm = host.GetComponent<CharacterClassManager>();
		Class klasa = ccm.klasy[classID];
		youare.text = (TutorialManager.status ? string.Empty : ((!isPL) ? "YOU ARE" : "TWOJA ROLA:"));
		wmi.text = ((!isPL) ? klasa.fullName : klasa.fullName_pl);
		wmi.color = klasa.classColor;
		wihtd.text = ((!isPL) ? klasa.description : klasa.description_pl);
		while (popup.transform.localScale.x < 1f)
		{
			popup.transform.localScale += Vector3.one * Time.deltaTime * 2f;
			if (popup.transform.localScale.x > 1f)
			{
				popup.transform.localScale = Vector3.one;
			}
			yield return new WaitForEndOfFrame();
		}
		while (black.color.a > 0f)
		{
			black.color = new Color(0f, 0f, 0f, black.color.a - Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(7f);
		CanvasRenderer c1 = youare.GetComponent<CanvasRenderer>();
		CanvasRenderer c2 = wmi.GetComponent<CanvasRenderer>();
		CanvasRenderer c3 = wihtd.GetComponent<CanvasRenderer>();
		HintManager.singleton.AddHint(0);
		while (c1.GetAlpha() > 0f)
		{
			c1.SetAlpha(c1.GetAlpha() - Time.deltaTime / 2f);
			c2.SetAlpha(c2.GetAlpha() - Time.deltaTime / 2f);
			c3.SetAlpha(c3.GetAlpha() - Time.deltaTime / 2f);
			yield return new WaitForEndOfFrame();
		}
		black.gameObject.SetActive(false);
	}
}
