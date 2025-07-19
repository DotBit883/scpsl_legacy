using UnityEngine;
using Mirror;

public class ShockGrenade : GrenadeInstance
{
	public GameObject gfx;

	public float range;

	public AnimationCurve damageOverRange;

	public AnimationCurve shakeOverRange;

	public LayerMask dmgmask;

	public override void Explode(string thrower)
	{
		base.Explode(thrower);
		Object.Destroy(Object.Instantiate(gfx, base.transform.position, Quaternion.Euler(Vector3.zero)), 8f);
		Object.Destroy(base.gameObject);
		HurtPlayer(thrower);
	}

	private void HurtPlayer(string thrower)
	{
		bool flag = false;
		bool flag2 = false;
		CharacterClassManager characterClassManager = null;
		WeaponManager weaponManager = null;
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (characterClassManager == null && gameObject.GetComponent<NetworkIdentity>().netId.ToString() == thrower)
			{
				characterClassManager = gameObject.GetComponent<CharacterClassManager>();
			}
			if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				Transform transform = gameObject.GetComponent<Scp049PlayerScript>().plyCam.transform;
				RaycastHit hitInfo;
				if (Physics.Raycast(transform.position, (base.transform.position - transform.position).normalized, out hitInfo, range, dmgmask) && hitInfo.transform.name == base.name)
				{
					flag = true;
					weaponManager = gameObject.GetComponent<WeaponManager>();
					flag2 = gameObject.GetComponent<NetworkIdentity>().netId.ToString() == thrower;
				}
				ExplosionCameraShake explosionCameraShake = FindAnyObjectByType<ExplosionCameraShake>();
				if (explosionCameraShake != null)
				{
					explosionCameraShake.Shake(shakeOverRange.Evaluate(Vector3.Distance(base.transform.position, gameObject.transform.position)));
				}
			}
		}
		if (flag)
		{
			Team team = characterClassManager.klasy[characterClassManager.curClass].team;
			if (WeaponManager.friendlyFire || weaponManager.GetShootPermission(team) || flag2)
			{
				weaponManager.GetComponent<PlayerStats>().CmdHurtPlayer(new PlayerStats.HitInfo(damageOverRange.Evaluate(Vector3.Distance(weaponManager.transform.position, base.transform.position) / range), "WORLD", "TESLA"), weaponManager.gameObject);
			}
		}
	}
}
