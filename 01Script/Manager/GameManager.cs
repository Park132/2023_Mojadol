using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

	public static float SELECTATTACKPATHTIME = 10f, ROUNDTIME = 1000f;

	public MoonHeader.ManagerState state;
	public MoonHeader.GameState gameState;

	public LSM_TimerSc timerSc;
	public int numOfPlayer;
	public TextMeshProUGUI turnText;
	private GameObject[] spawnPoints;

	//public GameObject MainCam, MapCam, MapSubCam;

	//public Vector3 mapCamBasePosition;
	public GameObject canvas;
	public LSM_PlayerCtrl[] player;

	private void Awake_Function()
	{
		state = MoonHeader.ManagerState.Ready;
		timerSc = this.GetComponent<LSM_TimerSc>();
		numOfPlayer = 1;
		spawnPoints = GameObject.FindGameObjectsWithTag("Spawner");
		canvas = GameObject.Find("Canvas");
		
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
		// �Ŵ����� ���� Ready�ϰ�� �۾��� ó���ϵ��� ����.
		if (state == MoonHeader.ManagerState.Ready)
		{
			switch (gameState)
			{
				// �÷��̾ ���� ���ݷθ� ���� ��
				case MoonHeader.GameState.SettingAttackPath:
					state = MoonHeader.ManagerState.Processing;
					SettingAttack();
					SettingTurnText();
					foreach (GameObject s in spawnPoints)
					{
						s.GetComponent<LSM_Spawner>().ChangeTurn();
					}
					
					break;

				// ���ݷ� ������ ��� �Ϸ� �� ������ �����ϱ� �� ī��Ʈ �ٿ�
				case MoonHeader.GameState.StartGame:
					timerSc.TimerStart(3.5f, true);
					state = MoonHeader.ManagerState.Processing;
					SettingTurnText();
					foreach (GameObject s in spawnPoints)
					{
						s.GetComponent<LSM_Spawner>().ChangeTurn();
					}
					break;

				// ���� ���� ��.
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
				// ���ݷ� ������ �ð��� ����Ǿ��ٸ�, �ణ�� �ð��� �帥 �� ���۵ǵ��� ����.
				case MoonHeader.GameState.SettingAttackPath:
					foreach (GameObject s in spawnPoints)
					{
						s.GetComponent<LSM_Spawner>().CheckingSelectMon();						
					}
					state = MoonHeader.ManagerState.Ready;
					gameState = MoonHeader.GameState.StartGame;
					break;
			}
		}
	}

	private void SettingAttack()
	{
		if (MoonHeader.GameState.SettingAttackPath != gameState) return;

		// ��� �����ʸ� ã��, �������� ���� ���¸� ���ݷ� �������� �ٲ۴�.
		
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
}