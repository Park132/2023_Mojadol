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

	const float SELECTATTACKPATHTIME = 10f, ROUNDTIME = 50f;
	// SEARCHATTACKPATHTIME: ���ݷ� ���� �ð�. ROUNDTIME: ���� ���� �ð�.

	public MoonHeader.ManagerState state;		// ���� ���ӸŴ����� ����. --> ���ӸŴ����� ���� � �������� ex: �غ���, ó����, ó���Ϸ�
	public MoonHeader.GameState gameState;		// ���� ������ ���� ex: ���ݷ� ���� �ð�, ���� ���� ��, ���

	public LSM_TimerSc timerSc;			// Ÿ�̸� ��ũ��Ʈ. ���� ���� �� Ÿ�̸Ӱ� �ʿ���(ex: ���� ���ݷ� �����ð�, ���� ����ð�) ��� ����ϴ� ��ũ��Ʈ.
	
	public int numOfPlayer;				// ���� �÷��̾��� ��.
	public TextMeshProUGUI turnText;	// ���� ���� ������ ���Ͽ� ����ڿ��� �����ִ� UI. �Ŀ� �ٲ� ����.
	private GameObject[] spawnPoints;	// ���� �����ϴ� "������ ������"�� ����.
	public GameObject[] wayPoints;		// ���� �����ϴ� ��� "��������Ʈ"�� ����

	public GameObject canvas;						// ���� �����ϴ� ĵ����. �ϳ��� �ִٰ� �����Ͽ� Awake���� ã�� ����.
	public GameObject selectAttackPathUI, mapUI;	// selectAttackPathUI: ���ݷ� ���� �� ����ڿ��� �����ִ� UI���� ����� ������Ʈ.  mapUI: TopView ������ �� ����ڿ��� �����ִ� UI���� ����� ������Ʈ.
	private Image screen;							// ���̵� IN, OUT�� �� �� ����ϴ� �̹���.
	public LSM_PlayerCtrl[] players;				// ��� �÷��̾���� �����ϴ� �迭
	public LSM_PlayerCtrl mainPlayer;				// ���� �����ϰ��ִ� �÷��̾ �����ϴ� ����
	public TeamManager[] teamManagers;				// ��� ���� ���Ŵ���

	public List<GameObject>[] playerMinions;		// ��� �÷��̾���� �̴Ͼ��� ����. �ش� �κ� ���� PoolManager���� ������� ��� ��..


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
	}

	private void Start()
	{
		
	}



	private void Update()
	{
		Game();	
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
					StartCoroutine(mainPlayer.AttackPathSelectSetting());
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
		float origin = inout ? 1 : 0, alpha = 0.01f * (inout ? -1 : 1);

		screen.color = new Color(0, 0, 0, origin);

		for (int i = 0; i < 100; i++)
		{
			yield return new WaitForSeconds(0.01f);
			origin += alpha;
			screen.color = new Color(0, 0, 0, origin);
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
	private void ChangeRound_PlayerRemover()
	{
		for (int i = 0; i < playerMinions.Length; i++)
		{
			foreach (GameObject obj in playerMinions[i])
			{
				// �÷��̾� ��ġ�� �ش� Ÿ�԰� ���� �̴Ͼ��� ��ȯ.
				// �÷����� ���� �ش��ϴ� ��. ���� ����� ��ž���� �̵�.
				// ���� �ش� ��ž�� �ش��ϴ� ���ݷ� �̵� �������� ������. ex: ž ��ž�̶��, ���� ���� �ش� ���ݷθ� �Լ�.
				// wayIndex�� �ش� ��ž �� ��������Ʈ�� ���ϸ� �����ε����� ����.

				//teamManagers[i].this_teamSpawner.
				GameObject.Destroy(obj);
			}
			playerMinions[i].Clear();
		}
	}
}
