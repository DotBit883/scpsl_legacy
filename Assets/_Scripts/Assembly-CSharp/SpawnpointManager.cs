using UnityEngine;

public class SpawnpointManager : MonoBehaviour
{
	public GameObject GetRandomPosition(int classID)
	{
		GameObject result = null;
		Class obj = GameObject.Find("Host").GetComponent<CharacterClassManager>().klasy[classID];
		if (obj.team == Team.CDP || obj.team == Team.TUT)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("SP_CDP");
			int num = Random.Range(0, array.Length);
			result = array[num];
		}
		if (obj.team == Team.SCP)
		{
			if (obj.fullName == "SCP-173")
			{
				GameObject[] array2 = GameObject.FindGameObjectsWithTag("SP_173");
				int num2 = Random.Range(0, array2.Length);
				result = array2[num2];
			}
			else if (obj.fullName == "SCP-106")
			{
				GameObject[] array3 = GameObject.FindGameObjectsWithTag("SP_106");
				int num3 = Random.Range(0, array3.Length);
				result = array3[num3];
			}
			else if (obj.fullName == "SCP-049")
			{
				GameObject[] array4 = GameObject.FindGameObjectsWithTag("SP_049");
				int num4 = Random.Range(0, array4.Length);
				result = array4[num4];
			}
			else if (obj.fullName == "SCP-079")
			{
				GameObject[] array5 = GameObject.FindGameObjectsWithTag("SP_079");
				int num5 = Random.Range(0, array5.Length);
				result = array5[num5];
			}
		}
		if (obj.team == Team.MTF)
		{
			GameObject[] array6 = GameObject.FindGameObjectsWithTag("SP_MTF");
			int num6 = Random.Range(0, array6.Length);
			result = array6[num6];
		}
		if (obj.team == Team.RSC)
		{
			GameObject[] array7 = GameObject.FindGameObjectsWithTag("SP_RSC");
			int num7 = Random.Range(0, array7.Length);
			result = array7[num7];
		}
		if (obj.team == Team.CHI)
		{
			GameObject[] array8 = GameObject.FindGameObjectsWithTag("SP_CI");
			int num8 = Random.Range(0, array8.Length);
			result = array8[num8];
		}
		return result;
	}
}
