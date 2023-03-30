using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonHeader : MonoBehaviour
{
	public enum GameState { ChangeRound, SettingAttackPath, StartGame, Gaming };	// ������ ���� ���¸� ��Ÿ���� ���� ���.
																					// ChangeRound: ���� ����, SettingAttackPath: ���ݷ� �̴Ͼ� ����, StartGame: ������ �����ϱ� �� ������ ����, Gaming: �������� ��
	public enum ManagerState { Ready, Processing, End };	// ���� �Ŵ����� ���� ���¸� ��Ÿ���� ���� ���. GameState ó���� ������ ��.
															// Ready: ���� �ش� GameState�� ������ �� ����, Processing: �ش� GameState�� ���� ��, End: �ش� GameState�� ����ħ.
	public enum SpawnerState { None, Setting, Spawn };		// �������� ���� ���¸� ��Ÿ���� ���� ���.
															// None: �ƹ��͵� ����, Setting: SettingAttackPath �����϶� �����ʸ� �����ϴ� �ܰ�, Spawn: �����ʰ� �����ϴ� ��.
	public enum State_P { None, Selected , Possession};		// �÷��̾��� ���� ���¸� ��Ÿ���� ���� ���.
															// None: �ƹ��͵� ���ϴ� ��. �ַ� TopView ���������� ����, Seleted: �̴Ͼ��� Ŭ���� ����, Possession: ���� ��.
	public enum Team { Red = 0, Blue = 1, Yellow = 2 };		// ���� ������ ���/
	
	public enum State { Normal, Dead, Attack, Invincibility};	// �̴Ͼ��� ���� ���¸� ��Ÿ���� ���� ���.
																// Normal: ���� ����, Dead: ����, Attack: �����ϴ� ��, Invincibility: ���� ����.
	public enum MonType { Melee, Range };   // ���� Ÿ��
											// Melee: ����, Range: ���Ÿ�

	[Serializable]
	public struct S_ActorState		// ��� ���͵��� ���� ����ü. ü��, ���ݷ�, �� ���� �����ְ� ����.
	{
		public Team team;
		public int maxHealth;
		public int health;
		public int Atk;

		public S_ActorState(int hp, int at, Team t) { team = t; maxHealth = hp; health = maxHealth; Atk = at; }
	}

	[Serializable]
	public struct S_PlayerState	// �÷��̾� ���� ����ü. TopView������ �÷��̾� ���� ����ü
	{
		public State_P statep;	// ���� �÷��̾��� ���¿� ���� ����
		public Team team;		// ���� �÷��̾��� ��
	}

	[Serializable]
	public struct S_MinionStats		// �̴Ͼ��� ���¿� ���õ� ����ü.
	{
		public State state;			// �̴Ͼ��� ���� ���¸� ��Ÿ���� ����
		//public Team team;			// �̴Ͼ��� ��
		//public int maxHealth;		// �̴Ͼ��� �ִ� ü��.
		//public int health;			// �̴Ͼ��� ü��.
		public float speed;			// �̵� �ӵ�
		//public int Atk;				// ���ݷ�
		public MonType type;        // Ÿ�� -> ����, ���Ÿ� ����
		public S_ActorState actorHealth;

		public GameObject[] destination;	// �̴Ͼ��� �̵� ���. �迭�� �޾ƿ�.

		public void Setting(int mh, float sp, int atk, GameObject[] des, Team t)	// �Ʒ� �Լ��� �����ε�. MonType ���� �Ű������� ���� ����.
		{ this.Setting(mh,sp,atk,des,t,MonType.Melee); }
		public void Setting(int mh, float sp, int atk, GameObject[] des, Team t, MonType type_d)	// �̴Ͼ��� ��ȯ�� ���� ���� ������ ���� �Լ�. 
																									// mh: �ִ� ü��, sp: ���ǵ�, atk: ���ݷ�, des: �����ʷκ��� �޾ƿ� �̵����, t: �̴Ͼ��� ��, type_d: �̴Ͼ��� Ÿ��
		{ speed = sp; destination = des; state = State.Normal; type = type_d; actorHealth = new S_ActorState(mh, atk, t); }
		

	}

	[Serializable]
	public struct S_SpawnerPaths    // �ش� �����ʰ� ���� �ִ� ��������Ʈ�� ���� ����ü. �迭�� �ش� ����ü�� ���.
	{
		public GameObject path;	// ��������Ʈ. �ش� �����ʰ� �����մ� ��������Ʈ �� �ϳ�.
		public int num;			// �� ���̺� �� �ش� ��������Ʈ�� ��ȯ�ϴ� �ִ� ������.
		public int summon_;		// �ش� ��������Ʈ�� �� ���̺꿡 ��ȯ�� ������. ���̺갡 ������ 0���� �ʱ�ȭ.

		public S_SpawnerPaths(GameObject p ) { path = p; num = 0; summon_ = 0; }		// ������. p: ��������Ʈ.
		
	}

	[Serializable]
	public struct S_TurretStats	// �ͷ�(��ž)�� ���¿� ���� ����ü.
	{
		//public Team team;		// �ͷ��� ��.
		//public int Health;		// �ͷ��� �ִ� ü��.
		//public int Atk;         // �ͷ��� ���ݷ�
		public S_ActorState actorHealth;

		public S_TurretStats(int h, int a) { actorHealth = new S_ActorState(h, a, Team.Yellow); }		// ������. h: �ִ�ü��, a: ���ݷ�,  ���� ó�� ���� �� �߸�.

	}
}


// �������̽�.
public interface I_Actor		// ��� �����̴� ��ü���� ���� �� �������̽�.
{
	
	int Damaged(int dam, Vector3 origin, MoonHeader.Team t);	// ��� ĳ���ʹ� �������� �ޱ⿡ �߻��Լ��� ����.
}

