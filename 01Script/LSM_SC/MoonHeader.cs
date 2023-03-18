using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonHeader : MonoBehaviour
{

	// 게임의 현재 상태를 나타내기 위해 사용
	public enum GameState { ChangeRound, SettingAttackPath, StartGame, Gaming };
	// 매니저의 현재 상태를 나타내기 위해 사용
	public enum ManagerState { Ready, Processing, End };
	public enum SpawnerState { None, Setting, Spawn };
	public enum PlayerState { None, Selected };
	public enum Team { Red = 0, Blue = 1, Yellow = 2 };
	public enum State { Normal, Dead, Attack};

	[Serializable]
	public struct MinionStats
	{
		public State state;
		public Team team;
		public int maxHealth;
		public int health;
		public float speed;
		public int Atk;
		
		public GameObject[] destination;

		public void Setting(int mh, float sp, int atk, GameObject[] des, Team t)
		{ maxHealth = mh; health = mh; speed = sp; destination = des; team = t; state = State.Normal; Atk = atk; }
		

	}

	[Serializable]
	public struct SpawnerPaths
	{
		public GameObject path;
		public int num;
		public int summon_;

		public SpawnerPaths(GameObject p ) { path = p; num = 0; summon_ = 0; }
		
	}

	[Serializable]
	public struct TurretStats
	{
		public Team team;
		public int Health;
		public int Atk;

		public TurretStats(int h, int a) { team = Team.Yellow; Health = h; Atk = a; }

	}
}
