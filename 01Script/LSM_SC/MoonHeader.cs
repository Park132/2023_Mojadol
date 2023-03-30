using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonHeader : MonoBehaviour
{
	public enum GameState { ChangeRound, SettingAttackPath, StartGame, Gaming };	// 게임의 현재 상태를 나타내기 위해 사용.
																					// ChangeRound: 라운드 변경, SettingAttackPath: 공격로 미니언 지정, StartGame: 게임이 시작하기 전 마무리 세팅, Gaming: 게임진행 중
	public enum ManagerState { Ready, Processing, End };	// 게임 매니저의 현재 상태를 나타내기 위해 사용. GameState 처리에 도움을 줌.
															// Ready: 현재 해당 GameState를 진행할 수 있음, Processing: 해당 GameState를 진행 중, End: 해당 GameState를 끝마침.
	public enum SpawnerState { None, Setting, Spawn };		// 스포너의 현재 상태를 나타내기 위해 사용.
															// None: 아무것도 안함, Setting: SettingAttackPath 상태일때 스포너를 조정하는 단계, Spawn: 스포너가 동작하는 중.
	public enum State_P { None, Selected , Possession};		// 플레이어의 현재 상태를 나타내기 위해 사용.
															// None: 아무것도 안하는 중. 주로 TopView 시점에서의 상태, Seleted: 미니언을 클릭한 시점, Possession: 빙의 중.
	public enum Team { Red = 0, Blue = 1, Yellow = 2 };		// 팀을 나눌때 사용/
	
	public enum State { Normal, Dead, Attack, Invincibility};	// 미니언의 현재 상태를 나타내기 위해 사용.
																// Normal: 현재 생존, Dead: 죽음, Attack: 공격하는 중, Invincibility: 무적 상태.
	public enum MonType { Melee, Range };   // 몬스터 타입
											// Melee: 근접, Range: 원거리

	[Serializable]
	public struct S_ActorState		// 모든 액터들이 갖는 구조체. 체력, 공격력, 팀 등을 갖고있게 설정.
	{
		public Team team;
		public int maxHealth;
		public int health;
		public int Atk;

		public S_ActorState(int hp, int at, Team t) { team = t; maxHealth = hp; health = maxHealth; Atk = at; }
	}

	[Serializable]
	public struct S_PlayerState	// 플레이어 관련 구조체. TopView상태의 플레이어 관련 구조체
	{
		public State_P statep;	// 현재 플레이어의 상태에 대한 변수
		public Team team;		// 현재 플레이어의 팀
	}

	[Serializable]
	public struct S_MinionStats		// 미니언의 상태에 관련된 구조체.
	{
		public State state;			// 미니언의 현재 상태를 나타내는 변수
		//public Team team;			// 미니언의 팀
		//public int maxHealth;		// 미니언의 최대 체력.
		//public int health;			// 미니언의 체력.
		public float speed;			// 이동 속도
		//public int Atk;				// 공격력
		public MonType type;        // 타입 -> 근접, 원거리 구현
		public S_ActorState actorHealth;

		public GameObject[] destination;	// 미니언의 이동 경로. 배열로 받아옴.

		public void Setting(int mh, float sp, int atk, GameObject[] des, Team t)	// 아래 함수의 오버로드. MonType 관련 매개변수를 받지 않음.
		{ this.Setting(mh,sp,atk,des,t,MonType.Melee); }
		public void Setting(int mh, float sp, int atk, GameObject[] des, Team t, MonType type_d)	// 미니언이 소환될 때의 기초 설정을 위한 함수. 
																									// mh: 최대 체력, sp: 스피드, atk: 공격력, des: 스포너로부터 받아올 이동경로, t: 미니언의 팀, type_d: 미니언의 타입
		{ speed = sp; destination = des; state = State.Normal; type = type_d; actorHealth = new S_ActorState(mh, atk, t); }
		

	}

	[Serializable]
	public struct S_SpawnerPaths    // 해당 스포너가 갖고 있는 스폰포인트에 대한 구조체. 배열로 해당 구조체를 사용.
	{
		public GameObject path;	// 스폰포인트. 해당 스포너가 갖고잇는 스폰포인트 중 하나.
		public int num;			// 한 웨이브 당 해당 스폰포인트가 소환하는 최대 마릿수.
		public int summon_;		// 해당 스폰포인트가 현 웨이브에 소환한 마릿수. 웨이브가 끝나면 0으로 초기화.

		public S_SpawnerPaths(GameObject p ) { path = p; num = 0; summon_ = 0; }		// 생성자. p: 스폰포인트.
		
	}

	[Serializable]
	public struct S_TurretStats	// 터렛(포탑)의 상태에 대한 구조체.
	{
		//public Team team;		// 터렛의 팀.
		//public int Health;		// 터렛의 최대 체력.
		//public int Atk;         // 터렛의 공격력
		public S_ActorState actorHealth;

		public S_TurretStats(int h, int a) { actorHealth = new S_ActorState(h, a, Team.Yellow); }		// 생성자. h: 최대체력, a: 공격력,  팀은 처음 시작 시 중립.

	}
}


// 인터페이스.
public interface I_Actor		// 모든 움직이는 객체들이 갖게 한 인터페이스.
{
	
	int Damaged(int dam, Vector3 origin, MoonHeader.Team t);	// 모든 캐릭터는 데미지를 받기에 추상함수로 설정.
}

