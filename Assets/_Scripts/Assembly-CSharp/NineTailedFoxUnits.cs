using TMPro;
using UnityEngine;
using Mirror;

public class NineTailedFoxUnits : NetworkBehaviour
{
	public string[] names;

	public readonly SyncList<string> list = new SyncList<string>();

	private CharacterClassManager ccm;

	private TextMeshProUGUI txtlist;

	private NineTailedFoxUnits host;

	private static int kListlist;

	private void AddUnit(string unit)
	{
		list.Add(unit);
	}

	private string GenerateName()
	{
		return names[Random.Range(0, names.Length)] + "-" + Random.Range(1, 20);
	}

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
		txtlist = GameObject.Find("NTFlist").GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (host == null)
		{
			GameObject gameObject = GameObject.Find("Host");
			if (gameObject != null)
			{
				host = gameObject.GetComponent<NineTailedFoxUnits>();
			}
			return;
		}
		txtlist.text = string.Empty;
		if (ccm.curClass <= 0 || ccm.klasy[ccm.curClass].team != Team.MTF)
		{
			return;
		}
		for (int i = 0; i < host.list.Count; i++)
		{
			if (i == ccm.ntfUnit)
			{
				TextMeshProUGUI textMeshProUGUI = txtlist;
				textMeshProUGUI.text = textMeshProUGUI.text + "<u>" + host.GetNameById(i) + "</u>";
			}
			else
			{
				txtlist.text += host.GetNameById(i);
			}
			txtlist.text += "\n";
		}
	}

	public int NewName()
	{
		int num = 0;
		string text = GenerateName();
		while (list.Contains(text) && num < 100)
		{
			num++;
			text = GenerateName();
		}
		AddUnit(text);
		return list.Count - 1;
	}

	public string GetNameById(int id)
	{
		return list[id];
	}
}
