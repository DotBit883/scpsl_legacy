using System.Collections;
using UnityEngine;
using Mirror;

public class LeverButton : NetworkBehaviour
{
	public enum LeverOrientation
	{
		OnIsDown = 0,
		OnIsUp = 1
	}

	[SyncVar(hook = nameof(SetSwitch))]
	public bool defaultState;

	[SerializeField]
	private Transform lever;

	private Quaternion targetQuaternion;

	[SerializeField]
	private LeverOrientation orientation;

	private float cooldown;

	public Light[] onLights;

	public Light[] offLights;

	public float intensity = 1.4f;

	private void SetSwitch(bool oldState, bool newState)
	{
		SetupLights();
	}

	public void Switch()
	{
		if (cooldown <= 0f)
		{
			cooldown = 0.6f;
			lever.GetComponent<AudioSource>().Play();
			defaultState = !defaultState;
		}
	}

	private void SetupLights()
	{
		if (onLights.Length != 0 && offLights.Length != 0)
		{
			for (int i = 0; i < onLights.Length; i++)
			{
				StartCoroutine(SetupLights(onLights[i], (!GetState()) ? 0f : intensity));
			}
			for (int j = 0; j < offLights.Length; j++)
			{
				StartCoroutine(SetupLights(offLights[j], GetState() ? 0f : intensity));
			}
		}
	}

	private IEnumerator SetupLights(Light l, float targetIntens)
	{
		if (l.intensity < targetIntens)
		{
			while (l.intensity < targetIntens)
			{
				l.intensity += Time.deltaTime * 5f;
				yield return new WaitForEndOfFrame();
			}
		}
		else
		{
			while (l.intensity > 0f)
			{
				l.intensity -= Time.deltaTime * 5f;
				yield return new WaitForEndOfFrame();
			}
		}
	}

	public bool GetState()
	{
		return (orientation != LeverOrientation.OnIsDown) ? (!defaultState) : defaultState;
	}

	private void Update()
	{
		if (cooldown > 0f)
		{
			cooldown -= Time.deltaTime;
		}
		targetQuaternion = Quaternion.Euler((!defaultState) ? new Vector3(92f, 0f, 0f) : new Vector3(268f, 0f, 0f));
		lever.transform.localRotation = Quaternion.LerpUnclamped(lever.transform.localRotation, targetQuaternion, Time.deltaTime * 5f);
	}
}
