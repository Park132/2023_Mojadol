using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	// 싱글톤///
    private static GameManager instance;
	private void Awake()
	{
		if (instance == null) { instance = this; }
		else { Destroy(this); }
		Awake_Function();
	}
	public static GameManager Instance{ get{ return instance; } }
	// ///

	public static float SELECTATTACKPATHTIME = 10f, ROUNDTIME = 1000f;

	public MoonHeader.ManagerState state;
	public MoonHeader.GameState gameState;

	public LSM_TimerSc timerSc;
	public int numOfPlayer;
	public TextMeshProUGUI turnText;
	private GameObject[] spawnPoints;

	public GameObject canvas;
	public GameObject selectAttackPathUI, mapUI;
	private Image screen;
	public LSM_PlayerCtrl[] player;

	private void Awake_Function()
	{
		state = MoonHeader.ManagerState.Ready;
		timerSc = this.GetComponent<LSM_TimerSc>();
		numOfPlayer = 1;
		spawnPoints = GameObject.FindGameObjectsWithTag("Spawner");
		canvas = GameObject.Find("Canvas");
		selectAttackPathUI = GameObject.Find("AttackPathUIs");
		selectAttackPathUI.GetComponentInChildren<Button>().onClick.AddListener(timerSc.TimerOut);
		selectAttackPathUI.SetActive(false);
		mapUI = GameObject.Find("MapUIs");

		screen = GameObject.Find("Screen").GetComponent<Image>();
		screen.transform.SetAsLastSibling();
		
	}

	private void Start()
	{
		
	}



	private void Update()
	{

		Game();	
	}

	private void Game()
	{
		// 매니저가 현재 Ready일경우 작업을 처리하도록 명령.
		if (state == MoonHeader.ManagerState.Ready)
		{
			switch (gameState)
			{
				// 플레이어가 각각 공격로를 설정 턴
				case MoonHeader.GameState.SettingAttackPath:
                    StartCoroutine(ScreenFade(true));
                    state = MoonHeader.ManagerState.Processing;
					SettingAttack();
					SettingTurnText();
					foreach (GameObject s in spawnPoints)
					{
						s.GetComponent<LSM_Spawner>().ChangeTurn();
					}
					selectAttackPathUI.SetActive(true);
					break;

				// 공격로 지정이 모두 완료 후 게임을 시작하기 전 카운트 다운
				case MoonHeader.GameState.StartGame:
					timerSc.TimerStart(3.5f, true);
					state = MoonHeader.ManagerState.Processing;
					SettingTurnText();
					foreach (GameObject s in spawnPoints)
					{
						s.GetComponent<LSM_Spawner>().ChangeTurn();
					}
					break;

				// 현재 게임 중.
				case MoonHeader.GameState.Gaming:
					SettingTurnText();
					foreach (GameObject s in spawnPoints)
					{
						s.GetComponent<LSM_Spawner>().state = MoonHeader.SpawnerState.Spawn;
						timerSc.TimerStart(ROUNDTIME);
						state = MoonHeader.ManagerState.Processing;
					}
					//MapCam.SetActive(false); MainCam.SetActive(true);
					break;
			}
		}
		if (state == MoonHeader.ManagerState.End)
		{
			switch (gameState)
			{
				// 공격로 선택의 시간이 종료되었다면, 약간의 시간이 흐른 후 시작되도록 설정.
				case MoonHeader.GameState.SettingAttackPath:
					foreach (GameObject s in spawnPoints)
					{
						s.GetComponent<LSM_Spawner>().CheckingSelectMon();						
					}
					state = MoonHeader.ManagerState.Ready;
					gameState = MoonHeader.GameState.StartGame;
					selectAttackPathUI.SetActive(false);
					break;
			}
		}
	}

	private void SettingAttack()
	{
		if (MoonHeader.GameState.SettingAttackPath != gameState) return;

		// 모든 스포너를 찾아, 스포너의 현재 상태를 공격로 설정으로 바꾼다.
		
		foreach (GameObject s in spawnPoints)
		{
			s.GetComponent<LSM_Spawner>().state = MoonHeader.SpawnerState.Setting;
		}
		timerSc.TimerStart(SELECTATTACKPATHTIME);
	}

	public void TimeOutProcess()
	{
		if (gameState == MoonHeader.GameState.SettingAttackPath)
			state = MoonHeader.ManagerState.End;

		else if (gameState == MoonHeader.GameState.StartGame && state == MoonHeader.ManagerState.Processing)
		{ gameState = MoonHeader.GameState.Gaming; state = MoonHeader.ManagerState.Ready; }

		else if (gameState == MoonHeader.GameState.Gaming && state == MoonHeader.ManagerState.Processing)
		{gameState = MoonHeader.GameState.SettingAttackPath; state = MoonHeader.ManagerState.Ready; }
	}

	private void SettingTurnText()
	{
		switch (gameState)
		{
			case MoonHeader.GameState.SettingAttackPath:
				turnText.text = "Setting Attack";
				break;
			case MoonHeader.GameState.StartGame:
				turnText.text = "Ready";
				break;
			case MoonHeader.GameState.Gaming:
				turnText.text = "Game";
				break;
		}
	}

	public IEnumerator ScreenFade(bool inout)
	{
		float origin = inout? 1:0, alpha = 0.01f * (inout?-1:1);

		screen.color = new Color(0,0,0,origin);

		for(int i = 0; i < 100; i++)
		{
			yield return new WaitForSeconds(0.01f);
			origin += alpha;
			screen.color = new Color(0,0,0,origin);
		}
	}
}
