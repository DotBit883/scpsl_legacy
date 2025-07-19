using UnityEngine;

public class SmokeGrenade : GrenadeInstance
{
	public GameObject gfx;

	public float duration = 20f;

	public override void Explode(string thrower)
	{
		base.Explode(thrower);
		Object.Destroy(Object.Instantiate(gfx, base.transform.position, Quaternion.Euler(Vector3.zero)), duration);
	}
}
