using UnityEngine;

public class HandPart : MonoBehaviour
{
	public GameObject part;

	public int id;

	public Animator anim;

	private void Start()
	{
		if (anim == null)
		{
			anim = GetComponentsInParent<Animator>()[0];
		}
	}

	public void UpdateItem()
	{
		part.SetActive(anim.GetInteger("CurItem") == id);
	}
}
