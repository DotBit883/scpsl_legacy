using UnityEngine;

public class ElectroMagnets : MonoBehaviour
{
	public LeverButton lever;

	public Animator anim;

	private void Update()
	{
		anim.SetBool("isUP", lever.GetState());
	}
}
