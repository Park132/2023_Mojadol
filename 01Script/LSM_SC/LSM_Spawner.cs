using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 팀 별 마스터 스포너!!
public class LSM_Spawner : MonoBehaviour
{
	float BASEDELAY = 1.5f;
	float BASEWAVEDELAY = 10f;
	public int MAX_NUM_MINION ;
	public MoonHeader.Team team;

	public MoonHeader.SpawnerState state;
	//public GameObject[] way;
	public MoonHeader.SpawnerPaths[] spawnpoints;
	


	//public GameObject[] arrowDirect;

	public int wave_Minions_Num, selectedNum;
	//public byte num_attack;
	public float delay;

	private void Start()
	{
		state = MoonHeader.SpawnerState.None;

		//GameObject[] ways = GameObject.FindGameObjectsWithTag("SpawnPoint");
		List<GameObject> ways = new List<GameObject>();
		foreach (Transform tr in gameObject.GetComponentsInChildren<Transform>())
		{
			if (tr.CompareTag("SpawnPoint"))
				ways.Add(tr.gameObject);
		}
		spawnpoints = new MoonHeader.SpawnerPaths[ways.Count];
		for (int i = 0; i < ways.Count; i++)
		{ spawnpoints[i] = new MoonHeader.SpawnerPaths(ways[i]); }

		//num_attack = 0;
		delay = 0;
		wave_Minions_Num = 0;
		selectedNum = 0;
		MAX_NUM_MINION = 9;
		
	}

	private void Update()
	{
		CheckingSpawn();
		
	}

	

	private void CheckingSpawn()
	{
		// 현재 스포너의 상태가 공격로 선택이라면 선택
		if (state == MoonHeader.SpawnerState.Setting)
		{
			selectedNum = 0;
			foreach (MoonHeader.SpawnerPaths item in spawnpoints)
			{
				selectedNum += item.num;
			}

		}
		else if (state == MoonHeader.SpawnerState.Spawn)
		{
			// 게임이 진행중일 경우 소환.
			if (GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
			{
				delay += Time.deltaTime;

				// 웨이브 최대 소환수보다 적게 소환했다면, 소환.
				if (delay > BASEDELAY && wave_Minions_Num < MAX_NUM_MINION)
				{
					delay = 0;

					for (int i = 0; i < spawnpoints.Length; i++)
					{
						if (spawnpoints[i].num > spawnpoints[i].summon_)
						{
							GameObject dummy;
							// 가장 처음 소환되는 미니언은 덩치
							if (spawnpoints[i].summon_ == 0)
							{ dummy = PoolManager.Instance.Get_Minion(0); }
							else 
							{ 
								dummy = PoolManager.Instance.Get_Minion(0); 
							}

							dummy.transform.position = spawnpoints[i].path.transform.position;
							//dummy.transform.parent = this.transform;
							LSM_MinionCtrl dummy_ctrl = dummy.GetComponent<LSM_MinionCtrl>();
							dummy_ctrl.MonSetting(spawnpoints[i].path.GetComponent<LSM_SpawnPointSc>().Ways, team, this.GetComponent<LSM_Spawner>());
							dummy_ctrl.minionBelong = i;
							dummy_ctrl.minionType = spawnpoints[i].summon_ % 2;	//미니언의 타입을 결정

							spawnpoints[i].summon_++;
							wave_Minions_Num++;
						}
					}
				}
				// 만약 한 웨이브에 소환이 가능한 수를 넘었다면, 새로운 웨이브 소환 시간까지 대기
				else if (wave_Minions_Num >= MAX_NUM_MINION)
				{
					if (delay > BASEWAVEDELAY)
					{
						wave_Minions_Num = 0;
						for (int i = 0; i < spawnpoints.Length; i++)
						{
							spawnpoints[i].summon_ = 0;
						}
						
					}
				}
			}
		}

	}

	public void ChangeTurn()
	{
		bool change_dummy = false;
		// 현재 공격로 지정 턴일때
		if (GameManager.Instance.gameState == MoonHeader.GameState.SettingAttackPath)
		{change_dummy = true;}
		// 공격로 지정 턴이 아닐 경우
		else
		{change_dummy = false;}

		foreach (MoonHeader.SpawnerPaths item in spawnpoints)
		{
			LSM_SpawnPointSc dummy = item.path.GetComponent<LSM_SpawnPointSc>();
            LSM_AttackPathUI dummy_path_ui = dummy.pathUI.GetComponent<LSM_AttackPathUI>();

			dummy_path_ui.InvisibleSlider(change_dummy);
			dummy_path_ui.sl.value = item.num;
			foreach (GameObject path in dummy.Paths)
				path.SetActive(change_dummy);
		}
	}

	public void PathUI_ChangeMaxValue()
	{
		selectedNum = 0;
		foreach (MoonHeader.SpawnerPaths item in spawnpoints)
		{
			selectedNum += item.num;
		}

		foreach (MoonHeader.SpawnerPaths item in spawnpoints)
		{
			item.path.GetComponent<LSM_SpawnPointSc>().pathUI.GetComponent<LSM_AttackPathUI>().sl.maxValue =
				item.num + MAX_NUM_MINION - selectedNum;
		}
	}

	public void CheckingSelectMon()
	{
		state = MoonHeader.SpawnerState.None;
		while (true)
		{
			if (selectedNum == MAX_NUM_MINION)
				break;

			int minNum = int.MaxValue, index = -1;
			for (int i = 0; i < spawnpoints.Length; i++)
			{
				if (spawnpoints[i].num < minNum)
				{ index = i; minNum = spawnpoints[i].num; }
			}
			selectedNum++; spawnpoints[index].num++;
		}
		
	}
}
