using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


// 전체 게임에 대한 매니저.
// LSM담당 스크립트지만 톱니바퀴 아이콘이 맘에들어서.. 머쓱
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

	const float SELECTATTACKPATHTIME = 10f, ROUNDTIME = 500f;
	// SEARCHATTACKPATHTIME: 공격로 설정 시간. ROUNDTIME: 게임 진행 시간.

	public MoonHeader.ManagerState state;		// 현재 게임매니저의 상태. --> 게임매니저가 현재 어떤 상태인지 ex: 준비중, 처리중, 처리완료
	public MoonHeader.GameState gameState;		// 현재 게임의 상태 ex: 공격로 설정 시간, 게임 시작 전, 등등

	public LSM_TimerSc timerSc;			// 타이머 스크립트. 게임 진행 중 타이머가 필요한(ex: 게임 공격로 설정시간, 게임 진행시간) 경우 사용하는 스크립트.
	
	public int numOfPlayer;				// 현재 플레이어의 수.
	public TextMeshProUGUI turnText;	// 현재 턴의 종류에 대하여 사용자에게 보여주는 UI. 후에 바꿀 예정.
	private GameObject[] spawnPoints;	// 씬에 존재하는 "마스터 스포너"의 모음.
	public GameObject[] wayPoints;		// 씬에 존재하는 모든 "웨이포인트"의 모음

	public GameObject canvas;						// 씬에 존재하는 캔버스. 하나만 있다고 가정하여 Awake에서 찾아 저장.
	public GameObject selectAttackPathUI, mapUI, gameUI;    // selectAttackPathUI: 공격로 설정 때 사용자에게 보여주는 UI들이 저장된 오브젝트.  mapUI: TopView 상태일 때 사용자에게 보여주는 UI들이 저장된 오브젝트.
															// gameUI: 게임 진행 중 표시되는 UI
	public LSM_GameUI gameUI_SC;

	private Image screen;							// 페이드 IN, OUT을 할 때 사용하는 이미지.
	public LSM_PlayerCtrl[] players;				// 모든 플레이어들을 저장하는 배열
	public LSM_PlayerCtrl mainPlayer;				// 현재 접속하고있는 플레이어를 저장하는 변수
	public TeamManager[] teamManagers;				// 모든 팀의 팀매니저

	public List<GameObject>[] playerMinions;        // 모든 플레이어들의 미니언을 저장. 해당 부분 또한 PoolManager에서 사용할지 고민 중..
	private List<GameObject> logUIs;
	private List<string> logUIs_Reservation;
	private float timer_log;

	// 싱글톤으로 인해 Awake를 위로 배치하였기에 미관상 아래의 함수를 사용.
	private void Awake_Function()
	{
		// 모든 플레이어들을 저장하는 중. FindGameObjectsWithTag를 사용하여 오브젝트를 찾고, 해당 스크립트를 저장하게 구현.
		// 이 부분 이전에 플레이어를 소환하는 절차가 필요!, 로컬 플레이어또한 필요.
		GameObject[] playerdummys = GameObject.FindGameObjectsWithTag("Player");
		players = new LSM_PlayerCtrl[playerdummys.Length];
		for (int i = 0; i < playerdummys.Length; i++) players[i] = playerdummys[i].transform.GetComponent<LSM_PlayerCtrl>();
		mainPlayer.isMainPlayer = true;

		// 기존 게임매니저의 상태 초기화. Default값 Ready.
		state = MoonHeader.ManagerState.Ready;
		// 게임매니저에 존재하는 TimerSC를 받아옴.
		timerSc = this.GetComponent<LSM_TimerSc>();

		// 디버깅용으로 플레이어를 한명으로 설정.
		numOfPlayer = 1;

		// 해당 변수에 맞는 게임 오브젝트들을 저장.
		spawnPoints = GameObject.FindGameObjectsWithTag("Spawner");
		canvas = GameObject.Find("Canvas");
		selectAttackPathUI = GameObject.Find("AttackPathUIs");
		selectAttackPathUI.GetComponentInChildren<Button>().onClick.AddListener(timerSc.TimerOut);		// 스킵버튼에 해당. 클릭 시 TimerSC에 존재하는 TimerOut함수가 실행되도록 설정.
		selectAttackPathUI.SetActive(false);
		mapUI = GameObject.Find("MapUIs");
		gameUI = GameObject.Find("GameUI");
		gameUI_SC = gameUI.GetComponent<LSM_GameUI>();
		gameUI.SetActive(false);

		wayPoints = GameObject.FindGameObjectsWithTag("WayPoint");
		screen = GameObject.Find("Screen").GetComponent<Image>();
		screen.transform.SetAsLastSibling();	// 스크린이 다른 UI를 가리도록 가장 마지막에 배치하는 코드.
		GameObject[] teammdummy = GameObject.FindGameObjectsWithTag("TeamManager");
		teamManagers = new TeamManager[teammdummy.Length];
		foreach (GameObject t in teammdummy)
		{ teamManagers[(int)t.GetComponent<TeamManager>().team] = t.GetComponent<TeamManager>(); }

		// 플레이어 미니언의 리스트 저장.
		playerMinions = new List<GameObject>[2];	// 팀의 개수만큼 배열의 크기를 지정. 현재 디버깅용으로 2로 설정.
		for (int i = 0; i < 2; i++) { playerMinions[i] = new List<GameObject>(); }
		logUIs = new List<GameObject>();				// 로그 표시에 대하여. 5개 이하만 표시하려고 해당 리스트를 생성.
		logUIs_Reservation = new List<string>();        // 다섯개이상 부터는 예약 리스트를 생성하여 그 리스트 안에 저장.
		timer_log = 0;
	}

	private void Start()
	{
		
	}



	private void Update()
	{
		Game();
		DisplayEnable();
	}

	// 게임 진행 중 모든 상황을 처리하는 함수.
	// 보통 처음 시작, 혹은 처리 완료일 경우에 실행됨.
	private void Game()
	{
		// 게임매니저의 상태가 현재 Ready일경우.
		if (state == MoonHeader.ManagerState.Ready)
		{
			switch (gameState)	// 게임의 현재 상태에 따라 처리.
			{
				// 플레이어가 각각 공격로를 설정하는 턴
				case MoonHeader.GameState.SettingAttackPath:
					StartCoroutine(ScreenFade(true));
					state = MoonHeader.ManagerState.Processing;
					SettingAttack();		// 스포너의 상태를 변경.
					SettingTurnText();      // 턴 상태 UI를 변경
					timerSc.TimerStart(SELECTATTACKPATHTIME);	// 게임매니저에 설정된 공격로 설정 시간만큼 타이머 시작.
					selectAttackPathUI.SetActive(true);
					break;

				// 공격로 지정이 모두 완료 후 게임을 시작하기 전 카운트 다운
				case MoonHeader.GameState.StartGame:
					timerSc.TimerStart(3.5f, true);				//타이머 세팅. 3.5초의 설정 시간.
					state = MoonHeader.ManagerState.Processing;
					SettingTurnText();		// 턴 상태 UI를 변경.
					foreach (GameObject s in spawnPoints)	// 모든 마스터 스포너에게 턴이 변경됐음을 알림.
					{ s.GetComponent<LSM_Spawner>().ChangeTurn(); }
					break;

				// 현재 게임을 시작
				case MoonHeader.GameState.Gaming:
					SettingTurnText();		// 턴 상태 UI를 변경.
					foreach (GameObject s in spawnPoints)	// 모든 마스터 스포너의 상태를 변경.
					{ s.GetComponent<LSM_Spawner>().state = MoonHeader.SpawnerState.Spawn; }
					state = MoonHeader.ManagerState.Processing;
					timerSc.TimerStart(ROUNDTIME);	// 게임매니저에 설정된 게임 진행 시간만큼 타이머 시작.
					//MapCam.SetActive(false); MainCam.SetActive(true);
					break;
			}
		}
		// 게임매니저의 상태가 End(처리완료)상태일 경우.
		else if (state == MoonHeader.ManagerState.End)
		{
			switch (gameState)	// 게임의 현재 상태에 따라 처리.
			{
				// 공격로 선택의 시간이 종료되었다면, 약간의 시간이 흐른 후 시작되도록 설정.
				case MoonHeader.GameState.SettingAttackPath:
					foreach (GameObject s in spawnPoints)		// 모든 마스터 스포너에게 현재 최대 설정 가능한 포인트 만큼 설정하였는지 확인하는 함수 실행.
					{ s.GetComponent<LSM_Spawner>().CheckingSelectMon(); } 
					state = MoonHeader.ManagerState.Ready;
					gameState = MoonHeader.GameState.StartGame;
					selectAttackPathUI.SetActive(false);
					break;
				// 게임 턴이 종료되었다면.
				case MoonHeader.GameState.Gaming:
					ScreenFade(false);
					StartCoroutine(mainPlayer.AttackPathSelectSetting());
					ChangeRound_AllRemover();
					Cursor.lockState = CursorLockMode.None;
					state = MoonHeader.ManagerState.Ready;
					gameState = MoonHeader.GameState.SettingAttackPath;
					break;
			}
		}
	}

	// 현재 턴이 공격로 선택 시간이므로, 모든 마스터스포너에게 공격로 설정을 할 때 사용하는 함수.
	private void SettingAttack()
	{
		if (MoonHeader.GameState.SettingAttackPath != gameState) return;

		// 모든 스포너를 찾아, 스포너의 현재 상태를 공격로 설정으로 바꾼다.
		foreach (GameObject s in spawnPoints)
		{
			s.GetComponent<LSM_Spawner>().state = MoonHeader.SpawnerState.Setting;
			s.GetComponent<LSM_Spawner>().ChangeTurn();
		}
	}

	// 타임아웃. 설정해두었던 타이머가 끝났을 경우.
	public void TimeOutProcess()
	{
		if (gameState == MoonHeader.GameState.SettingAttackPath)	//공격로 설정 턴일 경우. 타이머가 종료되었을 경우.
			state = MoonHeader.ManagerState.End;

		else if (gameState == MoonHeader.GameState.StartGame && state == MoonHeader.ManagerState.Processing)	// 게임 시작 전 상태에서 타이머가 종료되었을 경우.
		{ gameState = MoonHeader.GameState.Gaming; state = MoonHeader.ManagerState.Ready; }						// 게임의 상태를 게임 중으로 변경. 게임매니저의 상태를 준비중으로 변경.

		else if (gameState == MoonHeader.GameState.Gaming && state == MoonHeader.ManagerState.Processing)		// 게임 도중에 타이머가 종료되었을 경우.
		{state = MoonHeader.ManagerState.End; }			// 게임의 상태를 공격로 설정으로 변경. 게임매니저의 상태를 준비중으로 변경.
	}

	// 현재 게임의 상태를 나타내는 UI의 텍스트를 변경. 해당 함수는 디버그용으로 간략하게 구현.
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

	// 스크린 페이드 인/아웃을 구현하는 함수.
	// 매개변수가 true일 경우 FadeIn (점점 밝아짐.)
	// 매개변수가 false일 경우 FadeOut (점점 어두워짐.)
	public IEnumerator ScreenFade(bool inout)
	{
		if ((inout && screen.color.a >= 0.9f) || (!inout && screen.color.a <= 0.1f))
		{
			int time = 50;
			float origin = inout ? 1 : 0, alpha = ((float)1 / time ) * (inout ? -1 : 1);

			screen.color = new Color(0, 0, 0, origin);

			for (int i = 0; i < time; i++)
			{
				yield return new WaitForSeconds(0.01f);
				origin += alpha;
				screen.color = new Color(0, 0, 0, origin);
			}
			screen.color = new Color(0, 0, 0, (inout ? 0 : 1));
		}
	}

	// 게임매니저에 저장되어있는 플레이어 미니언을 삭제하는 함수.
	// 보통 플레이어의 미니언이 게임 도중 죽었을 경우 사용되는 함수.
	public void PlayerMinionRemover(MoonHeader.Team t, string nam)
	{
		for (int i = 0; i < playerMinions[(int)t].Count; i++)
		{
			if (playerMinions[(int)t][i].name.Equals(nam))
				playerMinions[(int)t].Remove(playerMinions[(int)t][i]);
		}

	}

	// 게임 매니저에 저장되어있는 플레이어 미니언들을 전부 파괴하는 함수.
	// 보통 게임 라운드가 변경되었을 경우 사용.
	private void ChangeRound_AllRemover()
	{
		for (int i = 0; i < playerMinions.Length; i++)
		{
			foreach (GameObject obj in playerMinions[i])
			{
				obj.SetActive(false);
			}
			playerMinions[i].Clear();
		}
		for (int i = 0; i < PoolManager.Instance.minions.Length; i++) {
			foreach (GameObject minion in PoolManager.Instance.poolList_Minion[i])
			{
				if (minion.activeSelf)
				{
					LSM_MinionCtrl dummyCtrl = minion.GetComponent<LSM_MinionCtrl>();
					teamManagers[(int)dummyCtrl.stats.actorHealth.team].exp += dummyCtrl.stats.exp;
					minion.SetActive(false);
				}
			}
		}
	}

	// log를 최대 5개 표시하게 관리하기 위한 함수.
	private void DisplayEnable()
	{
		timer_log += Time.deltaTime;
		if (logUIs.Count <= 5 && logUIs_Reservation.Count > 0 && timer_log >= 1f)
		{
			timer_log = 0;
			GameObject dummy = PoolManager.Instance.Get_UI(0);
			dummy.GetComponentInChildren<TextMeshProUGUI>().text = logUIs_Reservation[0];
			dummy.GetComponent<RectTransform>().anchoredPosition = new Vector3(-50, -100, 0);
			logUIs.Add(dummy);
			logUIs_Reservation.RemoveAt(0);
		}
		
	}
	public void DisplayAdd(string content)
	{
		logUIs_Reservation.Add(content);
	}
	public void DisplayChecking() {
		for (int i = logUIs.Count-1; i >= 0; i--)
		{
			if (!logUIs[i].activeSelf)
				logUIs.RemoveAt(i);
		}
	}

	// 게임이 종료되었을 경우 실행.
	public void GameEndingProcess()
	{

	}
}
