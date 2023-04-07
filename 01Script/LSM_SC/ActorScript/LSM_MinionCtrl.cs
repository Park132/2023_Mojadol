using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* 2023_03_20_HSH_�������� : �̴Ͼ��� �Ҽӵ� ���� ���� Scene������ Color�� ����ǵ��� ��(CHangeTeamColor).
 * �� �̴Ͼ� �ǰ� �� ��ȫ������ ���̶���Ʈ + �˹� �߰�(DamagedEffect)
 *
 */

// �̴Ͼ� ��ũ��Ʈ.
// �Ŀ� ����, ���Ÿ� ���� �̴Ͼ���� �ش� ��ũ��Ʈ�� ��ӹް� �� ������.
public class LSM_MinionCtrl : MonoBehaviour, I_Actor
{
	public MoonHeader.S_MinionStats stats;			// �̴Ͼ��� ���¿� ���� ����ü.
	public LSM_Spawner mySpawner;					// �̴Ͼ��� ������ ������.
	private bool PlayerSelect, once_changeRound;	// PlayerSelect: �÷��̾ �ش� �̴Ͼ� �����Ͽ�����, once_changeRound: ���� ���� �� �ѹ��� ����ǵ���.
	[SerializeField] private int way_index;			// ���� �̴Ͼ� ����� ��� �� ���°�� ��ǥ�� �����.

	// Ÿ�� ã��, ���� ������ ���� ���ȭ
	const float MAXIMUMVELOCITY = 3f, SEARCHTARGET_DELAY = 1f, ATTACK_DELAY = 2f;
	
	// �Ʒ� �ʿ��� ������Ʈ�� ����ȭ
	private Rigidbody rigid;
	[SerializeField]private Animator anim;
	private NavMeshAgent nav;
	private NavMeshObstacle nav_ob;
	private IEnumerator navenable_IE;   // StopCorutine�� ����ϱ� ���� ����
	private bool is_attackFinish_Act;

	private Renderer[] bodies;  // ������ ������ ������.

	public GameObject CameraPosition;		// ī�޶� �ʱ�ȭ�� �� ����� ����.
	private GameObject icon, playerIcon;	// �̴Ͼ� �����ϴ� ������, �÷��̾� ������

	[SerializeField] protected GameObject target_attack;	// �̴Ͼ��� Ÿ��
	[SerializeField]
	protected float searchRadius, minAtkRadius, maxAtkRadius;	// �̴Ͼ��� Ž�� ����, �ּ� ���� ���� �Ÿ�, �ִ� ���� ���� �Ÿ�
	private float timer_Searching, timer_Attack;				// Ž���� ������ Ÿ�̸� ����.

	public int minionBelong;    //Spawner.cs���� �ڱⰡ �� �� ���ݷ� �Ҽ����� �޾ƿ�
	public int minionType;  //0�̸� ���Ÿ�, 1�̸� �ٰŸ� �̴Ͼ�

	public bool debugging_minion; // ����� Ȯ�ο�...

	// �ٽ� Ȱ��ȭ�� ��� Obstacle ��Ȱ��ȭ, Agent Ȱ��ȭ
	void OnEnable()
	{
		nav_ob.enabled = false;
		nav.enabled = false;
	}

	private void Awake()
	{
		PlayerSelect = false;
		// ������Ʈ �޾ƿ���.
		rigid = this.GetComponent<Rigidbody>();
		nav = this.GetComponent<NavMeshAgent>();
		nav_ob = this.GetComponent<NavMeshObstacle>();
		anim = this.GetComponentInChildren<Animator>();
		// �ʱ�ȭ
		timer_Searching = 0;
		timer_Attack = 0;
		target_attack = null;
		nav.stoppingDistance = 1f;
		stats = new MoonHeader.S_MinionStats();
		bodies = this.transform.GetComponentsInChildren<Renderer>();
		navenable_IE = NavEnable(true);

		// ������ ���� �� ���� ����.
		icon = GameObject.Instantiate(PrefabManager.Instance.icons[0], transform);
		icon.transform.localPosition = new Vector3(0, 60, 0);
		playerIcon = GameObject.Instantiate(PrefabManager.Instance.icons[4], transform);
		playerIcon.transform.localPosition = new Vector3(0, 60, 0);
		playerIcon.SetActive(false);

		// ����׿� �̸� ����. ���� Melee
		//searchRadius = 10f;
		//minAtkRadius = 9f;
		//maxAtkRadius = 13f;

	}
	private void Start()
	{

	}
	private void LateUpdate()
	{
		// ���� ������ ���� ���°� ��� �Ǵ��� Ȯ�� ��, ���¸� ����.
		if (once_changeRound&&GameManager.Instance.gameState != MoonHeader.GameState.Gaming)
		{
			// �����ϰ����� ������, Agent�� Ȱ��ȭ�Ǿ����� ��
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

		// ���������, ������ �ʱ�ȭ�� �Ǿ��ְ�, ���� �������� ���¶�� �Ʒ� �Լ����� ����.
		if (stats.state != MoonHeader.State.Dead && !ReferenceEquals(mySpawner, null) && once_changeRound )
		{
			SearchingTarget();
			Attack();
			MyDestination();
			AnimationSetting();
		}



	}

	// �̴Ͼ��� �⺻ ���Ȱ� �������� ���ϴ� �Լ�. Spawner.cs���� ���
	// ��� ���� �ʱ�ȭ
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
		// ���� �������̹Ƿ� �̸� �����ص�.
		stats.Setting(10, 4f, 3, point.Ways, t, typeM);
		nav.speed = stats.speed;
		//stats = new MoonHeader.MinionStats(10, 50f, 10, way, t);

		// ��������Ʈ�� ����� ���ݷη� ������ ����.
		transform.LookAt(stats.destination[way_index].transform);
		nav.destination = stats.destination[way_index].transform.position;
		//nav.avoidancePriority = 50;
		nav.isStopped = false;

		// ������ ������ ����. ������ Ȱ��ȭ.
		mySpawner = spawn;
		icon.SetActive(true);
		playerIcon.SetActive(false);
		rigid.angularVelocity = Vector3.zero;
		rigid.velocity = Vector3.zero;

		// ������ �� ���� �� ���� ���� �°� ��ȭ
		ChangeTeamColor();
		ChangeTeamColor(playerIcon);
		ChangeTeamColor(bodies[0].gameObject);
	}

	// ��������Ʈ Ʈ���ſ� ��Ҵٸ� �ߵ��ϴ� �Լ�.
	// �ش� ��������Ʈ�� �̴Ͼ��� ���� �������� ������ Ȯ���ϴ� �Լ� ����.
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("WayPoint") && stats.state != MoonHeader.State.Dead)
		{
			CheckingTurretTeam(other.transform.gameObject);
		}
	}


	// �̴Ͼ��� ���� ��� �Ѿ�� ���� ������ �Լ�.
	// NavObstacle�� ����ϱ� ���� ��ǥ ������ 1/2������ ��ġ�� �̵��ϰ� ����.
	// �̴Ͼ��� �����ϴ� ���⺤�Ϳ�, �Ÿ��� ���� �� 0.5�� ���Ͽ� NavMeshObstacle�� Ȱ��/��Ȱ��ȭ���� ��ĩ�Ÿ��� ���̱� ���� �ڵ�.
	public void MyDestination()
	{
		if (ReferenceEquals(target_attack, null) && nav.enabled)
		{
			Vector3 destination_direction = (stats.destination[way_index].transform.position - this.transform.position).normalized;
			float destination_distance = Vector3.Distance(stats.destination[way_index].transform.position, this.transform.position);
			nav.destination = this.transform.position + (destination_direction * (destination_distance * 0.5f));
		}
	}

	// �ͷ��� ���Ͽ�. �޾ƿ� �ͷ��� ���� �ڽ��� ��ǥ���� �ͷ��� �´��� Ȯ��.
	// ���� �����ϴٸ� way_index�� ��½��� ���� �������� �̵�.
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

	// �̴Ͼ��� �ֺ��� ���� ����� Ž���ϴ� �Լ�.
	private void SearchingTarget()
	{
		// ���� �̴Ͼ��� Ÿ���� �����ϴ���, �÷��̾ ���� ������, �̴Ͼ��� ����,���� ���� ���°� �ƴ��� Ȯ��.
		if (ReferenceEquals(target_attack, null) && !PlayerSelect && stats.state == MoonHeader.State.Normal)
		{
			timer_Searching += Time.deltaTime;
			if (timer_Searching >= SEARCHTARGET_DELAY)
			{
				timer_Searching = 0;

				// ���Ǿ�ĳ��Ʈ�� ����Ͽ� ���� ������ ���� ���� �ִ��� Ȯ��.
				RaycastHit[] hits;
				hits = Physics.SphereCastAll(transform.position, searchRadius, Vector3.up, 0);
				float dummyDistance = float.MaxValue;
				// ���� ���� �����ϴ� ��� ������Ʈ�� Ȯ�� �� ������ �������� ���� �Ǵ�.
				foreach (RaycastHit hit in hits)
				{
					float hit_dummy_distance = Vector3.Distance(transform.position, hit.transform.position);
					if (dummyDistance > hit_dummy_distance)
					{
						bool different_Team = false;

						if (hit.transform.CompareTag("Minion"))
						{ different_Team = (stats.actorHealth.team != hit.transform.GetComponent<LSM_MinionCtrl>().stats.actorHealth.team); }   //�ڽŰ� ���� ���ݷ��� �̴Ͼ� ������� ����
						else if (hit.transform.CompareTag("Turret"))
						{
							different_Team = (stats.actorHealth.team != hit.transform.GetComponent<LSM_TurretSc>().stats.actorHealth.team)
								&& hit.transform.GetComponent<LSM_TurretSc>().TurretBelong == minionBelong;
						}   //�ڽŰ� ���� ���ݷ��� �ͷ��� ������� ����
						else if (hit.transform.CompareTag("PlayerMinion"))
						{
							different_Team = (stats.actorHealth.team != hit.transform.GetComponent<PSH_PlayerFPSCtrl>().actorHealth.team);
						}
						else if (hit.transform.CompareTag("Nexus"))
						{
							different_Team = (stats.actorHealth.team != hit.transform.GetComponent<LSM_NexusSC>().stats.actorHealth.team);
						}

						// ���� ���� ������, ����� ��� Ÿ������ ����.
						if (different_Team)
						{
							dummyDistance = hit_dummy_distance;
							target_attack = hit.transform.gameObject;
						}
					}
				}

				// Ÿ���� ã������, �̵����̶��.. �ڽ��� ��ǥ�� Ÿ������ ����. �ش� �������� �̵�
				if (nav.enabled && !ReferenceEquals(target_attack, null))
					nav.destination = target_attack.transform.position;
				// 
				//else if (ReferenceEquals(target_attack,null) && nav.enabled && nav.isStopped) { nav.isStopped = false; }
				// Ÿ���� ã�� ���Ͽ�����, NavMeshAgent�� ��Ȱ��ȭ�Ǿ��������, Obstacle ��Ȱ��ȭ, Agent Ȱ��ȭ
				else if (ReferenceEquals(target_attack,null) && !nav.enabled) {StopCoroutine(navenable_IE); navenable_IE = NavEnable(true); StartCoroutine(navenable_IE); }
			}
		}

		// Ÿ���� ã������, �÷��̾ �������� �ʾҰ�, ���� Agent�� Ȱ��ȭ �Ǿ����� ���
		else if (!ReferenceEquals(target_attack, null) && !PlayerSelect && nav.enabled)
		{
			// Agent�� �������� Ÿ���� ��ġ�� ����.
			nav.destination = target_attack.transform.position;
			

			// ����ĳ��Ʈ�� ���� ��ü�� ������ �ִ��� Ȯ��.
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

			// Ÿ���� MaxDistance�̻� �������ִٸ� Ÿ���� ������. null����.
			if (dist > maxAtkRadius && stats.state != MoonHeader.State.Thinking && stats.state != MoonHeader.State.Dead)
			{
				//Debug.Log("target setting null. : distance : " + Vector3.Distance(target_attack.transform.position, this.transform.position) + "target : " +target_attack.name);
				Debug.Log("AttackFinish in Far away");
				StartCoroutine(AttackFin()); 
			}

			// ���� Ÿ�ٰ��� �Ÿ��� �ּ� ���� ���� �Ÿ����� ���ٸ� ���� �Լ� ȣ��.
			else if (dist <= minAtkRadius)
			{
				stats.state = MoonHeader.State.Attack;
			}

		}

	}

	// �̴Ͼ��� ���������, Ÿ���� ã�Ҵٸ� �ش� �Լ��� ����.
	// ���� �����Ҷ� NavAgent�� ��Ȱ��ȭ, NavObstacle�� Ȱ��ȭ �Ͽ�����, �̸� �����ϸ� NavMesh�� ���� ��ã�⸦ �ǽð����� �ٽ� �ݺ��ϴ� ����������.
	// �׷��Ƿ� NavAgent�� Priority�� �ϰ���Ű�� ������ �ش� �̴Ͼ��� ��ġ�� �ʰ� ����.
	// �� �ٽ� ����... NavObstacle�� ���. 
	
	private void Attack()
	{
		if (timer_Attack <= ATTACK_DELAY) { timer_Attack += Time.deltaTime; }
		
		// Ÿ���� �����Ѵٸ�.
		if (!ReferenceEquals(target_attack, null))
		{
			// Ÿ���� �ı��Ǿ��ٸ�. -> ���� ObjectPooling�� ����ϰ������Ƿ�, ActiveSelf�� ����Ͽ� ���� Ȱ��/��Ȱ�� ���¸� Ȯ��.
			if (!target_attack.activeSelf && stats.state != MoonHeader.State.Thinking && stats.state != MoonHeader.State.Dead)
			{Debug.Log("Attack Finish in Destroy"); StartCoroutine(AttackFin()); }

			else if (stats.state == MoonHeader.State.Attack && !PlayerSelect)
			{
				// ���� �ٽ� NavMeshObstacle�� ��� ������ �𸣱⿡ �ش� �κ��� �ּ�ó���� ���ܵξ���.
				//bool dummy_cant_attack = Vector3.Distance(target_attack.transform.position, this.transform.position) > minAtkRadius * (nav.isStopped ? 1f : 0.7f);
				//if (!dummy_cant_attack) { nav.isStopped = true; nav.avoidancePriority = 10; }
				//else { nav.isStopped = false; nav.avoidancePriority = 50; }

				// Ÿ�ٰ��� �Ÿ��� minAtkRadius���� ũ�ٸ� ������ �Ұ���.
				//-> Agent�� �����ִ� ���(Ÿ���� �����ϱ� ���Ͽ� �����̰� �ִ� ���)���� ���� �� �ణ�� �����ӿ� ����Ͽ� �� �� ������ �ٰ����� ����.

				// ����ĳ��Ʈ�� ���� ��ü�� ������ �ִ��� Ȯ��.
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

				// ������ �Ұ����� ���, ������ ��쿡 ���� Obstacl, Agent�� Ȱ��/��Ȱ��.
				// Agent �� Obstacle�� ���ÿ� ����Ѵٸ� ���� �߻� -> �ڽ� ���� ��ֹ��̶� �����ϸ� �ڽ��� �ִ� ���� ���Ϸ��� ���
				// �׷��⿡ Obstacle�� Agent�� ���� Ű�� ���� �ϴ� ����. �㳪 ��Ȱ��ȭ�Ѵٰ� �ٷ� ��Ȱ��ȭ������ ��������.
				// �ణ�� ���� ���� �ʴ´ٸ� ���� �浹�Ͽ� �ðܳ����� ��찡 ������. �����ϰ� �ذ��ϱ� ���Ͽ� ��ȯ�Ǵ� ���� �ӵ��� 0���� ����.
				if (dummy_cant_attack && !nav.enabled) {
					//Debug.Log("cant attack!"); //StopCoroutine(navenable_IE);
					navenable_IE = NavEnable(true); StartCoroutine(navenable_IE); }
				else if (!dummy_cant_attack && nav.enabled) {
					//Debug.Log("can attack!!"); //StopCoroutine(navenable_IE); 
					navenable_IE = NavEnable(false); StartCoroutine(navenable_IE); rigid.velocity = Vector3.zero; } //���� ����. �Ƹ��� ������ ������ ��� �ҷ����µ�.

				// ���� ������ �����ϴٸ� �����ϴ� ����
				if (!dummy_cant_attack)
				{
					// y�� rotation���� ������ ����.
					this.transform.LookAt(target_attack.transform.position);
					this.transform.rotation = Quaternion.Euler(Vector3.Scale(this.transform.rotation.eulerAngles, Vector3.up));
					if (timer_Attack >= ATTACK_DELAY)
					{
						timer_Attack = 0;
						// ���� �ִϸ��̼� ����. ������ ���. ������ �߻�ü�� ����ҰŸ� �̶� ��ȯ.
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

	// Generic ������ ����Ͽ� �ش� ������ �Լ�ȭ. IActor �������̽��� ���� player, turret, minion�� �����ϰ�����.
	// ���� Damaged�� ȣ���� ������.
	private void Attack_other<T>(GameObject other) where T : I_Actor {
		T Script = other.GetComponent<T>();
		Script.Damaged(this.stats.actorHealth.Atk, this.transform.position, this.stats.actorHealth.team, this.gameObject);
	}

	
	// �̴Ͼ��� �������� ���� �� ����ϴ� �Լ�.
	// dam = �̴Ͼ� Ȥ�� ��ž�� ���ݷ�. �̴Ͼ��� �޴� ������.
	// origin = ������ �ϴ� ��ü�� ��ġ. �̸� �̿��Ͽ� ���� �ڿ������� �˹��� ��������. // ���� �˹��� �����ϰ�����.

	public void Damaged(int dam, Vector3 origin, MoonHeader.Team t, GameObject other)
	{
		// ���� Ȥ�� ���� ������ ��� �������� ��������. �ٷ� return
		if (stats.state == MoonHeader.State.Invincibility || stats.state == MoonHeader.State.Dead)
			return;
		else if (t == this.stats.actorHealth.team)
			return;

		stats.actorHealth.health -= dam;
		StartCoroutine(DamagedEffect(origin));

		//Debug.Log("Minion Damaged!! : " +stats.health);
		// ü���� 0 ���϶�� DeadProcessing
		if (stats.actorHealth.health <= 0 && stats.state != MoonHeader.State.Dead)
		{
			StartCoroutine(DeadProcessing(other));
		}
		return;
	}

	// ü���� 0 ������ ��� ȣ��.
	// ������ Ÿ�Կ����� 0.5������ ��Ȱ��ȭ.
	private IEnumerator DeadProcessing(GameObject other)
	{
		stats.state = MoonHeader.State.Dead;
		if (nav.enabled)
		{nav.velocity = Vector3.zero; nav.isStopped = true; }
		if (other.transform.CompareTag("PlayerMinion"))
		{
			other.GetComponent<PSH_PlayerFPSCtrl>().myPlayerCtrl.GetExp(50);   // ���������� ���� ����ġ�� 50���� ���� ����.
																			   // ������ �÷��̾ �̴Ͼ��� óġ�Ͽ��ٸ�..
			GameManager.Instance.DisplayAdd(string.Format("{0} killed {1}", other.name, this.name));
		}
		

		yield return new WaitForSeconds(0.5f);
		this.gameObject.SetActive(false);
	}

	// LSM ����. ��� ���� ������ �̴Ͼ��� �տ����� ���� ���� �� ����.
	// �׷��Ƿ� �ش� �̴Ͼ��� ��ġ�� �޾ƿ� ���� ���͸� ���, �� ���⺤�ͷ� ���� ���� ũ��� AddForce
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

		// ù��° ������ �������� ����.
		ChangeTeamColor(bodies[0].gameObject);
	}

	// ������ ������ �� ȣ��.
	// ���� Ÿ���� null�� �ʱ�ȭ, Obstacle ��Ȱ��ȭ, Agent Ȱ��ȭ. �̶� �ణ�� ���� �����ؾ���.
	// ���� �������� ���� ���, �ڽ��� �ʺ�ŭ �����̵�.
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

	// NavMesh Agent�� Obstacle�� ���ÿ� Ű�� ����.
	// �׷��ٰ� ���� ���ְ� Ű�� �����̵� ����.
	// true��� Agent�� Ŵ.
	// false��� Obstacle�� Ŵ.
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

	// ������ �� ��ü �� ����.
    #region ChangeTeamColors
    // �����ε�. �Ű������� �������� ������� �̴Ͼ��� �������� ������ ����.
    public void ChangeTeamColor() { ChangeTeamColor(icon); }

	// ���� Ȥ�� ������ �� �̴Ͼ��� ������ ���� ������ ����.
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
		//this.gameObject.GetComponent<Renderer>().material.color = dummy_color;	//UI���� �Ӹ� �ƴ϶� Scene������ ������ ����
		// Scene �� ���� ȭ�鿡�� ������ ������ ���ϴ� ����... ���߿� ��ƼŬ�̳� ����Ʈ�� �ϴ°� ���? �̿� ���� ������ �ϴ� ����... �㳪 ������ ������ ���.
	}
    #endregion

    // �÷��̾ �ش� �̴Ͼ𿡰� ����
    public void PlayerConnect()
	{
		PlayerSelect = true;

		navenable_IE = NavEnable(false);
		StartCoroutine(navenable_IE);

		icon.SetActive(false);
		playerIcon.SetActive(true);
		
		//stats.team = MoonHeader.Team.Blue;
	}

	// �÷��̾ �ش� �̴Ͼ𿡰Լ� ����.
	// ��𿡼� ������������ �𸣴�, navmesh�� �������� �� �����ؾ���.
	public void PlayerDisConnect()
	{
		PlayerSelect = false;
		nav_ob.enabled = false;
		nav.enabled = true;

		icon.SetActive(true);
		playerIcon.SetActive(false);
		MyDestination();
	}

	// �÷��̾ �ش� �̴Ͼ��� ž���� �� �����Ͽ��� ���.
	public void PlayerSelected()
	{
		this.icon.GetComponent<Renderer>().material.color = Color.green;
	}

	// �ִϸ��̼� ���� ������ �������ִ� �Լ�
	private void AnimationSetting()
	{
		if (nav.enabled)
			anim.SetFloat("Velocity", Vector3.Magnitude(nav.velocity));
		else
			anim.SetFloat("Velocity", 0f);
	}

	// I_Actor ���� �Լ�
    #region I_Actor
    public int GetHealth(){return this.stats.actorHealth.health;}
	public int GetMaxHealth() { return this.stats.actorHealth.maxHealth; }
	public MoonHeader.Team GetTeam() { return this.stats.actorHealth.team; }
    #endregion
}
