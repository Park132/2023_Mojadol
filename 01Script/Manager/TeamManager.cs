using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� �ϳ��� ���Ŵ����� �߰��� ����.
// ���� ų, �۷ι� ���, �����ʰ��� ���� �ش� ��ũ��Ʈ���� ����.
public class TeamManager : MonoBehaviour
{
    public MoonHeader.Team team;		// �ش� ���Ŵ����� ��

    // ��� ��Ȳ�� �߰��� ����.
    int kill, exp;

	// �ش� ���� �÷��̾�� ����Ʈ��, �ش����� ����(������ ������) ã�ƿ���.
    public List<LSM_PlayerCtrl> this_teamPlayers;
	public LSM_Spawner this_teamSpawner;

	public int MaximumSpawnNum;			// �� �� �ִ� ��ȯ ���� ���� ��.
	public int[] AttackPathNumber;		// �������� ��������Ʈ ������ŭ �迭�� ũ�⸦ ��������. ���� ����Ʈ���� ������ �̴Ͼ� ���� ��
	public int selectedNumber;			// ���� �÷��̾ ������ ���ݷ��� ���� ��. �̸� �̿��Ͽ� �����̴��� �ִ� ���� ����.

	private void Start()
	{
		// ������
		selectedNumber = 0;
		MaximumSpawnNum = 3;

		// �ش� ���� �÷��̾���� �޾ƿ�. �ش� �÷��̾ �޾ƿ����� GameManager���� �÷��̾ �������� �����Ͽ� �޾ƿ��� ��.
		this_teamPlayers = new List<LSM_PlayerCtrl>();
		GameObject[] dummyplayer = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject p in dummyplayer) {
			LSM_PlayerCtrl pSC = p.GetComponent<LSM_PlayerCtrl>();
			if (pSC.player.team == this.team) { this_teamPlayers.Add(pSC); }
		}

		// ��� �����ʸ� �޾ƿ� �� ���� �ش��ϴ� �����ʸ� �޾ƿ�. �Ѱ��ۿ� ���ٴ� �������� �ϳ��� �����ͽ����ʸ� �޾ƿ�.
		GameObject[] dummySpawners = GameObject.FindGameObjectsWithTag("Spawner");
		foreach (GameObject s in dummySpawners) { 
			LSM_Spawner sSC = s.GetComponent<LSM_Spawner>();
			if (sSC.team == this.team) { this_teamSpawner = sSC; break; }
		}
		// �����ͽ����ʿ� �����ϴ� ��������Ʈ�� ������ŭ �迭�� ũ�⸦ ����.
		AttackPathNumber = new int[this_teamSpawner.spawnpoints.Length];
	}

	// PathUI�� �����̴� �ִ� ���� �����ϴ� �Լ�.
	public void PathUI_ChangeMaxValue()	
	{
		// selectedNumber �� ���� ���ݷο� ������ ��� ������ ����.
		selectedNumber = 0;
		foreach (int n in AttackPathNumber)
		{ selectedNumber += n; }

		// �� �����̴��� �ִ� ��. �ش� ���ݷο� ������ �� + �ִ� ���� ������ �� - ������ ��
		for (int i = 0; i < AttackPathNumber.Length; i++)
		{
			this_teamSpawner.spawnpoints[i].path.GetComponent<LSM_SpawnPointSc>().pathUI.GetComponent<LSM_AttackPathUI>().sl.maxValue =
				AttackPathNumber[i] + MaximumSpawnNum - selectedNumber;
		}
	}

	// ���ݷ� ������ ���� ���� ���ݷο� ��ŭ ������ �Ͽ����� Ȯ��.
	public void CheckingSelectMon()
	{
		// ���ݷ� ���� ���� ���� ����.. ���� ����Ʈ�� �����Ѵٸ�... -> ���� ���� ������ ���ݷ�(But Top -> Mid -> Bottom������ Ȯ��.)�� �߰�.
		while (true)
		{
			if (selectedNumber >= MaximumSpawnNum)
				break;

			int minNum = int.MaxValue, index = -1;
			for (int i = 0; i < AttackPathNumber.Length; i++)
			{
				if (AttackPathNumber[i] < minNum)
				{ index = i; minNum = AttackPathNumber[i]; }
			}
			selectedNumber++; AttackPathNumber[index]++;
		}

	}
}
