using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* 2023_03_20_HSH_수정사항 : 미니언이 소속된 팀에 따라 Scene에서도 Color가 변경되도록 함(CHangeTeamColor).
 * ㄴ 미니언 피격 시 분홍색으로 하이라이트 + 넉백 추가(DamagedEffect)
 *
 */

// 미니언 스크립트.
// 후에 근접, 원거리 등의 미니언들은 해당 스크립트를 상속받고 할 생각임.
public class LSM_MinionCtrl : MonoBehaviour, I_Actor
{
	public MoonHeader.S_MinionStats stats;			// 미니언의 상태에 대한 구조체.
	public LSM_Spawner mySpawner;					// 미니언의 마스터 스포너.
	private bool PlayerSelect, once_changeRound;	// PlayerSelect: 플레이어가 해당 미니언에 강령하였는지, once_changeRound: 라운드 변경 시 한번만 실행되도록.
	[SerializeField] private int way_index;			// 현재 미니언에 저장된 경로 중 몇번째를 목표로 삼는지.

	// 타겟 찾기, 공격 딜래이 등을 상수화
	const float MAXIMUMVELOCITY = 3f, SEARCHTARGET_DELAY = 1f, ATTACK_DELAY = 2f;
	
	// 아래 필요한 컴포넌트를 변수화
	private Rigidbody rigid;
	[SerializeField]private Animator anim;
	private NavMeshAgent nav;
	private NavMeshObstacle nav_ob;
	private IEnumerator navenable_IE;   // StopCorutine을 사용하기 위한 변수
	private bool is_attackFinish_Act;

	private Renderer[] bodies;  // 색상을 변경할 렌더러.

	public GameObject CameraPosition;		// 카메라를 초기화할 때 사용할 변수.
	private GameObject icon, playerIcon;	// 미니언에 존재하는 아이콘, 플레이어 아이콘

	[SerializeField] protected GameObject target_attack;	// 미니언의 타겟
	[SerializeField]
	protected float searchRadius, minAtkRadius, maxAtkRadius;	// 미니언의 탐색 범위, 최소 공격 가능 거리, 최대 공격 가능 거리
	private float timer_Searching, timer_Attack;				// 탐색과 공격의 타이머 변수.

	public int minionBelong;    //Spawner.cs에서 자기가 몇 번 공격로 소속인지 받아옴
	public int minionType;  //0이면 원거리, 1이면 근거리 미니언

	public bool debugging_minion; // 디버깅 확인용...

	// 다시 활성화할 경우 Obstacle 비활성화, Agent 활성화
	void OnEnable()
	{
		nav_ob.enabled = false;
		nav.enabled = false;
	}

	private void Awake()
	{
		PlayerSelect = false;
		// 컴포넌트 받아오기.
		rigid = this.GetComponent<Rigidbody>();
		nav = this.GetComponent<NavMeshAgent>();
		nav_ob = this.GetComponent<NavMeshObstacle>();
		anim = this.GetComponentInChildren<Animator>();
		// 초기화
		timer_Searching = 0;
		timer_Attack = 0;
		target_attack = null;
		nav.stoppingDistance = 1f;
		stats = new MoonHeader.S_MinionStats();
		bodies = this.transform.GetComponentsInChildren<Renderer>();
		navenable_IE = NavEnable(true);

		// 아이콘 생성 및 변수 저장.
		icon = GameObject.Instantiate(PrefabManager.Instance.icons[0], transform);
		icon.transform.localPosition = new Vector3(0, 60, 0);
		playerIcon = GameObject.Instantiate(PrefabManager.Instance.icons[4], transform);
		playerIcon.transform.localPosition = new Vector3(0, 60, 0);
		playerIcon.SetActive(false);

		// 디버그용 미리 설정. 현재 Melee
		//searchRadius = 10f;
		//minAtkRadius = 9f;
		//maxAtkRadius = 13f;

	}
	private void Start()
	{

	}
	private void LateUpdate()
	{
		// 현재 게임의 진행 상태가 어떻게 되는지 확인 후, 상태를 변경.
		if (once_changeRound&&GameManager.Instance.gameState != MoonHeader.GameState.Gaming)
		{
			// 공격하고있지 않으며, Agent가 활성화되어있을 때
			if (nav.enabled && stats.state != MoonHeader.State.Attack)
			{ nav.velocity = Vector3.zero; nav.isStopped = true; }
			rigid.velocity = Vector3.zero;
			rigid.angularVelocity = Vector3.zero;
			once_changeRound = false;
		}
		else if (!once_changeRound && GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
		{
			if (nav.enabled)
				nav.isStopped = false;
			once_changeRound = true;
		}

		// 살아있으며, 스포너 초기화가 되어있고, 현재 게임중인 상태라면 아래 함수들을 실행.
		if (stats.state != MoonHeader.State.Dead && !ReferenceEquals(mySpawner, null) && once_changeRound )
		{
			SearchingTarget();
			Attack();
			MyDestination();
			AnimationSetting();
		}



	}

	// 미니언의 기본 스탯과 목적지를 정하는 함수. Spawner.cs에서 사용
	// 모든 변수 초기화
	public void MonSetting(LSM_SpawnPointSc point, MoonHeader.Team t, LSM_Spawner spawn, MoonHeader.MonType typeM)
	{
		nav_ob.enabled = false;
		nav.enabled = true;
		PlayerSelect = false;
		timer_Searching = 0;
		timer_Attack = 0;
		target_attack = null;
		way_index = 0;
		is_attackFinish_Act = false;
		// maxhealth, speed, atk, paths, team
		// 현재 개발중이므로 미리 설정해둠.
		stats.Setting(10, 4f, 3, point.Ways, t, typeM);
		nav.speed = stats.speed;
		//stats = new MoonHeader.MinionStats(10, 50f, 10, way, t);

		// 스폰포인트에 저장된 공격로로 목적지 지정.
		transform.LookAt(stats.destination[way_index].transform);
		nav.destination = stats.destination[way_index].transform.position;
		//nav.avoidancePriority = 50;
		nav.isStopped = false;

		// 마스터 스포너 지정. 아이콘 활성화.
		mySpawner = spawn;
		icon.SetActive(true);
		playerIcon.SetActive(false);
		rigid.angularVelocity = Vector3.zero;
		rigid.velocity = Vector3.zero;

		// 아이콘 및 몸통 색 팀의 색상에 맞게 변화
		ChangeTeamColor();
		ChangeTeamColor(playerIcon);
		ChangeTeamColor(bodies[0].gameObject);
	}

	// 웨이포인트 트리거에 닿았다면 발동하는 함수.
	// 해당 웨이포인트와 미니언의 현재 목적지가 같은지 확인하는 함수 구현.
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("WayPoint") && stats.state != MoonHeader.State.Dead)
		{
			CheckingTurretTeam(other.transform.gameObject);
		}
	}


	// 미니언이 다음 길로 넘어가는 것을 구현한 함수.
	// NavObstacle을 사용하기 위해 목표 지점을 1/2정도의 위치로 이동하게 설정.
	// 미니언이 가야하는 방향벡터와, 거리를 구한 후 0.5를 곱하여 NavMeshObstacle의 활성/비활성화시의 멈칫거림을 줄이기 위한 코드.
	public void MyDestination()
	{
		if (ReferenceEquals(target_attack, null) && nav.enabled)
		{
			Vector3 destination_direction = (stats.destination[way_index].transform.position - this.transform.position).normalized;
			float destination_distance = Vector3.Distance(stats.destination[way_index].transform.position, this.transform.position);
			nav.destination = this.transform.position + (destination_direction * (destination_distance * 0.5f));
		}
	}

	// 터렛에 대하여. 받아온 터렛이 현재 자신이 목표로한 터렛이 맞는지 확인.
	// 만약 동일하다면 way_index를 상승시켜 다음 지점으로 이동.
	private void CheckingTurretTeam(GameObject obj)
	{
		if (stats.destination[way_index].Equals(obj))
		{
			LSM_TurretSc dummySc = obj.transform.GetComponentInChildren<LSM_TurretSc>();
			if (dummySc.stats.actorHealth.health <= 0 || dummySc.stats.actorHealth.team == this.stats.actorHealth.team)
			{
				way_index++;
			}
		}
	}

	// 미니언이 주변을 공격 대상을 탐색하는 함수.
	private void SearchingTarget()
	{
		// 현재 미니언의 타겟이 존재하는지, 플레이어가 강림 중인지, 미니언이 공격,죽음 등의 상태가 아닌지 확인.
		if (ReferenceEquals(target_attack, null) && !PlayerSelect && stats.state == MoonHeader.State.Normal)
		{
			timer_Searching += Time.deltaTime;
			if (timer_Searching >= SEARCHTARGET_DELAY)
			{
				timer_Searching = 0;

				// 스피어캐스트를 사용하여 일정 반지름 내에 적이 있는지 확인.
				RaycastHit[] hits;
				hits = Physics.SphereCastAll(transform.position, searchRadius, Vector3.up, 0);
				float dummyDistance = float.MaxValue;
				// 범위 내에 존재하는 모든 오브젝트를 확인 후 공격이 가능한지 여부 판단.
				foreach (RaycastHit hit in hits)
				{
					float hit_dummy_distance = Vector3.Distance(transform.position, hit.transform.position);
					if (dummyDistance > hit_dummy_distance)
					{
						bool different_Team = false;

						if (hit.transform.CompareTag("Minion"))
						{ different_Team = (stats.actorHealth.team != hit.transform.GetComponent<LSM_MinionCtrl>().stats.actorHealth.team); }   //자신과 같은 공격로의 미니언만 대상으로 지정
						else if (hit.transform.CompareTag("Turret"))
						{
							different_Team = (stats.actorHealth.team != hit.transform.GetComponent<LSM_TurretSc>().stats.actorHealth.team)
								&& hit.transform.GetComponent<LSM_TurretSc>().TurretBelong == minionBelong;
						}   //자신과 같은 공격로의 터렛만 대상으로 지정
						else if (hit.transform.CompareTag("PlayerMinion"))
						{
							different_Team = (stats.actorHealth.team != hit.transform.GetComponent<PSH_PlayerFPSCtrl>().actorHealth.team);
						}
						else if (hit.transform.CompareTag("Nexus"))
						{
							different_Team = (stats.actorHealth.team != hit.transform.GetComponent<LSM_NexusSC>().stats.actorHealth.team);
						}

						// 팀이 같지 않으며, 가까운 경우 타겟으로 적용.
						if (different_Team)
						{
							dummyDistance = hit_dummy_distance;
							target_attack = hit.transform.gameObject;
						}
					}
				}

				// 타겟을 찾았으며, 이동중이라면.. 자신의 목표를 타겟으로 지정. 해당 지점으로 이동
				if (nav.enabled && !ReferenceEquals(target_attack, null))
					nav.destination = target_attack.transform.position;
				// 
				//else if (ReferenceEquals(target_attack,null) && nav.enabled && nav.isStopped) { nav.isStopped = false; }
				// 타겟을 찾지 못하였으며, NavMeshAgent가 비활성화되어있을경우, Obstacle 비활성화, Agent 활성화
				else if (ReferenceEquals(target_attack,null) && !nav.enabled) {StopCoroutine(navenable_IE); navenable_IE = NavEnable(true); StartCoroutine(navenable_IE); }
			}
		}

		// 타겟을 찾았으며, 플레이어가 강령하지 않았고, 현재 Agent가 활성화 되어있을 경우
		else if (!ReferenceEquals(target_attack, null) && !PlayerSelect && nav.enabled)
		{
			// Agent의 목적지를 타겟의 위치로 설정.
			nav.destination = target_attack.transform.position;
			

			// 레이캐스트를 쏴서 몸체에 가까이 있는지 확인.
			RaycastHit[] hits = Physics.RaycastAll(this.transform.position, (target_attack.transform.position - this.transform.position).normalized, maxAtkRadius);
			//Debug.DrawRay(this.transform.position, (target_attack.transform.position - this.transform.position).normalized * maxAtkRadius, Color.red);
			float dist = Vector3.Distance(target_attack.transform.position, this.transform.position);
			foreach (RaycastHit hit in hits)
			{
				if ((hit.transform.gameObject.Equals(target_attack)))
				{
					/*
					if (debugging_minion)
					{
						Debug.Log("my position = " + this.transform.position + " \n target point position  = " + hit.point + " \n hit.distance = " + hit.distance + 
						"\n distance < minatk = " +(dist <= minAtkRadius).ToString() +"\n hit.distance < minatk = " + (hit.distance < minAtkRadius)); }*/
					dist = hit.distance;
					break;
				}
			}

			// 타겟이 MaxDistance이상 떨어져있다면 타겟을 놓아줌. null지정.
			if (dist > maxAtkRadius && stats.state != MoonHeader.State.Thinking && stats.state != MoonHeader.State.Dead)
			{
				//Debug.Log("target setting null. : distance : " + Vector3.Distance(target_attack.transform.position, this.transform.position) + "target : " +target_attack.name);
				Debug.Log("AttackFinish in Far away");
				StartCoroutine(AttackFin()); 
			}

			// 만약 타겟과의 거리가 최소 공격 가능 거리보다 적다면 공격 함수 호출.
			else if (dist <= minAtkRadius)
			{
				stats.state = MoonHeader.State.Attack;
			}

		}

	}

	// 미니언이 살아있으며, 타겟을 찾았다면 해당 함수를 실행.
	// 원래 공격할때 NavAgent를 비활성화, NavObstacle을 활성화 하였으나, 이를 실행하면 NavMesh를 통한 길찾기를 실시간으로 다시 반복하는 문제가있음.
	// 그러므로 NavAgent의 Priority를 하강시키는 것으로 해당 미니언을 밀치지 않게 설정.
	// 또 다시 변경... NavObstacle을 사용. 
	
	private void Attack()
	{
		if (timer_Attack <= ATTACK_DELAY) { timer_Attack += Time.deltaTime; }
		
		// 타겟이 존재한다면.
		if (!ReferenceEquals(target_attack, null))
		{
			// 타겟이 파괴되었다면. -> 현재 ObjectPooling을 사용하고있으므로, ActiveSelf를 사용하여 현재 활성/비활성 상태를 확인.
			if (!target_attack.activeSelf && stats.state != MoonHeader.State.Thinking && stats.state != MoonHeader.State.Dead)
			{Debug.Log("Attack Finish in Destroy"); StartCoroutine(AttackFin()); }

			else if (stats.state == MoonHeader.State.Attack && !PlayerSelect)
			{
				// 언제 다시 NavMeshObstacle을 사용 안할지 모르기에 해당 부분을 주석처리로 남겨두었음.
				//bool dummy_cant_attack = Vector3.Distance(target_attack.transform.position, this.transform.position) > minAtkRadius * (nav.isStopped ? 1f : 0.7f);
				//if (!dummy_cant_attack) { nav.isStopped = true; nav.avoidancePriority = 10; }
				//else { nav.isStopped = false; nav.avoidancePriority = 50; }

				// 타겟과의 거리가 minAtkRadius보다 크다면 공격이 불가능.
				//-> Agent가 켜져있는 경우(타겟을 공격하기 위하여 움직이고 있는 경우)에는 오차 및 약간의 움직임에 대비하여 좀 더 가까이 다가가게 구현.

				// 레이캐스트를 쏴서 몸체에 가까이 있는지 확인.
				RaycastHit[] hits = Physics.RaycastAll(this.transform.position, (target_attack.transform.position - this.transform.position).normalized, maxAtkRadius);
				float dist = Vector3.Distance(target_attack.transform.position, this.transform.position);
				foreach (RaycastHit hit in hits)
				{
					if (hit.transform.gameObject.Equals(target_attack))
					{
						dist = Vector3.Distance(hit.point, this.transform.position);
						break;
					}
				}

				bool dummy_cant_attack = dist > minAtkRadius * (nav.enabled ? 0.7f : 1f);

				// 공격이 불가능한 경우, 가능한 경우에 따라 Obstacl, Agent를 활성/비활성.
				// Agent 및 Obstacle을 동시에 사용한다면 오류 발생 -> 자신 또한 장애물이라 생각하며 자신이 있는 길을 피하려는 모순
				// 그렇기에 Obstacle과 Agent를 서로 키고 끄고를 하는 것임. 허나 비활성화한다고 바로 비활성화되지는 않은듯함.
				// 약간의 텀을 주지 않는다면 서로 충돌하여 팅겨나가는 경우가 존재함. 간단하게 해결하기 위하여 변환되는 순간 속도를 0으로 설정.
				if (dummy_cant_attack && !nav.enabled) {
					//Debug.Log("cant attack!"); //StopCoroutine(navenable_IE);
					navenable_IE = NavEnable(true); StartCoroutine(navenable_IE); }
				else if (!dummy_cant_attack && nav.enabled) {
					//Debug.Log("can attack!!"); //StopCoroutine(navenable_IE); 
					navenable_IE = NavEnable(false); StartCoroutine(navenable_IE); rigid.velocity = Vector3.zero; } //여기 오류. 아마도 공격이 끝나고 계속 불러오는듯.

				// 만약 공격이 가능하다면 공격하는 구문
				if (!dummy_cant_attack)
				{
					// y축 rotation만을 변경할 것임.
					this.transform.LookAt(target_attack.transform.position);
					this.transform.rotation = Quaternion.Euler(Vector3.Scale(this.transform.rotation.eulerAngles, Vector3.up));
					if (timer_Attack >= ATTACK_DELAY)
					{
						timer_Attack = 0;
						// 공격 애니메이션 실행. 지금은 즉발. 하지만 발사체를 사용할거면 이때 소환.
						anim.SetTrigger("Attack");
						switch (target_attack.tag)
						{
							case "Minion":
								Attack_other<LSM_MinionCtrl>(target_attack);

								break;
							case "Turret":
								Attack_other<LSM_TurretSc>(target_attack);
								LSM_TurretSc dummy_Sc = target_attack.GetComponent<LSM_TurretSc>();
								if (dummy_Sc.stats.actorHealth.team == stats.actorHealth.team && stats.state == MoonHeader.State.Attack && stats.state != MoonHeader.State.Dead)
								{ CheckingTurretTeam(target_attack.transform.parent.gameObject); StartCoroutine(AttackFin()); Debug.Log("Attack Finish in Turret destroy"); }

								break;
							case "PlayerMinion":
								Attack_other<PSH_PlayerFPSCtrl>(target_attack);
								break;
						}

					}
				}
			}
		}

	}

	// Generic 변수를 사용하여 해당 구문을 함수화. IActor 인터페이스는 현재 player, turret, minion가 구현하고있음.
	// 따라서 Damaged를 호출이 가능함.
	private void Attack_other<T>(GameObject other) where T : I_Actor {
		T Script = other.GetComponent<T>();
		Script.Damaged(this.stats.actorHealth.Atk, this.transform.position, this.stats.actorHealth.team, this.gameObject);
	}

	
	// 미니언이 데미지를 받을 때 사용하는 함수.
	// dam = 미니언 혹은 포탑의 공격력. 미니언이 받는 데미지.
	// origin = 공격을 하는 주체의 위치. 이를 이용하여 더욱 자연스러운 넉백이 가능해짐. // 현재 넉백을 제외하고있음.

	public void Damaged(int dam, Vector3 origin, MoonHeader.Team t, GameObject other)
	{
		// 죽음 혹은 무적 상태일 경우 데미지를 입지않음. 바로 return
		if (stats.state == MoonHeader.State.Invincibility || stats.state == MoonHeader.State.Dead)
			return;
		else if (t == this.stats.actorHealth.team)
			return;

		stats.actorHealth.health -= dam;
		StartCoroutine(DamagedEffect(origin));

		//Debug.Log("Minion Damaged!! : " +stats.health);
		// 체력이 0 이하라면 DeadProcessing
		if (stats.actorHealth.health <= 0 && stats.state != MoonHeader.State.Dead)
		{
			StartCoroutine(DeadProcessing(other));
		}
		return;
	}

	// 체력이 0 이하일 경우 호출.
	// 프로토 타입에서는 0.5초이후 비활성화.
	private IEnumerator DeadProcessing(GameObject other)
	{
		stats.state = MoonHeader.State.Dead;
		if (nav.enabled)
		{nav.velocity = Vector3.zero; nav.isStopped = true; }
		if (other.transform.CompareTag("PlayerMinion"))
		{
			other.GetComponent<PSH_PlayerFPSCtrl>().myPlayerCtrl.GetExp(50);   // 디버깅용으로 현재 경험치를 50으로 고정 지급.
																			   // 디버깅용 플레이어가 미니언을 처치하였다면..
			GameManager.Instance.DisplayAdd(string.Format("{0} killed {1}", other.name, this.name));
		}
		

		yield return new WaitForSeconds(0.5f);
		this.gameObject.SetActive(false);
	}

	// LSM 변경. 모든 적의 공격이 미니언의 앞에서만 오지 않을 수 있음.
	// 그러므로 해당 미니언의 위치를 받아와 방향 벡터를 얻고, 그 방향벡터로 일정 힘의 크기로 AddForce
	private IEnumerator DamagedEffect(Vector3 origin)
	{
		Color damagedColor = new Color32(255, 150, 150, 255);
		
        //Vector3 knockbackDirection = Vector3.Scale(this.transform.position - origin, Vector3.zero - Vector3.up).normalized * 500 + Vector3.up * 100;

		foreach (Renderer r in bodies)
		{ r.material.color = damagedColor; }
		//this.rigid.AddForce(knockbackDirection);

        yield return new WaitForSeconds(0.25f);
		foreach (Renderer r in bodies)
		{ r.material.color = Color.white; }

		// 첫번째 렌더러 팀색으로 변경.
		ChangeTeamColor(bodies[0].gameObject);
	}

	// 공격이 끝났을 때 호출.
	// 공격 타겟을 null로 초기화, Obstacle 비활성화, Agent 활성화. 이때 약간의 텀이 존재해야함.
	// 텀이 존재하지 않을 경우, 자신의 너비만큼 순간이동.
    protected IEnumerator AttackFin()
    {
		if (!PlayerSelect && !is_attackFinish_Act)
		{
			is_attackFinish_Act = true;
			Debug.Log("Attack Finish");
			this.stats.state = MoonHeader.State.Thinking;

			StopCoroutine(navenable_IE);
			navenable_IE = NavEnable(true);
			yield return StartCoroutine(navenable_IE);
			target_attack = null;
			this.stats.state = MoonHeader.State.Normal;
			
			//nav.isStopped = false;
			timer_Searching = SEARCHTARGET_DELAY;
			is_attackFinish_Act = false;
		}
    }

	// NavMesh Agent와 Obstacle을 동시에 키면 오류.
	// 그렇다고 텀을 안주고 키면 순간이동 버그.
	// true라면 Agent를 킴.
	// false라면 Obstacle을 킴.
	protected IEnumerator NavEnable(bool on)
	{
		rigid.velocity = Vector3.zero;
		if (!on)
		{
			nav.enabled = false;
			yield return new WaitForSeconds(0.1f);
			nav_ob.enabled = true;
		}
		else
		{
			nav_ob.enabled = false;
			yield return new WaitForSeconds(0.1f);
			nav.enabled = true;

		}
	}

	// 아이콘 및 몸체 색 변경.
    #region ChangeTeamColors
    // 오버로드. 매개변수가 존재하지 않을경우 미니언의 아이콘의 색상을 변경.
    public void ChangeTeamColor() { ChangeTeamColor(icon); }

	// 시작 혹은 생성할 때 미니언의 아이콘 등의 색상을 변경.
    public void ChangeTeamColor(GameObject obj)
	{
		Color dummy_color;
		switch (stats.actorHealth.team)
		{
			case MoonHeader.Team.Red:
				dummy_color = Color.red;
				break;
			case MoonHeader.Team.Blue:
				dummy_color = Color.blue;
				break;
			case MoonHeader.Team.Yellow:
				dummy_color = Color.yellow;
				break;
			default: dummy_color = Color.gray; break;
		}
		obj.GetComponent<Renderer>().material.color = dummy_color;
		//this.gameObject.GetComponent<Renderer>().material.color = dummy_color;	//UI에서 뿐만 아니라 Scene에서도 색상이 변경
		// Scene 즉 게임 화면에서 팀마다 색상이 변하는 것은... 나중에 파티클이나 이펙트로 하는건 어떤지? 이에 대한 내용은 일단 유지... 허나 데미지 받으면 흰색.
	}
    #endregion

    // 플레이어가 해당 미니언에게 강령
    public void PlayerConnect()
	{
		PlayerSelect = true;

		navenable_IE = NavEnable(false);
		StartCoroutine(navenable_IE);

		icon.SetActive(false);
		playerIcon.SetActive(true);
		
		//stats.team = MoonHeader.Team.Blue;
	}

	// 플레이어가 해당 미니언에게서 나옴.
	// 어디에서 빠져나왔을지 모르니, navmesh의 목적지를 재 설정해야함.
	public void PlayerDisConnect()
	{
		PlayerSelect = false;
		nav_ob.enabled = false;
		nav.enabled = true;

		icon.SetActive(true);
		playerIcon.SetActive(false);
		MyDestination();
	}

	// 플레이어가 해당 미니언을 탑뷰일 때 선택하였을 경우.
	public void PlayerSelected()
	{
		this.icon.GetComponent<Renderer>().material.color = Color.green;
	}

	// 애니메이션 관련 변수를 변경해주는 함수
	private void AnimationSetting()
	{
		if (nav.enabled)
			anim.SetFloat("Velocity", Vector3.Magnitude(nav.velocity));
		else
			anim.SetFloat("Velocity", 0f);
	}

	// I_Actor 구현 함수
    #region I_Actor
    public int GetHealth(){return this.stats.actorHealth.health;}
	public int GetMaxHealth() { return this.stats.actorHealth.maxHealth; }
	public MoonHeader.Team GetTeam() { return this.stats.actorHealth.team; }
    #endregion
}
