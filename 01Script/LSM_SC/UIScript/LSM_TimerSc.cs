using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


// 타이머 스크립트
public class LSM_TimerSc : MonoBehaviour
{
	public GameObject timerPannel;			// 타이머 UI
    public TextMeshProUGUI timerT;			// 타이머를 표시해줄 UI
	public bool startTimer = false, limitTimeSetting, reverse;
    float timer, limitS;


	private void Start()
	{
		// 변수 초기화
		timer = 0;
		reverse = false;
		timerPannel.SetActive(false);
		startTimer = false;
	}

	private void Update()
	{
		// 타이머가 시작되었다면
		if (startTimer)
		{
			// 리버스에 대하여 더할지 뺄지 확인.
			timer += Time.deltaTime * (reverse ? -1 : 1);
			
			// UI 글자 처리
			TimerText();

			// 최대 시간이 정해져있는지 확인. 및 reverse 확인.
			if (timer >= limitS && limitTimeSetting && !reverse)
			{ GameManager.Instance.TimeOutProcess(); TimerStop(); }
			else if (reverse && timer <= 0 && limitTimeSetting)
			{ GameManager.Instance.TimeOutProcess(); TimerStop(); }
		}
	}

	// 타이머 시간에 따라 텍스트 변경
	public void TimerText()
	{
		timerT.text = ((timer / 60 > 0) ? (int)timer / 60 : 0) + " : " + (timer%60 < 10? "0":"")+((int)timer % 60);
	}


	// 타이머 오버로드.
	// 매개변수가 없는 타이머 시작. -> 제한시간이 없는 타이머시작.
	public void TimerStart(){ TimerText(); timerPannel.SetActive(true) ; startTimer = true;}
	// 매개변수가 하나 있는 타이머 시작 -> 제한시간이 존재하며, 해당 시간이 흐르면 TimeStop함수가 호출
	public void TimerStart(float maxTime) { TimerStart(maxTime, false); }
	// 매개변수가 두개 있는 타이머 시작 -> 제한시간이 존재하며, bool값을 true로 선언할 경우 제한 시간초에서 점차 줄어드는 타이머가 실행.
	public void TimerStart(float maxTime, bool rev) { timer = (rev? maxTime : 0); limitS = (rev? 0:maxTime); limitTimeSetting = true; reverse = rev; TimerStart(); }

	// 타이머가 정상적으로 종료될 경우 타이머 관련 변수를 초기화.
	public void TimerStop() { timer = 0; timerPannel.SetActive(false); startTimer = false; limitTimeSetting = false; limitS = 0; reverse = false; }

	// 스킵버튼을 클릭 시 실행되는 함수. 바로 타이머가 종료되게 설정.
	public void TimerOut() { GameManager.Instance.TimeOutProcess(); TimerStop(); }

	// Getter
	public float Get() { return timer; }

}
