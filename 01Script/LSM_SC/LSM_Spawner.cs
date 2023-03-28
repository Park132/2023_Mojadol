using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// �� �� ������ ������!!
public class LSM_Spawner : MonoBehaviour
{
	float BASEDELAY = 1.5f;
	float BASEWAVEDELAY = 10f;
	int BASEMINIONMULTIPLER = 5;
	int BASEMAXIMUMMELEE = 3;

	public int MAX_NUM_MINION ;
	public MoonHeader.Team team;

	public MoonHeader.SpawnerState state;
	//public GameObject[] way;
	public MoonHeader.SpawnerPaths[] spawnpoints;
	


	//public GameObject[] arrowDirect;

	public int wave_Minions_Num, selectedNum;
	//public byte num_attack;
	public float delay;

	private void Awake()
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
		MAX_NUM_MINION = 0;
		
	}

	private void Update()
	{
		CheckingSpawn();
		
	}

	

	private void CheckingSpawn()
	{
		// ���� �������� ���°� ���ݷ� �����̶�� ����
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
			// ������ �������� ��� ��ȯ.
			if (GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
			{
				delay += Time.deltaTime;

				// ���̺� �ִ� ��ȯ������ ���� ��ȯ�ߴٸ�, ��ȯ.
				if (delay > BASEDELAY && wave_Minions_Num < MAX_NUM_MINION)
				{
					delay = 0;

					for (int i = 0; i < spawnpoints.Length; i++)
					{
						if (spawnpoints[i].num > spawnpoints[i].summon_)
						{
							GameObject dummy;
							MoonHeader.MonType monT;
							// ���� �̴Ͼ��� ��ȯ ������ ���� ��ȯ�ƴٸ�, ����.
							if (spawnpoints[i].summon_ % BASEMINIONMULTIPLER < BASEMAXIMUMMELEE)
							{ dummy = PoolManager.Instance.Get_Minion(0); monT = MoonHeader.MonType.Melee; }
							else 
							{ dummy = PoolManager.Instance.Get_Minion(1); monT = MoonHeader.MonType.Range; }

							dummy.transform.position = spawnpoints[i].path.transform.position;
							//dummy.transform.parent = this.transform;
							LSM_MinionCtrl dummy_ctrl = dummy.GetComponent<LSM_MinionCtrl>();
							dummy_ctrl.MonSetting(spawnpoints[i].path.GetComponent<LSM_SpawnPointSc>().Ways, team, this.GetComponent<LSM_Spawner>(), monT);
							dummy_ctrl.minionBelong = i;
							//dummy_ctrl.minionType = spawnpoints[i].summon_ % 2;	//�̴Ͼ��� Ÿ���� ����

							spawnpoints[i].summon_++;
							wave_Minions_Num++;
						}
					}
				}
				// ���� �� ���̺꿡 ��ȯ�� ������ ���� �Ѿ��ٸ�, ���ο� ���̺� ��ȯ �ð����� ���
				else if (wave_Minions_Num >= MAX_NUM_MINION)
				{
					if (delay > BASEWAVEDELAY)
					{
						wave_Minions_Num = 0;
						SettingPath_MinionSpawn();
						
					}
				}
			}
		}

	}

	public void ChangeTurn()
	{
		if (GameManager.Instance.mainPlayer.player.team != this.team) return;
		bool change_dummy = false;
		// ���� ���ݷ� ���� ���϶�
		if (GameManager.Instance.gameState == MoonHeader.GameState.SettingAttackPath)
		{change_dummy = true;}
		// ���ݷ� ���� ���� �ƴ� ���
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

	// ���ݷ� ������ ���� ������ ���ݷο� ��ŭ ������ �Ͽ����� Ȯ��.
	public void CheckingSelectMon()
	{
		state = MoonHeader.SpawnerState.None;
		GameManager.Instance.teamManagers[(int)this.team].CheckingSelectMon();
		if (wave_Minions_Num <= 0)
		{
			wave_Minions_Num = 0;
			SettingPath_MinionSpawn();
		}
	}

	private void SettingPath_MinionSpawn()
	{
		MAX_NUM_MINION = GameManager.Instance.teamManagers[(int)this.team].MaximumSpawnNum * BASEMINIONMULTIPLER;
		for (int i = 0; i < spawnpoints.Length; i++)
		{
			spawnpoints[i].num = GameManager.Instance.teamManagers[(int)this.team].AttackPathNumber[i] * BASEMINIONMULTIPLER;
			spawnpoints[i].summon_ = 0;
		}
	}
}
