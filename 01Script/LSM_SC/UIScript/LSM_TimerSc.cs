using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


// Ÿ�̸� ��ũ��Ʈ
public class LSM_TimerSc : MonoBehaviour
{
	public GameObject timerPannel;			// Ÿ�̸� UI
    public TextMeshProUGUI timerT;			// Ÿ�̸Ӹ� ǥ������ UI
	public bool startTimer = false, limitTimeSetting, reverse;
    float timer, limitS;


	private void Start()
	{
		// ���� �ʱ�ȭ
		timer = 0;
		reverse = false;
		timerPannel.SetActive(false);
		startTimer = false;
	}

	private void Update()
	{
		// Ÿ�̸Ӱ� ���۵Ǿ��ٸ�
		if (startTimer)
		{
			// �������� ���Ͽ� ������ ���� Ȯ��.
			timer += Time.deltaTime * (reverse ? -1 : 1);
			
			// UI ���� ó��
			TimerText();

			// �ִ� �ð��� �������ִ��� Ȯ��. �� reverse Ȯ��.
			if (timer >= limitS && limitTimeSetting && !reverse)
			{ GameManager.Instance.TimeOutProcess(); TimerStop(); }
			else if (reverse && timer <= 0 && limitTimeSetting)
			{ GameManager.Instance.TimeOutProcess(); TimerStop(); }
		}
	}

	// Ÿ�̸� �ð��� ���� �ؽ�Ʈ ����
	public void TimerText()
	{
		timerT.text = ((timer / 60 > 0) ? (int)timer / 60 : 0) + " : " + (timer%60 < 10? "0":"")+((int)timer % 60);
	}


	// Ÿ�̸� �����ε�.
	// �Ű������� ���� Ÿ�̸� ����. -> ���ѽð��� ���� Ÿ�̸ӽ���.
	public void TimerStart(){ TimerText(); timerPannel.SetActive(true) ; startTimer = true;}
	// �Ű������� �ϳ� �ִ� Ÿ�̸� ���� -> ���ѽð��� �����ϸ�, �ش� �ð��� �帣�� TimeStop�Լ��� ȣ��
	public void TimerStart(float maxTime) { TimerStart(maxTime, false); }
	// �Ű������� �ΰ� �ִ� Ÿ�̸� ���� -> ���ѽð��� �����ϸ�, bool���� true�� ������ ��� ���� �ð��ʿ��� ���� �پ��� Ÿ�̸Ӱ� ����.
	public void TimerStart(float maxTime, bool rev) { timer = (rev? maxTime : 0); limitS = (rev? 0:maxTime); limitTimeSetting = true; reverse = rev; TimerStart(); }

	// Ÿ�̸Ӱ� ���������� ����� ��� Ÿ�̸� ���� ������ �ʱ�ȭ.
	public void TimerStop() { timer = 0; timerPannel.SetActive(false); startTimer = false; limitTimeSetting = false; limitS = 0; reverse = false; }

	// ��ŵ��ư�� Ŭ�� �� ����Ǵ� �Լ�. �ٷ� Ÿ�̸Ӱ� ����ǰ� ����.
	public void TimerOut() { GameManager.Instance.TimeOutProcess(); TimerStop(); }

	// Getter
	public float Get() { return timer; }

}
