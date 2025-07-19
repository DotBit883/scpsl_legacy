using UnityEngine;
using Mirror;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour
{
	[SerializeField]
	private bool m_IsWalking;

	[SerializeField]
	public float m_WalkSpeed;

	[SerializeField]
	public float m_RunSpeed;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_RunstepLenghten;

	[SerializeField]
	public float m_JumpSpeed;

	[SerializeField]
	private float m_StickToGroundForce;

	[SerializeField]
	private float m_GravityMultiplier;

	public MouseLook m_MouseLook;

	[SerializeField]
	private bool m_UseFovKick;

	[SerializeField]
	private FOVKick m_FovKick = new FOVKick();

	[SerializeField]
	public bool m_UseHeadBob;

	[SerializeField]
	private CurveControlledBob m_HeadBob = new CurveControlledBob();

	[SerializeField]
	private LerpControlledBob m_JumpBob = new LerpControlledBob();

	[SerializeField]
	private float m_StepInterval;

	[SerializeField]
	public AudioClip[] m_FootstepSounds;

	[SerializeField]
	public AudioClip m_JumpSound;

	[SerializeField]
	public AudioClip m_LandSound;

	public Camera m_Camera;

	private bool m_Jump;

	private float m_YRotation;

	private Vector2 m_Input;

	public Vector3 m_MoveDir = Vector3.zero;

	public Vector2 plySpeed;

	public CharacterController m_CharacterController;

	private CollisionFlags m_CollisionFlags;

	private bool m_PreviouslyGrounded;

	private Vector3 m_OriginalCameraPosition;

	private float m_StepCycle;

	private float m_NextStep;

	private bool m_Jumping;

	private AudioSource m_AudioSource;

	public float zoomSlowdown = 1f;

	public bool lookingAtMe;

	public bool tutstop;

	public bool isInfected;

	public bool isSearching;

	public bool usingConsole;

	public bool usingTurret;

	public bool isPaused;

	public bool noclip;

	public bool lockMovement;

	public bool rangeSpeed;

	public int animationID;

	public float blinkAddition;

	private Vector3 previousPosition;

	private void Start()
	{
		m_CharacterController = GetComponent<CharacterController>();
		m_OriginalCameraPosition = m_Camera.transform.localPosition;
		m_FovKick.Setup(m_Camera);
		m_HeadBob.Setup(m_Camera, m_StepInterval);
		m_StepCycle = 0f;
		m_NextStep = m_StepCycle / 2f;
		m_Jumping = false;
		m_AudioSource = GetComponent<AudioSource>();
		m_MouseLook.Init(base.transform, m_Camera.transform);
	}

	private void Update()
	{
		RotateView();
		lockMovement = usingTurret | isSearching | usingConsole | noclip | isPaused;
		if (!m_Jump && m_CharacterController.isGrounded)
		{
			m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
		}
		if (lockMovement || lookingAtMe || isInfected || tutstop)
		{
			m_Jump = false;
		}
		if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
		{
			StartCoroutine(m_JumpBob.DoBobCycle());
			PlayLandingSound();
			m_MoveDir.y = 0f;
			m_Jumping = false;
		}
		if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
		{
			m_MoveDir.y = 0f;
		}
		m_PreviouslyGrounded = m_CharacterController.isGrounded;
	}

	private void PlayLandingSound()
	{
		if (GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			base.gameObject.SendMessage("SyncJump", false);
		}
		m_AudioSource.clip = m_LandSound;
		m_AudioSource.Play();
		m_NextStep = m_StepCycle + 0.5f;
	}

	public void MotorPlayer()
	{
		float speed;
		GetInput(out speed);
		Vector3 vector = base.transform.forward * m_Input.y + base.transform.right * m_Input.x;
		RaycastHit hitInfo;
		Physics.SphereCast(base.transform.position, m_CharacterController.radius, Vector3.down, out hitInfo, m_CharacterController.height / 2f, -1, QueryTriggerInteraction.Ignore);
		vector = Vector3.ProjectOnPlane(vector, hitInfo.normal).normalized;
		m_MoveDir.x = vector.x * speed * zoomSlowdown;
		m_MoveDir.z = vector.z * speed * zoomSlowdown;
		if (m_CharacterController.isGrounded)
		{
			m_MoveDir.y = 0f - m_StickToGroundForce;
			if (m_Jump)
			{
				m_MoveDir.y = m_JumpSpeed;
				PlayJumpSound();
				m_Jump = false;
				m_Jumping = true;
			}
		}
		else
		{
			m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime * ((!noclip) ? 1 : 0);
		}
		if (blinkAddition == 0f)
		{
			m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
		}
		else
		{
			m_CollisionFlags = m_CharacterController.Move(m_MoveDir * blinkAddition / 10f);
		}
		ProgressStepCycle(speed);
		UpdateCameraPosition(speed);
	}

	private void FixedUpdate()
	{
		MotorPlayer();
		Vector3 vector = (base.transform.position - previousPosition) / Time.fixedDeltaTime;
		vector = Quaternion.Euler(0f, 0f - base.transform.rotation.eulerAngles.y, 0f) * vector;
		plySpeed = new Vector2(Mathf.Round(vector.z), Mathf.Round(vector.x));
		previousPosition = base.transform.position;
	}

	private void PlayJumpSound()
	{
		if (GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			base.gameObject.SendMessage("SyncJump", true);
		}
		m_AudioSource.clip = m_JumpSound;
		m_AudioSource.Play();
	}

	private void ProgressStepCycle(float speed)
	{
		if (m_CharacterController.velocity.sqrMagnitude > 0f && (m_Input.x != 0f || m_Input.y != 0f))
		{
			m_StepCycle += (m_CharacterController.velocity.magnitude + speed * ((!m_IsWalking) ? m_RunstepLenghten : 1f)) * Time.fixedDeltaTime;
		}
		if (m_StepCycle > m_NextStep)
		{
			m_NextStep = m_StepCycle + m_StepInterval;
			PlayFootStepAudio();
		}
	}

	private void PlayFootStepAudio()
	{
		if (m_CharacterController.isGrounded && !(zoomSlowdown <= 0.4f))
		{
			int num = Random.Range(1, m_FootstepSounds.Length);
			m_AudioSource.clip = m_FootstepSounds[num];
			m_AudioSource.PlayOneShot(m_AudioSource.clip);
			if (GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				base.gameObject.SendMessage("SyncFoot");
			}
			m_FootstepSounds[num] = m_FootstepSounds[0];
			m_FootstepSounds[0] = m_AudioSource.clip;
		}
	}

	private void UpdateCameraPosition(float speed)
	{
		if (m_UseHeadBob)
		{
			Vector3 localPosition;
			if (m_CharacterController.velocity.magnitude > 0f && m_CharacterController.isGrounded)
			{
				m_Camera.transform.localPosition = m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude + speed * ((!m_IsWalking) ? m_RunstepLenghten : 1f));
				localPosition = m_Camera.transform.localPosition;
				localPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
			}
			else
			{
				localPosition = m_Camera.transform.localPosition;
				localPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
			}
			m_Camera.transform.localPosition = localPosition;
		}
	}

	private void GetInput(out float speed)
	{
		float axis = CrossPlatformInputManager.GetAxis("Horizontal");
		float axis2 = CrossPlatformInputManager.GetAxis("Vertical");
		bool isWalking = m_IsWalking;
		m_IsWalking = !Input.GetButton("Run") || zoomSlowdown != 1f;
		speed = ((!m_IsWalking) ? m_RunSpeed : m_WalkSpeed) * (float)((!(lockMovement | lookingAtMe | isInfected | tutstop)) ? 1 : 0) * (float)((!(rangeSpeed & !m_IsWalking)) ? 1 : 15) + blinkAddition;
		m_Input = new Vector2(axis, axis2);
		if (m_Input.sqrMagnitude > 1f)
		{
			m_Input.Normalize();
		}
		if (m_IsWalking != isWalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0f)
		{
			StopAllCoroutines();
			StartCoroutine(m_IsWalking ? m_FovKick.FOVKickDown() : m_FovKick.FOVKickUp());
		}
	}

	private void RotateView()
	{
		if (!lockMovement || noclip)
		{
			m_MouseLook.LookRotation(base.transform, m_Camera.transform);
		}
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (m_CollisionFlags != CollisionFlags.Below && !(attachedRigidbody == null) && !attachedRigidbody.isKinematic)
		{
			attachedRigidbody.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
		}
	}

	private void LateUpdate()
	{
		animationID = (m_Jumping ? 2 : ((Input.GetButton("Run") && zoomSlowdown == 1f) ? 1 : 0));
	}
}
