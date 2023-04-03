using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


// ��ü ���ӿ� ���� �Ŵ���.
// LSM��� ��ũ��Ʈ���� ��Ϲ��� �������� ������.. �Ӿ�
public class GameManager : MonoBehaviour
{
	// �̱���///
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
	// SEARCHATTACKPATHTIME: ���ݷ� ���� �ð�. ROUNDTIME: ���� ���� �ð�.

	public MoonHeader.ManagerState state;		// ���� ���ӸŴ����� ����. --> ���ӸŴ����� ���� � �������� ex: �غ���, ó����, ó���Ϸ�
	public MoonHeader.GameState gameState;		// ���� ������ ���� ex: ���ݷ� ���� �ð�, ���� ���� ��, ���

	public LSM_TimerSc timerSc;			// Ÿ�̸� ��ũ��Ʈ. ���� ���� �� Ÿ�̸Ӱ� �ʿ���(ex: ���� ���ݷ� �����ð�, ���� ����ð�) ��� ����ϴ� ��ũ��Ʈ.
	
	public int numOfPlayer;				// ���� �÷��̾��� ��.
	public TextMeshProUGUI turnText;	// ���� ���� ������ ���Ͽ� ����ڿ��� �����ִ� UI. �Ŀ� �ٲ� ����.
	private GameObject[] spawnPoints;	// ���� �����ϴ� "������ ������"�� ����.
	public GameObject[] wayPoints;		// ���� �����ϴ� ��� "��������Ʈ"�� ����

	public GameObject canvas;						// ���� �����ϴ� ĵ����. �ϳ��� �ִٰ� �����Ͽ� Awake���� ã�� ����.
	public GameObject selectAttackPathUI, mapUI, gameUI;    // selectAttackPathUI: ���ݷ� ���� �� ����ڿ��� �����ִ� UI���� ����� ������Ʈ.  mapUI: TopView ������ �� ����ڿ��� �����ִ� UI���� ����� ������Ʈ.
															// gameUI: ���� ���� �� ǥ�õǴ� UI
	public LSM_GameUI gameUI_SC;

	private Image screen;							// ���̵� IN, OUT�� �� �� ����ϴ� �̹���.
	public LSM_PlayerCtrl[] players;				// ��� �÷��̾���� �����ϴ� �迭
	public LSM_PlayerCtrl mainPlayer;				// ���� �����ϰ��ִ� �÷��̾ �����ϴ� ����
	public TeamManager[] teamManagers;				// ��� ���� ���Ŵ���

	public List<GameObject>[] playerMinions;        // ��� �÷��̾���� �̴Ͼ��� ����. �ش� �κ� ���� PoolManager���� ������� ��� ��..
	private List<GameObject> logUIs;
	private List<string> logUIs_Reservation;
	private float timer_log;

	// �̱������� ���� Awake�� ���� ��ġ�Ͽ��⿡ �̰��� �Ʒ��� �Լ��� ���.
	private void Awake_Function()
	{
		// ��� �÷��̾���� �����ϴ� ��. FindGameObjectsWithTag�� ����Ͽ� ������Ʈ�� ã��, �ش� ��ũ��Ʈ�� �����ϰ� ����.
		// �� �κ� ������ �÷��̾ ��ȯ�ϴ� ������ �ʿ�!, ���� �÷��̾���� �ʿ�.
		GameObject[] playerdummys = GameObject.FindGameObjectsWithTag("Player");
		players = new LSM_PlayerCtrl[playerdummys.Length];
		for (int i = 0; i < playerdummys.Length; i++) players[i] = playerdummys[i].transform.GetComponent<LSM_PlayerCtrl>();
		mainPlayer.isMainPlayer = true;

		// ���� ���ӸŴ����� ���� �ʱ�ȭ. Default�� Ready.
		state = MoonHeader.ManagerState.Ready;
		// ���ӸŴ����� �����ϴ� TimerSC�� �޾ƿ�.
		timerSc = this.GetComponent<LSM_TimerSc>();

		// ���������� �÷��̾ �Ѹ����� ����.
		numOfPlayer = 1;

		// �ش� ������ �´� ���� ������Ʈ���� ����.
		spawnPoints = GameObject.FindGameObjectsWithTag("Spawner");
		canvas = GameObject.Find("Canvas");
		selectAttackPathUI = GameObject.Find("AttackPathUIs");
		selectAttackPathUI.GetComponentInChildren<Button>().onClick.AddListener(timerSc.TimerOut);		// ��ŵ��ư�� �ش�. Ŭ�� �� TimerSC�� �����ϴ� TimerOut�Լ��� ����ǵ��� ����.
		selectAttackPathUI.SetActive(false);
		mapUI = GameObject.Find("MapUIs");
		gameUI = GameObject.Find("GameUI");
		gameUI_SC = gameUI.GetComponent<LSM_GameUI>();
		gameUI.SetActive(false);

		wayPoints = GameObject.FindGameObjectsWithTag("WayPoint");
		screen = GameObject.Find("Screen").GetComponent<Image>();
		screen.transform.SetAsLastSibling();	// ��ũ���� �ٸ� UI�� �������� ���� �������� ��ġ�ϴ� �ڵ�.
		GameObject[] teammdummy = GameObject.FindGameObjectsWithTag("TeamManager");
		teamManagers = new TeamManager[teammdummy.Length];
		foreach (GameObject t in teammdummy)
		{ teamManagers[(int)t.GetComponent<TeamManager>().team] = t.GetComponent<TeamManager>(); }

		// �÷��̾� �̴Ͼ��� ����Ʈ ����.
		playerMinions = new List<GameObject>[2];	// ���� ������ŭ �迭�� ũ�⸦ ����. ���� ���������� 2�� ����.
		for (int i = 0; i < 2; i++) { playerMinions[i] = new List<GameObject>(); }
		logUIs = new List<GameObject>();				// �α� ǥ�ÿ� ���Ͽ�. 5�� ���ϸ� ǥ���Ϸ��� �ش� ����Ʈ�� ����.
		logUIs_Reservation = new List<string>();        // �ټ����̻� ���ʹ� ���� ����Ʈ�� �����Ͽ� �� ����Ʈ �ȿ� ����.
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

	// ���� ���� �� ��� ��Ȳ�� ó���ϴ� �Լ�.
	// ���� ó�� ����, Ȥ�� ó�� �Ϸ��� ��쿡 �����.
	private void Game()
	{
		// ���ӸŴ����� ���°� ���� Ready�ϰ��.
		if (state == MoonHeader.ManagerState.Ready)
		{
			switch (gameState)	// ������ ���� ���¿� ���� ó��.
			{
				// �÷��̾ ���� ���ݷθ� �����ϴ� ��
				case MoonHeader.GameState.SettingAttackPath:
					StartCoroutine(ScreenFade(true));
					state = MoonHeader.ManagerState.Processing;
					SettingAttack();		// �������� ���¸� ����.
					SettingTurnText();      // �� ���� UI�� ����
					timerSc.TimerStart(SELECTATTACKPATHTIME);	// ���ӸŴ����� ������ ���ݷ� ���� �ð���ŭ Ÿ�̸� ����.
					selectAttackPathUI.SetActive(true);
					break;

				// ���ݷ� ������ ��� �Ϸ� �� ������ �����ϱ� �� ī��Ʈ �ٿ�
				case MoonHeader.GameState.StartGame:
					timerSc.TimerStart(3.5f, true);				//Ÿ�̸� ����. 3.5���� ���� �ð�.
					state = MoonHeader.ManagerState.Processing;
					SettingTurnText();		// �� ���� UI�� ����.
					foreach (GameObject s in spawnPoints)	// ��� ������ �����ʿ��� ���� ��������� �˸�.
					{ s.GetComponent<LSM_Spawner>().ChangeTurn(); }
					break;

				// ���� ������ ����
				case MoonHeader.GameState.Gaming:
					SettingTurnText();		// �� ���� UI�� ����.
					foreach (GameObject s in spawnPoints)	// ��� ������ �������� ���¸� ����.
					{ s.GetComponent<LSM_Spawner>().state = MoonHeader.SpawnerState.Spawn; }
					state = MoonHeader.ManagerState.Processing;
					timerSc.TimerStart(ROUNDTIME);	// ���ӸŴ����� ������ ���� ���� �ð���ŭ Ÿ�̸� ����.
					//MapCam.SetActive(false); MainCam.SetActive(true);
					break;
			}
		}
		// ���ӸŴ����� ���°� End(ó���Ϸ�)������ ���.
		else if (state == MoonHeader.ManagerState.End)
		{
			switch (gameState)	// ������ ���� ���¿� ���� ó��.
			{
				// ���ݷ� ������ �ð��� ����Ǿ��ٸ�, �ణ�� �ð��� �帥 �� ���۵ǵ��� ����.
				case MoonHeader.GameState.SettingAttackPath:
					foreach (GameObject s in spawnPoints)		// ��� ������ �����ʿ��� ���� �ִ� ���� ������ ����Ʈ ��ŭ �����Ͽ����� Ȯ���ϴ� �Լ� ����.
					{ s.GetComponent<LSM_Spawner>().CheckingSelectMon(); } 
					state = MoonHeader.ManagerState.Ready;
					gameState = MoonHeader.GameState.StartGame;
					selectAttackPathUI.SetActive(false);
					break;
				// ���� ���� ����Ǿ��ٸ�.
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

	// ���� ���� ���ݷ� ���� �ð��̹Ƿ�, ��� �����ͽ����ʿ��� ���ݷ� ������ �� �� ����ϴ� �Լ�.
	private void SettingAttack()
	{
		if (MoonHeader.GameState.SettingAttackPath != gameState) return;

		// ��� �����ʸ� ã��, �������� ���� ���¸� ���ݷ� �������� �ٲ۴�.
		foreach (GameObject s in spawnPoints)
		{
			s.GetComponent<LSM_Spawner>().state = MoonHeader.SpawnerState.Setting;
			s.GetComponent<LSM_Spawner>().ChangeTurn();
		}
	}

	// Ÿ�Ӿƿ�. �����صξ��� Ÿ�̸Ӱ� ������ ���.
	public void TimeOutProcess()
	{
		if (gameState == MoonHeader.GameState.SettingAttackPath)	//���ݷ� ���� ���� ���. Ÿ�̸Ӱ� ����Ǿ��� ���.
			state = MoonHeader.ManagerState.End;

		else if (gameState == MoonHeader.GameState.StartGame && state == MoonHeader.ManagerState.Processing)	// ���� ���� �� ���¿��� Ÿ�̸Ӱ� ����Ǿ��� ���.
		{ gameState = MoonHeader.GameState.Gaming; state = MoonHeader.ManagerState.Ready; }						// ������ ���¸� ���� ������ ����. ���ӸŴ����� ���¸� �غ������� ����.

		else if (gameState == MoonHeader.GameState.Gaming && state == MoonHeader.ManagerState.Processing)		// ���� ���߿� Ÿ�̸Ӱ� ����Ǿ��� ���.
		{state = MoonHeader.ManagerState.End; }			// ������ ���¸� ���ݷ� �������� ����. ���ӸŴ����� ���¸� �غ������� ����.
	}

	// ���� ������ ���¸� ��Ÿ���� UI�� �ؽ�Ʈ�� ����. �ش� �Լ��� ����׿����� �����ϰ� ����.
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

	// ��ũ�� ���̵� ��/�ƿ��� �����ϴ� �Լ�.
	// �Ű������� true�� ��� FadeIn (���� �����.)
	// �Ű������� false�� ��� FadeOut (���� ��ο���.)
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

	// ���ӸŴ����� ����Ǿ��ִ� �÷��̾� �̴Ͼ��� �����ϴ� �Լ�.
	// ���� �÷��̾��� �̴Ͼ��� ���� ���� �׾��� ��� ���Ǵ� �Լ�.
	public void PlayerMinionRemover(MoonHeader.Team t, string nam)
	{
		for (int i = 0; i < playerMinions[(int)t].Count; i++)
		{
			if (playerMinions[(int)t][i].name.Equals(nam))
				playerMinions[(int)t].Remove(playerMinions[(int)t][i]);
		}

	}

	// ���� �Ŵ����� ����Ǿ��ִ� �÷��̾� �̴Ͼ���� ���� �ı��ϴ� �Լ�.
	// ���� ���� ���尡 ����Ǿ��� ��� ���.
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

	// log�� �ִ� 5�� ǥ���ϰ� �����ϱ� ���� �Լ�.
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

	// ������ ����Ǿ��� ��� ����.
	public void GameEndingProcess()
	{

	}
}
