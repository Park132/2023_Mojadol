using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LSM_TimerSc : MonoBehaviour
{
	public GameObject timerPannel;
    public TextMeshProUGUI timerT;
	public bool startTimer = false, limitTimeSetting, reverse;
    float timer, limitS;


	private void Start()
	{
		timer = 0;
		reverse = false;
		timerPannel.SetActive(false);
		startTimer = false;
	}

	private void Update()
	{
		if (startTimer)
		{
			if (!reverse)
				timer += Time.deltaTime;
			else
				timer -= Time.deltaTime;
			TimerText();

			if (timer >= limitS && limitTimeSetting && !reverse)
			{ GameManager.Instance.TimeOutProcess(); TimerStop(); }
			else if (reverse && timer <= 0 && limitTimeSetting)
			{ GameManager.Instance.TimeOutProcess(); TimerStop(); }
		}
	}

	public void TimerText()
	{
		timerT.text = ((timer / 60 > 0) ? (int)timer / 60 : 0) + " : " + (timer%60 < 10? "0":"")+((int)timer % 60);
	}

	public void TimerStart(){ TimerText(); timerPannel.SetActive(true) ; startTimer = true;}
	public void TimerStart(float maxTime) {  limitS = maxTime; limitTimeSetting = true; TimerStart(); }
	public void TimerStart(float maxTime, bool rev) { timer = maxTime; limitTimeSetting = true; reverse = rev; TimerStart(); }


	public void TimerStop() { timer = 0; timerPannel.SetActive(false); startTimer = false; limitTimeSetting = false; limitS = 0; reverse = false; }

	public float Get() { return timer; }

}
