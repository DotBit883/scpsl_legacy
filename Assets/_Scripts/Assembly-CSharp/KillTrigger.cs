using UnityEngine;

public class KillTrigger : MonoBehaviour
{
	public int killsToTrigger;

	public int triggerID;

	public string alias;

	public bool disableOnEnd = true;

	public int prioirty;

	public void Trigger(int amount)
	{
		if (amount == killsToTrigger)
		{
			if (triggerID == -1)
			{
				FindAnyObjectByType<TutorialManager>().Tutorial2_Result();
			}
			else if (alias != string.Empty)
			{
				FindAnyObjectByType<TutorialManager>().Trigger(alias);
			}
			else
			{
				FindAnyObjectByType<TutorialManager>().Trigger(triggerID);
			}
			if (disableOnEnd)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
