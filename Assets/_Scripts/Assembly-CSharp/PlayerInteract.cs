using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerInteract : NetworkBehaviour
{
	public GameObject playerCamera;

	public LayerMask mask;

	public float raycastMaxDistance;

	private void AllowToOpenDoor(GameObject doorName)
	{
		Door.RoomAccessConditions roomAccessConditions = null;
		bool flag = GetComponent<CharacterClassManager>().klasy[GetComponent<CharacterClassManager>().curClass].team == Team.SCP;
		if (!flag)
		{
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject in players)
			{
				if (gameObject.GetComponent<Scp079PlayerScript>().lockedDoor == doorName)
				{
					CmdPlayDoorSound(doorName);
					GameObject.Find("Lock Denied Text").GetComponent<Text>().enabled = true;
					Invoke("DisableLockText", 1f);
					return;
				}
			}
		}
		if (GetComponent<Inventory>().curItem >= 0)
		{
			roomAccessConditions = GetComponent<Inventory>().availableItems[GetComponent<Inventory>().curItem].optionalKeycardSetup;
		}
		Door.RoomAccessConditions conditions = doorName.GetComponent<Door>().conditions;
		if (!conditions.safe_scp && !conditions.allow_detonate && !conditions.check_point && !conditions.exit_gate && !conditions.highlvl_arm && !conditions.intercom_room && !conditions.lowlvl_arm && !conditions.medium_scp && !conditions.midlvl_arm && !conditions.violent_scp)
		{
			CmdOpenDoor(doorName);
			return;
		}
		if (GetComponent<Inventory>().curItem < 0)
		{
			if (flag && conditions.check_point)
			{
				CmdOpenDoor(doorName);
				return;
			}
			CmdPlayDoorSound(doorName);
			GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = true;
			Invoke("DisableDeniedText", 1f);
			return;
		}
		bool flag2 = false;
		if (conditions.allow_detonate && roomAccessConditions.allow_detonate)
		{
			flag2 = true;
		}
		else if (conditions.check_point && (flag || roomAccessConditions.check_point))
		{
			flag2 = true;
		}
		else if (conditions.exit_gate && roomAccessConditions.exit_gate)
		{
			flag2 = true;
		}
		else if (conditions.highlvl_arm && roomAccessConditions.highlvl_arm)
		{
			flag2 = true;
		}
		else if (conditions.intercom_room && roomAccessConditions.intercom_room)
		{
			flag2 = true;
		}
		else if (conditions.lowlvl_arm && roomAccessConditions.lowlvl_arm)
		{
			flag2 = true;
		}
		else if (conditions.medium_scp && roomAccessConditions.medium_scp)
		{
			flag2 = true;
		}
		else if (conditions.midlvl_arm && roomAccessConditions.midlvl_arm)
		{
			flag2 = true;
		}
		else if (conditions.safe_scp && roomAccessConditions.safe_scp)
		{
			flag2 = true;
		}
		else if (conditions.violent_scp && roomAccessConditions.violent_scp)
		{
			flag2 = true;
		}
		else
		{
			CmdPlayDoorSound(doorName);
			GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = true;
			Invoke("DisableDeniedText", 1f);
		}
		if (flag2)
		{
			CmdOpenDoor(doorName);
		}
	}

	private void Update()
	{
		RaycastHit hitInfo;
		if (!base.isLocalPlayer || !Input.GetButtonDown("Interact") || GetComponent<CharacterClassManager>().curClass == 2 || !Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hitInfo, raycastMaxDistance, mask))
		{
			return;
		}
		if (hitInfo.transform.CompareTag("Door"))
		{
			string text = hitInfo.transform.name;
			AllowToOpenDoor(GameObject.Find(text));
		}
		else if (hitInfo.transform.CompareTag("SecondDoor"))
		{
			string text2 = hitInfo.transform.name;
			text2 = text2.Remove(text2.IndexOf("-second"));
			AllowToOpenDoor(GameObject.Find(text2));
		}
		else if (hitInfo.transform.CompareTag("DoorButton"))
		{
			string text3 = hitInfo.transform.name;
			text3 = text3.Remove(text3.IndexOf("-button"));
			AllowToOpenDoor(GameObject.Find(text3));
		}
		else if (hitInfo.transform.CompareTag("914_knob"))
		{
			CmdChange914_State(hitInfo.transform.name);
		}
		else if (hitInfo.transform.CompareTag("914_use"))
		{
			GetComponent<Scp914_Controller>().Refine(hitInfo.transform.name);
		}
		else if (hitInfo.transform.CompareTag("Lever"))
		{
			CmdChangeLeverState(GameObject.Find(hitInfo.transform.name + "_Controller"));
		}
		else if (hitInfo.transform.CompareTag("AW_Button"))
		{
			Door.RoomAccessConditions roomAccessConditions = null;
			if (GetComponent<Inventory>().curItem >= 0)
			{
				roomAccessConditions = GetComponent<Inventory>().availableItems[GetComponent<Inventory>().curItem].optionalKeycardSetup;
			}
			if (roomAccessConditions != null)
			{
				if (roomAccessConditions.allow_detonate)
				{
					CmdSwitchAWButton(hitInfo.transform.name);
					return;
				}
				GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = true;
				Invoke("DisableDeniedText", 1f);
				CmdSwitchAWButton("DENIED");
			}
			else
			{
				GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = true;
				Invoke("DisableDeniedText", 1f);
				CmdSwitchAWButton("DENIED");
			}
		}
		else if (hitInfo.transform.CompareTag("AW_Detonation"))
		{
			if (GameObject.Find("Lever_Alpha_Controller").GetComponent<LeverButton>().GetState())
			{
				CmdDetonateWarhead();
				return;
			}
			GameObject.Find("Alpha Denied Text").GetComponent<Text>().enabled = true;
			Invoke("DisableAlphaText", 1f);
		}
		else if (hitInfo.transform.CompareTag("ElevatorButton"))
		{
			UseElevator(hitInfo.transform.name);
		}
		else if (hitInfo.transform.CompareTag("294"))
		{
			Inventory component = GetComponent<Inventory>();
			if (component.curItem == 17)
			{
				for (int i = 0; i < component.items.Count; i++)
				{
					if (component.items[i].id == 17)
					{
						component.items.RemoveAt(i);
						CmdUse294(hitInfo.transform.name);
						component.curItem = -1;
						break;
					}
				}
			}
			else
			{
				HintManager.singleton.AddHint(3);
			}
		}
		else if (hitInfo.transform.CompareTag("FemurBreaker") && !FindAnyObjectByType<OneOhSixContainer>().used && Object.FindObjectOfType<LureSubjectContainer>().allowContain)
		{
			CmdConatin106();
		}
	}

	public void UseElevator(string id)
	{
		LiftIdentity[] array = Object.FindObjectsOfType<LiftIdentity>();
		LiftIdentity[] array2 = array;
		foreach (LiftIdentity liftIdentity in array2)
		{
			if (liftIdentity.identity == id)
			{
				CmdUseLift(liftIdentity.gameObject, id);
			}
		}
	}

	[Command(channel = 4)]
	private void CmdUseLift(GameObject ply, string id)
	{
		ply.GetComponent<LiftIdentity>().Toggle();
	}

	[Command(channel = 4)]
	private void CmdChange914_State(string label)
	{
		GameObject gameObject = GameObject.Find(label);
		gameObject.GetComponentInParent<Scp914>().IncrementState();
	}

	[Command(channel = 4)]
	private void CmdSwitchAWButton(string label)
	{
		GameObject gameObject = GameObject.Find("DetonationController");
		gameObject.GetComponentInParent<AlphaWarheadButtonUnlocker>().ChangeButtonStage(label);
	}

	[Command(channel = 4)]
	private void CmdDetonateWarhead()
	{
		GameObject.Find("Host").GetComponent<AlphaWarheadDetonationController>().StartDetonation();
	}

	[Command(channel = 14)]
	private void CmdOpenDoor(GameObject doorID)
	{
		doorID.GetComponent<Door>().ChangeState();
		doorID.GetComponent<Door>().PlaySound(true);
	}

	[Command(channel = 10)]
	private void CmdPlayDoorSound(GameObject doorID)
	{
		doorID.GetComponent<Door>().PlaySound(false);
	}

	[Command(channel = 4)]
	private void CmdChangeLeverState(GameObject lever)
	{
		lever.GetComponent<LeverButton>().Switch();
	}

	[Command(channel = 4)]
	private void CmdConatin106()
	{
		RpcContain106();
		FindAnyObjectByType<OneOhSixContainer>().used = true;
	}

	[ClientRpc(channel = 4)]
	private void RpcContain106()
	{
		bool flag = false;
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.GetComponent<CharacterClassManager>().curClass == 3)
			{
				gameObject.GetComponent<Scp106PlayerScript>().Contain(true);
				flag = true;
			}
		}
		if (!flag && base.isLocalPlayer)
		{
			GetComponent<Scp106PlayerScript>().Contain(false);
		}
	}

	[Command(channel = 4)]
	private void CmdUse294(string label)
	{
		GameObject.Find(label).GetComponent<Scp294>().Buy();
	}

	private void DisableDeniedText()
	{
		GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = false;
		HintManager.singleton.AddHint(1);
	}

	private void DisableAlphaText()
	{
		GameObject.Find("Alpha Denied Text").GetComponent<Text>().enabled = false;
		HintManager.singleton.AddHint(2);
	}

	private void DisableLockText()
	{
		GameObject.Find("Lock Denied Text").GetComponent<Text>().enabled = false;
	}
}
