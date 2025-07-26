using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class WeaponCamera : MonoBehaviour
{
	private VignetteAndChromaticAberration vaca;

	private VignetteAndChromaticAberration myvaca;

	private void Start()
	{
		myvaca = GetComponent<VignetteAndChromaticAberration>();
		vaca = GetComponentInParent<VignetteAndChromaticAberration>();
	}

	private void Update()
	{
		myvaca = vaca;
	}
}
