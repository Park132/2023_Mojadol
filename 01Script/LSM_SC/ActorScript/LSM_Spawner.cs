using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// �� �� ������ ������!!
public class LSM_Spawner : MonoBehaviour
{
	// ��ȯ�� ���� ��� ����.
	float BASEDELAY = 1.5f;
	float BASEWAVEDELAY = 10f;
	int BASEMINIONMULTIPLER = 5;
	int BASEMAXIMUMMELEE = 3;

	// �ִ� ��ȯ ������ ��
	public int MAX_NUM_MINION ;
	public MoonHeader.Team team;

	public MoonHeader.SpawnerState state;			// �������� ���� ���¿� ���� enum
	//public GameObject[] way;
	public MoonHeader.S_SpawnerPaths[] spawnpoints;	// �����ʿ� ����� ��������Ʈ
	


	//public GameObject[] arrowDirect;

	public int wave_Minions_Num, selectedNum;
	//public byte num_attack;
	public float delay;

	private void Awake()
	{
		state = MoonHeader.SpawnerState.None;

		//GameObject[] ways = GameObject.FindGameObjectsWithTag("SpawnPoint");
		// ��������Ʈ�� �޾ƿ��� ����.
		List<GameObject> ways = new List<GameObject>();
		foreach (Transform tr in gameObject.GetComponentsInChildren<Transform>())
		{
			if (tr.CompareTag("SpawnPoint"))
				ways.Add(tr.gameObject);
		}
		spawnpoints = new MoonHeader.S_SpawnerPaths[ways.Count];
		for (int i = 0; i < ways.Count; i++)
		{ spawnpoints[i] = new MoonHeader.S_SpawnerPaths(ways[i]); }

		// ���� �ʱ�ȭ
		delay = 0;
		wave_Minions_Num = 0;
		selectedNum = 0;
		MAX_NUM_MINION = 0;
		
	}

	private void Update()
	{
		CheckingSpawn();
		
	}

	

	// ���� �������� ���¸� Ȯ���ϸ� �������� ������ �ϴ� �Լ�
	private void CheckingSpawn()
	{
		// ���� �������� ���°� ���ݷ� �����̶�� ����
		if (state == MoonHeader.SpawnerState.Setting)
		{
			/*
			selectedNum = 0;
			foreach (MoonHeader.SpawnerPaths item in spawnpoints)
			{
				selectedNum += item.num;
			}
			*/
		}
		// ������ ������ ���¶��
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
						// �� ���̺꿡 ��ȯ�� �̴Ͼ��� ������ Ȯ��.
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
							// �̴Ͼ� ����
							LSM_MinionCtrl dummy_ctrl = dummy.GetComponent<LSM_MinionCtrl>();
							LSM_SpawnPointSc dummy_point = spawnpoints[i].path.GetComponent<LSM_SpawnPointSc>();
							dummy_ctrl.MonSetting(dummy_point.Ways, team, this.GetComponent<LSM_Spawner>(), monT);
							dummy_ctrl.minionBelong = dummy_point.number;
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

	//���� ����ɶ����� ���ӸŴ������� ȣ��
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

		// ���ݷ� ������ ���� UI�� ���Ͽ�, �����̴��� ǥ�� ���θ� Ȯ��.
		foreach (MoonHeader.S_SpawnerPaths item in spawnpoints)
		{
			LSM_SpawnPointSc dummy = item.path.GetComponent<LSM_SpawnPointSc>();
            LSM_AttackPathUI dummy_path_ui = dummy.pathUI.GetComponent<LSM_AttackPathUI>();

			dummy_path_ui.InvisibleSlider(change_dummy);
			dummy_path_ui.sl.value = item.num;
			foreach (GameObject path in dummy.Paths)
				path.SetActive(change_dummy);
		}
	}

	// ���ݷ� ������ ���� ���� ���ݷο� ��ŭ ������ �Ͽ����� Ȯ��.
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

	// ���Ŵ������� ������ �������� ����Ǿ��ֱ⿡, ���Ŵ������� �޾ƿ;���.
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
