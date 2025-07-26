using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameConsole;
using UnityEngine;
using Mirror;
using UnityEngine.PostProcessing;

public class CharacterClassManager : NetworkBehaviour
{
	[SyncVar]
	public int ntfUnit;

	public float ciPercentage;

	public int forceClass = -1;

	[SerializeField]
	private AudioClip bell;

	[SerializeField]
	private AudioClip bell_dead;

	[HideInInspector]
	public GameObject myModel;

	[HideInInspector]
	public GameObject charCamera;

	public Class[] klasy;

	public List<Team> classTeamQueue = new List<Team>();

	[SyncVar(hook = nameof(SetClassID))]
	public int curClass;

	private int seed;

	private GameObject plyCam;

	public GameObject unfocusedCamera;

	[SyncVar]
	public bool roundStarted;

	private float aliveTime;

	private int prevId = -1;

	private void Start()
	{
		forceClass = ConfigFile.GetInt("server_forced_class", -1);
		ciPercentage = ConfigFile.GetInt("ci_on_start_percent", 10);
		StartCoroutine("Init");
		string text = ConfigFile.GetString("team_respawn_queue", "401431403144144") + "...........................";
		classTeamQueue.Clear();
		for (int i = 0; i < text.Length; i++)
		{
			int result = 4;
			if (!int.TryParse(text[i].ToString(), out result))
			{
				result = 4;
			}
			classTeamQueue.Add((Team)result);
		}
		if (!base.isLocalPlayer && TutorialManager.status)
		{
			ApplyProperties();
		}
	}

	private IEnumerator Init()
	{
		GameObject host = null;
		while (host == null)
		{
			host = GameObject.Find("Host");
			yield return new WaitForEndOfFrame();
		}
		while (seed == 0)
		{
			seed = host.GetComponent<RandomSeedSync>().seed;
			FindAnyObjectByType<Console>().UpdateValue("seed", seed.ToString());
		}
		if (base.isLocalPlayer)
		{
			yield return new WaitForSeconds(2f);
			if (base.isServer)
			{
				CursorManager.roundStarted = true;
				RoundStart rs = RoundStart.singleton;
				if (TutorialManager.status)
				{
					ForceRoundStart();
				}
				else
				{
					rs.ShowButton();
					int timeLeft = 20;
					int maxPlayers = 1;
					while (rs.info != "started")
					{
						if (maxPlayers > 1)
						{
							timeLeft--;
						}
						int count = PlayerManager.singleton.players.Length;
						if (count > maxPlayers)
						{
							maxPlayers = count;
							timeLeft = ((timeLeft < 5) ? 5 : ((timeLeft < 10) ? 10 : ((timeLeft >= 15) ? 20 : 15)));
							if (maxPlayers == NetworkManager.singleton.maxConnections)
							{
								timeLeft = 0;
							}
						}
						if (timeLeft > 0)
						{
							CmdUpdateStartText(timeLeft.ToString());
						}
						else
						{
							ForceRoundStart();
						}
						yield return new WaitForSeconds(1f);
					}
				}
				CursorManager.roundStarted = false;
				CmdStartRound();
				SetRandomRoles();
			}
			else
			{
				while (!host.GetComponent<CharacterClassManager>().roundStarted)
				{
					yield return new WaitForEndOfFrame();
				}
				yield return new WaitForSeconds(2f);
				if (curClass < 0)
				{
					CmdForceClass(2, base.gameObject);
				}
			}
		}
		InvokeRepeating("InitSCPs", 0.2f, 5f);
	}

	public void ForceRoundStart()
	{
		CmdUpdateStartText("started");
	}

	[Command(channel = 7)]
	private void CmdUpdateStartText(string str)
	{
		RoundStart.singleton.info = str;
	}

	public void InitSCPs()
	{
		if (curClass != -1 && !TutorialManager.status)
		{
			Class c = klasy[curClass];
			GetComponent<Scp173PlayerScript>().Init(curClass, c);
			GetComponent<Scp106PlayerScript>().Init(curClass, c);
			GetComponent<Scp457PlayerScript>().Init(curClass, c);
			GetComponent<Scp049PlayerScript>().Init(curClass, c);
			GetComponent<Scp049_2PlayerScript>().Init(curClass, c);
			GetComponent<Scp079PlayerScript>().Init(curClass, c);
		}
	}

	public void RegisterEscape(bool isD)
	{
		CmdRegisterEscape(isD);
	}

	[Command(channel = 2)]
	private void CmdRegisterEscape(bool isD)
	{
		RoundSummary component = GameObject.Find("Host").GetComponent<RoundSummary>();
		if (isD)
		{
			component.summary.classD_escaped++;
		}
		else
		{
			component.summary.scientists_escaped++;
		}
	}

	public void ApplyProperties()
	{
		Class obj = klasy[curClass];
		InitSCPs();
		Inventory component = GetComponent<Inventory>();
		if (TutorialManager.status || base.isLocalPlayer)
		{
			GetComponent<Radio>().UpdateClass();
			GetComponent<Handcuffs>().CmdTarget(null);
			GetComponent<Spectator>().Init();
			GetComponent<Searching>().Init((obj.team == Team.SCP) | (obj.team == Team.RIP));
			if (obj.team == Team.RIP)
			{
				if (base.isLocalPlayer)
				{
					component.items.Clear();
					component.curItem = -1;
					GetComponent<FirstPersonController>().enabled = false;
					FindAnyObjectByType<StartScreen>().PlayAnimation(curClass);
					GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(bell_dead);
					base.transform.position = new Vector3(0f, 2048f, 0f);
					base.transform.rotation = Quaternion.Euler(Vector3.zero);
					GetComponent<PlayerStats>().maxHP = obj.maxHP;
					CmdSetPlayerHealth(obj.maxHP, base.gameObject);
					unfocusedCamera.GetComponent<Camera>().enabled = false;
					unfocusedCamera.GetComponent<PostProcessingBehaviour>().enabled = false;
				}
				RefreshPlyModel();
			}
			else
			{
				if (base.isLocalPlayer)
				{
					GameObject gameObject = null;
					gameObject = FindAnyObjectByType<SpawnpointManager>().GetRandomPosition(curClass);
					if (gameObject != null)
					{
						base.transform.position = gameObject.transform.position;
						base.transform.rotation = gameObject.transform.rotation;
					}
					else
					{
						base.transform.position = GetComponent<PlayerStats>().deathPosition;
					}
					component.items.Clear();
					component.curItem = -1;
					int[] startItems = obj.startItems;
					foreach (int id in startItems)
					{
						component.AddItem(id);
					}
					FindAnyObjectByType<StartScreen>().PlayAnimation(curClass);
					if (!GetComponent<HorrorSoundController>().horrorSoundSource.isPlaying)
					{
						GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(bell);
					}
					Invoke("EnableFPC", 0.2f);
				}
				RefreshPlyModel();
				if (base.isLocalPlayer)
				{
					GetComponent<Radio>().curPreset = 0;
					GetComponent<Radio>().CmdUpdatePreset(0);
					GetComponent<AmmoBox>().SetAmmoAmount();
					FirstPersonController component2 = GetComponent<FirstPersonController>();
					PlayerStats component3 = GetComponent<PlayerStats>();
					if (obj.postprocessingProfile != null && GetComponentInChildren<PostProcessingBehaviour>() != null)
					{
						GetComponentInChildren<PostProcessingBehaviour>().profile = obj.postprocessingProfile;
					}
					unfocusedCamera.GetComponent<Camera>().enabled = true;
					unfocusedCamera.GetComponent<PostProcessingBehaviour>().enabled = true;
					component2.m_WalkSpeed = obj.walkSpeed;
					component2.m_RunSpeed = obj.runSpeed;
					component2.m_UseHeadBob = obj.useHeadBob;
					component2.m_FootstepSounds = obj.stepClips;
					component2.m_JumpSpeed = obj.jumpSpeed;
					GetComponent<WeaponManager>().SetRecoil(obj.classRecoil);
					int num = (component3.maxHP = obj.maxHP);
					CmdSetPlayerHealth(num, base.gameObject);
					FindAnyObjectByType<UserMainInterface>().lerpedHP = num;
				}
				else
				{
					GetComponent<PlayerStats>().maxHP = obj.maxHP;
				}
			}
			if (base.isLocalPlayer)
			{
				FindAnyObjectByType<InventoryDisplay>().isSCP = (curClass == 2) | (obj.team == Team.SCP);
				FindAnyObjectByType<InterfaceColorAdjuster>().ChangeColor(obj.classColor);
			}
		}
		else
		{
			RefreshPlyModel();
		}
	}

	private void EnableFPC()
	{
		GetComponent<FirstPersonController>().enabled = true;
	}

	private void RefreshPlyModel()
	{
		if (myModel != null)
		{
			Object.Destroy(myModel);
		}
		Class obj = klasy[curClass];
		if (obj.team != Team.RIP)
		{
			GameObject gameObject = Object.Instantiate(obj.model_player);
			gameObject.transform.SetParent(base.gameObject.transform);
			gameObject.transform.localPosition = obj.model_offset.position;
			gameObject.transform.localRotation = Quaternion.Euler(obj.model_offset.rotation);
			gameObject.transform.localScale = obj.model_offset.scale;
			myModel = gameObject;
			if (myModel.GetComponent<Animator>() != null)
			{
				GetComponent<AnimationController>().animator = myModel.GetComponent<Animator>();
			}
			if (base.isLocalPlayer)
			{
				if (myModel.GetComponent<Renderer>() != null)
				{
					myModel.GetComponent<Renderer>().enabled = false;
				}
				Renderer[] componentsInChildren = myModel.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.enabled = false;
				}
				Collider[] componentsInChildren2 = myModel.GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren2)
				{
					if (collider.name != "LookingTarget")
					{
						collider.enabled = false;
					}
				}
			}
		}
		GetComponent<CapsuleCollider>().enabled = obj.team != Team.RIP;
	}

	public void SetClassID(int oldId, int newId)
	{
		if (newId != 2 || isLocalPlayer)
		{
			aliveTime = 0f;
			ApplyProperties();
		}
	}

	public void InstantiateRagdoll(int id)
	{
		if (id >= 0)
		{
			Class obj = klasy[curClass];
			GameObject gameObject = Object.Instantiate(obj.model_ragdoll);
			gameObject.transform.position = base.transform.position + obj.ragdoll_offset.position;
			gameObject.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles + obj.ragdoll_offset.rotation);
			gameObject.transform.localScale = obj.ragdoll_offset.scale;
		}
	}

	public void SetRandomRoles()
	{
		MTFRespawn component = GetComponent<MTFRespawn>();
		if (!base.isLocalPlayer || !base.isServer)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject item in array)
		{
			list.Add(item);
		}
		while (list.Count > 0)
		{
			int index = Random.Range(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		GameObject[] array2 = list2.ToArray();
		RoundSummary component2 = GetComponent<RoundSummary>();
		bool flag = false;
		if ((float)Random.Range(0, 100) < ciPercentage)
		{
			flag = true;
		}
		for (int j = 0; j < array2.Length; j++)
		{
			int num = ((forceClass != -1) ? forceClass : Find_Random_ID_Using_Defined_Team(classTeamQueue[j]));
			if (klasy[num].team == Team.CDP)
			{
				component2.summary.classD_start++;
			}
			if (klasy[num].team == Team.RSC)
			{
				component2.summary.scientists_start++;
			}
			if (klasy[num].team == Team.SCP)
			{
				component2.summary.scp_start++;
			}
			if (num == 4)
			{
				if (flag)
				{
					num = 8;
				}
				else
				{
					component.playersToNTF.Add(array2[j]);
				}
			}
			if (TutorialManager.status)
			{
				CmdSetPlayersClass(14, base.gameObject);
			}
			else if (num != 4)
			{
				CmdSetPlayersClass(num, array2[j]);
			}
		}
		component.SummonNTF();
	}

	[Command(channel = 7)]
	private void CmdStartRound()
	{
		if (!TutorialManager.status)
		{
			Door componentInChildren = GameObject.Find("MeshDoor173").GetComponentInChildren<Door>();
			componentInChildren.curCooldown = 25f;
			componentInChildren.InvokeDeductCooldown();
			FindAnyObjectByType<ChopperAutostart>().isLanded = false;
		}
		roundStarted = true;
	}

	[Command(channel = 2)]
	private void CmdSetPlayersClass(int classid, GameObject ply)
	{
		ply.GetComponent<CharacterClassManager>().curClass = classid;
	}

	[Command(channel = 2)]
	private void CmdSetPlayerHealth(int hp, GameObject go)
	{
		go.GetComponent<PlayerStats>().health = hp;
	}

	[Command(channel = 2)]
	public void CmdForceClass(int id, GameObject go)
	{
		go.GetComponent<CharacterClassManager>().curClass = id;
	}

	private int Find_Random_ID_Using_Defined_Team(Team team)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < klasy.Length; i++)
		{
			if (klasy[i].team == team && !klasy[i].banClass)
			{
				list.Add(i);
			}
		}
		int index = Random.Range(0, list.Count);
		if (klasy[list[index]].team == Team.SCP)
		{
			klasy[list[index]].banClass = true;
		}
		return list[index];
	}

	public bool SpawnProtection()
	{
		return aliveTime < 2f;
	}

	private void Update()
	{
		if (curClass == 2)
		{
			aliveTime = 0f;
		}
		else
		{
			aliveTime += Time.deltaTime;
		}
		if (base.isLocalPlayer && ServerStatic.isDedicated)
		{
			CursorManager.isServerOnly = true;
		}
		if (prevId != curClass)
		{
			RefreshPlyModel();
			prevId = curClass;
		}
		if (base.name == "Host")
		{
			Radio.roundStarted = roundStarted;
		}
	}
}
