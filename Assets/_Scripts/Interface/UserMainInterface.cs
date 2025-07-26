using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class UserMainInterface : MonoBehaviour
{
	public Slider sliderHP;

	public Slider searchProgress;

	public Text textHP;

	public Text specatorInfo;

	public GameObject hpOBJ;

	public GameObject searchOBJ;

	public GameObject overloadMsg;

	public GameObject summary;

	[Space]
	public Text fps;

	public static UserMainInterface singleton;

	public float lerpSpeed = 3f;

	public float lerpedHP;

	private void Awake()
	{
		singleton = this;
	}

	private void Start()
	{
		ResolutionManager.RefreshScreen();
	}

	public void SearchProgress(float curProgress, float targetProgress)
	{
		searchProgress.maxValue = targetProgress;
		searchProgress.value = curProgress;
		searchOBJ.SetActive(curProgress != 0f);
	}

	public void SetHP(int _hp, int _maxhp)
	{
		float num = _maxhp;
		lerpedHP = Mathf.Lerp(lerpedHP, _hp, Time.deltaTime * lerpSpeed);
		sliderHP.value = lerpedHP;
		textHP.text = Mathf.Round(sliderHP.value / num * 100f) + "%";
		sliderHP.maxValue = num;
	}

	private void Update()
	{
		try
		{
			fps.text = NetworkTime.rtt + " ms";
		}
		catch
		{
		}
	}
}
