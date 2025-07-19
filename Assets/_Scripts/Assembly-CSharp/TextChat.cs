using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class TextChat : NetworkBehaviour
{
	public int messageDuration;

	private static Transform lply;

	public GameObject textMessagePrefab;

	private Transform attachParent;

	public bool enabledChat;

	private List<GameObject> msgs = new List<GameObject>();

	private static int kCmdCmdSendChat;

	private static int kRpcRpcSendChat;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			lply = base.transform;
		}
	}

	private void Update()
	{
		if (!base.isLocalPlayer || !enabledChat)
		{
			return;
		}
		for (int i = 0; i < msgs.Count; i++)
		{
			if (msgs[i] == null)
			{
				msgs.RemoveAt(i);
				break;
			}
			msgs[i].GetComponent<TextMessage>().position = msgs.Count - i - 1;
		}
		if (Input.GetKeyDown(KeyCode.Return))
		{
			SendChat("(づ｡◕\u203f\u203f◕｡)づ" + Random.Range(0, 4654), GetComponent<NicknameSync>().myNick, base.transform.position);
		}
	}

	private void SendChat(string msg, string nick, Vector3 position)
	{
		CmdSendChat(msg, nick, position);
	}

	[Command(channel = 2)]
	private void CmdSendChat(string msg, string nick, Vector3 pos)
	{
		RpcSendChat(msg, nick, pos);
	}

	[ClientRpc(channel = 2)]
	private void RpcSendChat(string msg, string nick, Vector3 pos)
	{
		if (Vector3.Distance(lply.position, pos) < 15f)
		{
			AddMsg(msg, nick);
		}
	}

	private void AddMsg(string msg, string nick)
	{
		while (msg.Contains("<"))
		{
			msg = msg.Replace("<", "＜");
		}
		while (msg.Contains(">"))
		{
			msg = msg.Replace(">", "＞");
		}
		string text = "<b>" + nick + "</b>: " + msg;
		GameObject gameObject = Object.Instantiate(textMessagePrefab);
		gameObject.transform.SetParent(attachParent);
		msgs.Add(gameObject);
		gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
		gameObject.transform.localScale = Vector3.one;
		gameObject.GetComponent<Text>().text = text;
		gameObject.GetComponent<TextMessage>().remainingLife = messageDuration;
		Object.Destroy(gameObject, messageDuration);
	}
}
