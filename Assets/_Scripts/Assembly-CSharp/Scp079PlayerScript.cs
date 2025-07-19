using Kino;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Scp079PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public bool iAm079;

	public bool sameClass;

	public LayerMask cameraMask;

	[Header("FOV relationships")]
	public AnimationCurve fovRelation;

	public AnimationCurve sliderRelation;

	[Header("Lockdown")]
	public float remainingLockdown;

	public GameObject lockedDoor;

	public float minLockTime = 13f;

	public float maxLockTime = 28f;

	private ScpInterfaces interfaces;

	private Camera cam;

	private Camera glowCam;

	private Image loadingCircle;

	private FirstPersonController fpc;

	private Interface079 gui;

	private SpectatorCamera spectCam;

	private Transform lookRotation;

	private AnalogGlitch glitchEffect;

	private GameObject curCamera;

	private GameObject interactable;

	private string interactableType = "unclassified";

	private float hackingTime;

	private RaycastHit hit;

	private bool isHacked;

	private float targetFov = 75f;

	private float offsetX;

	private float offsetY;

	private string liftID;

	private void Start()
	{
		if (!TutorialManager.status)
		{
			interfaces = FindAnyObjectByType<ScpInterfaces>();
			loadingCircle = interfaces.Scp049_loading;
			lookRotation = GameObject.Find("LookRotation_079").transform;
			if (base.isLocalPlayer)
			{
				fpc = GetComponent<FirstPersonController>();
				cam = FindAnyObjectByType<SpectatorCamera>().cam079;
				glowCam = FindAnyObjectByType<SpectatorCamera>().cam079_glow;
				glitchEffect = glowCam.GetComponent<AnalogGlitch>();
				gui = interfaces.Scp079_eq.GetComponent<Interface079>();
				gui.localplayer = base.gameObject;
			}
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		iAm079 = classID == 7;
		if (base.isLocalPlayer)
		{
			gui.gameObject.SetActive(iAm079);
			interfaces.Scp079_eq.SetActive(iAm079);
			cam.gameObject.SetActive(iAm079);
			if (!iAm079 && lockedDoor != null)
			{
				CmdLockDoor(null);
			}
			if (iAm079)
			{
				cam.transform.SetParent(null);
				RefreshCamerasLODs();
			}
		}
	}

	private void Update()
	{
		if (!TutorialManager.status)
		{
			CheckForInput();
			UpdateInteraction();
			MoveCamera();
			FovRelation();
			DeductLockDown();
		}
	}

	private void DeductLockDown()
	{
		if (remainingLockdown > 0f)
		{
			remainingLockdown -= Time.deltaTime;
			if (remainingLockdown <= 0f)
			{
				remainingLockdown = -1f;
			}
		}
		if (remainingLockdown == -1f)
		{
			remainingLockdown = 0f;
			CmdLockDoor(null);
		}
	}

	private void FovRelation()
	{
		if (base.isLocalPlayer && iAm079)
		{
			gui.transform.localScale = Vector3.one * fovRelation.Evaluate(cam.fieldOfView);
			gui.fovSlider.rectTransform.sizeDelta = new Vector2(sliderRelation.Evaluate(cam.fieldOfView), 13.25f);
		}
	}

	private void MoveCamera()
	{
		if (!base.isLocalPlayer || !iAm079)
		{
			return;
		}
		targetFov -= Input.GetAxis("Mouse ScrollWheel") * 35f;
		targetFov = Mathf.Clamp(targetFov, 25f, 85f);
		cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * 3.2f);
		glowCam.fieldOfView = cam.fieldOfView;
		if (hackingTime == 0f)
		{
			offsetY += Input.GetAxis("Horizontal") * Time.deltaTime * cam.fieldOfView * ((!Input.GetButton("Run")) ? 1.6f : 3.3f);
			offsetX -= Input.GetAxis("Vertical") * Time.deltaTime * cam.fieldOfView * ((!Input.GetButton("Run")) ? 1.6f : 3.3f);
			offsetX = Mathf.Clamp(offsetX, -20f, 70f);
		}
		else if (!isHacked)
		{
			MeshRenderer meshRenderer = hit.transform.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = hit.transform.GetComponentInParent<MeshRenderer>();
			}
			if (meshRenderer == null)
			{
				meshRenderer = hit.transform.GetComponentInChildren<MeshRenderer>();
			}
			lookRotation.LookAt(meshRenderer.bounds.center);
			offsetX = lookRotation.rotation.eulerAngles.x;
			offsetY = lookRotation.rotation.eulerAngles.y;
			if (offsetX > 180f)
			{
				offsetX -= 360f;
			}
		}
		cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, Quaternion.Euler(new Vector3(offsetX, offsetY, 0f)), Time.deltaTime * 9.5f);
	}

	private void CheckForInput()
	{
		if (iAm079 && base.isLocalPlayer)
		{
			if (Input.GetButtonDown("Fire1"))
			{
				Interact();
			}
			if (Input.GetButtonDown("Zoom"))
			{
				StopInteracting();
			}
		}
	}

	private void Interact()
	{
		if (!isHacked && interactableType == "unclassified")
		{
			if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 50f, cameraMask))
			{
				interactable = hit.transform.GetComponentInParent<CCTV_Camera>().gameObject;
				interactableType = RecognizeInteractable();
			}
			else if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
			{
				interactable = hit.transform.gameObject;
				interactableType = RecognizeInteractable();
			}
		}
		if (interactableType == "stop")
		{
			StopInteracting();
		}
	}

	public void StopInteracting()
	{
		isHacked = false;
		gui.ResetProgress();
		interactable = null;
		hackingTime = 0f;
		interactableType = "unclassified";
	}

	private void UpdateInteraction()
	{
		if (base.isLocalPlayer)
		{
			CursorManager.is079 = (iAm079 && hackingTime == 0f) || isHacked;
			glitchEffect.colorDrift = ((!CursorManager.is079) ? 0.15f : 0f);
		}
		if (!base.isLocalPlayer || !iAm079)
		{
			return;
		}
		gui.console.SetActive(isHacked);
		gui.progress_obj.SetActive(hackingTime != 0f && !isHacked);
		if (hackingTime != 0f)
		{
			gui.AddProgress(Time.deltaTime / hackingTime);
		}
		else
		{
			gui.ResetProgress();
		}
		if (!gui.Action())
		{
			return;
		}
		if (!isHacked)
		{
			isHacked = true;
			if (interactableType == "door" || interactableType == "sdoor")
			{
				GameObject gameObject = ((!(interactableType == "door")) ? GameObject.Find(interactable.name.Remove(interactable.name.IndexOf("-"))) : interactable);
				gui.SetConsoleScreen(0);
			}
			else if (interactableType == "cctv")
			{
				CCTV_Camera cCTV_Camera = hit.transform.GetComponent<CCTV_Camera>();
				if (cCTV_Camera == null)
				{
					cCTV_Camera = hit.transform.GetComponentInParent<CCTV_Camera>();
				}
				cam.transform.position = cCTV_Camera.cameraTarget.position;
				curCamera = cCTV_Camera.gameObject;
				liftID = cCTV_Camera.liftID;
				gui.liftButton.SetActive(liftID != string.Empty && !liftID.Contains("tesla"));
				gui.teslaButton.SetActive(liftID.Contains("tesla"));
				isHacked = false;
				StopInteracting();
				RefreshCamerasLODs();
			}
			else
			{
				if (!(interactableType == "lift"))
				{
					return;
				}
				CCTV_Camera[] array = Object.FindObjectsOfType<CCTV_Camera>();
				CCTV_Camera[] array2 = array;
				foreach (CCTV_Camera cCTV_Camera2 in array2)
				{
					if (cCTV_Camera2.liftID.Contains(liftID.Remove(4)) && cCTV_Camera2.liftID != liftID)
					{
						cam.transform.position = cCTV_Camera2.cameraTarget.position;
						liftID = cCTV_Camera2.liftID;
						gui.liftButton.SetActive(liftID != string.Empty);
						isHacked = false;
						StopInteracting();
						RefreshCamerasLODs();
						break;
					}
				}
			}
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Console_OpenDoor();
				StopInteracting();
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Console_LockDoor();
				StopInteracting();
			}
		}
	}

	private string RecognizeInteractable()
	{
		string result = "unclassified";
		if (interactable != null)
		{
			if (interactable.tag == "Door")
			{
				result = "door";
				hackingTime = GetHackingTime(interactable.GetComponent<Door>());
			}
			else if (interactable.tag == "SecondDoor" || interactable.tag == "DoorButton")
			{
				result = "sdoor";
				hackingTime = GetHackingTime(GameObject.Find(interactable.name.Remove(interactable.name.IndexOf("-"))).GetComponent<Door>());
			}
			else if (interactable.tag == "CCTV")
			{
				result = "cctv";
				hackingTime = 0.3f;
			}
			else if (interactable.tag == "LiftTarget")
			{
				result = "lift";
				hackingTime = 3.6f;
			}
			if (hackingTime == 0f)
			{
				result = "stop";
			}
		}
		return result;
	}

	public void UseElevator()
	{
		if (!isHacked && interactableType == "unclassified")
		{
			interactable = GameObject.FindGameObjectWithTag("LiftTarget");
			interactableType = RecognizeInteractable();
		}
	}

	public void UseTesla()
	{
		if (!isHacked && interactableType == "unclassified")
		{
			CmdTriggerTesla(curCamera.GetComponentInParent<TeslaGate>().gameObject);
		}
	}

	[Command(channel = 4)]
	private void CmdTriggerTesla(GameObject tesla)
	{
		RpcTriggerTelsa(tesla);
	}

	[ClientRpc(channel = 4)]
	private void RpcTriggerTelsa(GameObject tesla)
	{
		tesla.GetComponent<TeslaGate>().Hack();
	}

	private float GetHackingTime(Door door)
	{
		float result = 0f;
		if (door.curCooldown <= 0f)
		{
			Door.RoomAccessConditions conditions = door.conditions;
			result = 0.4f;
			if (conditions.allow_detonate)
			{
				result = 1f;
			}
			else if (conditions.check_point)
			{
				result = 5f;
			}
			else if (conditions.exit_gate)
			{
				result = 30f;
			}
			else if (conditions.highlvl_arm)
			{
				result = 30f;
			}
			else if (conditions.intercom_room)
			{
				result = 5f;
			}
			else if (conditions.lowlvl_arm)
			{
				result = 30f;
			}
			else if (conditions.medium_scp)
			{
				result = 15f;
			}
			else if (conditions.midlvl_arm)
			{
				result = 30f;
			}
			else if (conditions.safe_scp)
			{
				result = 15f;
			}
			else if (conditions.violent_scp)
			{
				result = 15f;
			}
		}
		return result;
	}

	private void RefreshCamerasLODs()
	{
		CCTV_Camera[] array = Object.FindObjectsOfType<CCTV_Camera>();
		CCTV_Camera[] array2 = array;
		foreach (CCTV_Camera cCTV_Camera in array2)
		{
			cCTV_Camera.UpdateLOD();
		}
	}

	private void LateUpdate()
	{
		SetListenerPosition();
		SetLookRotationPosition();
	}

	private void SetListenerPosition()
	{
		if (base.isLocalPlayer && iAm079)
		{
			if (spectCam == null)
			{
				spectCam = FindAnyObjectByType<SpectatorCamera>();
				return;
			}
			spectCam.cam.transform.position = spectCam.cam079.transform.position;
			spectCam.cam.transform.rotation = spectCam.cam079.transform.rotation;
		}
	}

	private void SetLookRotationPosition()
	{
		if (base.isLocalPlayer && iAm079)
		{
			lookRotation.transform.position = cam.transform.position;
		}
	}

	[Command(channel = 4)]
	private void CmdOpenDoor(GameObject door)
	{
		door.GetComponent<Door>().ChangeState();
	}

	[Command(channel = 4)]
	private void CmdLockDoor(GameObject door)
	{
		RpcLockDoor(door);
	}

	[ClientRpc(channel = 4)]
	private void RpcLockDoor(GameObject door)
	{
		lockedDoor = door;
	}

	public void Console_OpenDoor()
	{
		if (interactableType == "door" || interactableType == "sdoor")
		{
			GameObject door = ((!(interactableType == "door")) ? GameObject.Find(interactable.name.Remove(interactable.name.IndexOf("-"))) : interactable);
			CmdOpenDoor(door);
		}
	}

	public void Console_LockDoor()
	{
		if (interactableType == "door" || (interactableType == "sdoor" && gui.ability > 40f))
		{
			gui.ability = 0f;
			GameObject door = ((!(interactableType == "door")) ? GameObject.Find(interactable.name.Remove(interactable.name.IndexOf("-"))) : interactable);
			CmdLockDoor(door);
			remainingLockdown = Random.Range(minLockTime, maxLockTime);
		}
	}
}
