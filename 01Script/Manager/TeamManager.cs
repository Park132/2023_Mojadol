using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public MoonHeader.Team team;
    // 모든 전황을 추가할 것임.
    int kill, exp;

	// 해당 팀의 플레이어는 리스트로, 해당팀의 기지 (마스터 스포너)는 하나니까 찾아오기.
    public List<LSM_PlayerCtrl> this_teamPlayers;
	public LSM_Spawner this_teamSpawner;

	public int MaximumSpawnNum;
	public int[] AttackPathNumber;
	public int selectedNumber;

	private void Start()
	{
		// 디버깅용
		selectedNumber = 0;
		MaximumSpawnNum = 3;
		this_teamPlayers = new List<LSM_PlayerCtrl>();
		GameObject[] dummyplayer = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject p in dummyplayer) {
			LSM_PlayerCtrl pSC = p.GetComponent<LSM_PlayerCtrl>();
			if (pSC.player.team == this.team) { this_teamPlayers.Add(pSC); }
		}

		GameObject[] dummySpawners = GameObject.FindGameObjectsWithTag("Spawner");
		foreach (GameObject s in dummySpawners) { 
			LSM_Spawner sSC = s.GetComponent<LSM_Spawner>();
			if (sSC.team == this.team) { this_teamSpawner = sSC; break; }
		}
		AttackPathNumber = new int[this_teamSpawner.spawnpoints.Length];
	}

	public void PathUI_ChangeMaxValue()
	{
		selectedNumber = 0;
		foreach (int n in AttackPathNumber)
		{
			selectedNumber += n;
		}

		for (int i = 0; i < AttackPathNumber.Length; i++)
		{
			this_teamSpawner.spawnpoints[i].path.GetComponent<LSM_SpawnPointSc>().pathUI.GetComponent<LSM_AttackPathUI>().sl.maxValue =
				AttackPathNumber[i] + MaximumSpawnNum - selectedNumber;
		}
	}

	// 공격로 선택이 끝난 ㅎ이후 공격로에 얼만큼 지정을 하였는지 확인.
	public void CheckingSelectMon()
	{
		
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
