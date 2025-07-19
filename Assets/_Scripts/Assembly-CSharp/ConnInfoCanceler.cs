using UnityEngine;

public class ConnInfoCanceler : ConnInfoButton
{
	public override void UseButton()
	{
		base.UseButton();
		FindAnyObjectByType<CustomNetworkManager>().StopClient();
	}
}
