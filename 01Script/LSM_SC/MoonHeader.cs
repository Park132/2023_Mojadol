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
	public enum State_P { None, Selected , Possession};
	public enum Team { Red = 0, Blue = 1, Yellow = 2 };
	// 아래는 미니언용 enum class
	public enum State { Normal, Dead, Attack, Invincibility};
	public enum MonType { Melee,  };

	[Serializable]
	public struct PlayerState
	{
		public State_P statep;
		public Team team;
	}

	[Serializable]
	public struct MinionStats
	{
		public State state;
		public Team team;
		public int maxHealth;
		public int health;
		public float speed;
		public int Atk;
		public MonType type;
		
		public GameObject[] destination;

		public void Setting(int mh, float sp, int atk, GameObject[] des, Team t)
		{ this.Setting(mh,sp,atk,des,t,MonType.Melee); }
		public void Setting(int mh, float sp, int atk, GameObject[] des, Team t, MonType type_d)
		{ maxHealth = mh; health = mh; speed = sp; destination = des; team = t; state = State.Normal; Atk = atk; type = type_d; }
		

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

		public TurretStats(int h, int a) { team = Team.Yellow; Health = h; Atk = a;}

	}
}


public interface IActor
{
	int Damaged(int dam, Vector3 origin, MoonHeader.Team t);
}