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
	private NavMeshAgent nav;
	private NavMeshObstacle nav_ob;

	private Renderer[] bodies;  // ������ ������ ������.

	public GameObject CameraPosition;		// ī�޶� �ʱ�ȭ�� �� ����� ����.
	private GameObject icon, playerIcon;	// �̴Ͼ� �����ϴ� ������, �÷��̾� ������

	[SerializeField] protected GameObject target_attack;	// �̴Ͼ��� Ÿ��
	[SerializeField]
	protected float searchRadius, minAtkRadius, maxAtkRadius;	// �̴Ͼ��� Ž�� ����, �ּ� ���� ���� �Ÿ�, �ִ� ���� ���� �Ÿ�
	private float timer_Searching, timer_Attack;				// Ž���� ������ Ÿ�̸� ����.

	public int minionBelong;    //Spawner.cs���� �ڱⰡ �� �� ���ݷ� �Ҽ����� �޾ƿ�
	public int minionType;  //0�̸� ���Ÿ�, 1�̸� �ٰŸ� �̴Ͼ�

	// �ٽ� Ȱ��ȭ�� ��� Obstacle ��Ȱ��ȭ, Agent Ȱ��ȭ
	private void OnEnable()
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
		// �ʱ�ȭ
		timer_Searching = 0;
		timer_Attack = 0;
		target_attack = null;
		nav.stoppingDistance = 3f;
		stats = new MoonHeader.S_MinionStats();
		bodies = this.transform.GetComponentsInChildren<Renderer>();

		// ������ ���� �� ���� ����.
		icon = GameObject.Instantiate(PrefabManager.Instance.icons[0], transform);
		icon.transform.localPosition = new Vector3(0, 60, 0);
		playerIcon = GameObject.Instantiate(PrefabManager.Instance.icons[4], transform);
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
		}



	}

	// �̴Ͼ��� �⺻ ���Ȱ� �������� ���ϴ� �Լ�. Spawner.cs���� ���
	// ��� ���� �ʱ�ȭ
	public void MonSetting(GameObject[] way, MoonHeader.Team t, LSM_Spawner spawn, MoonHeader.MonType typeM)
	{
		nav_ob.enabled = false;
		nav.enabled = true;
		PlayerSelect = false;
		timer_Searching = 0;
		timer_Attack = 0;
		target_attack = null;
		way_index = 0;
		// maxhealth, speed, atk, paths, team
		// ���� �������̹Ƿ� �̸� �����ص�.
		stats.Setting(10, 100f, 3, way, t, typeM);
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
				else if (ReferenceEquals(target_attack,null) && !nav.enabled) { nav_ob.enabled = false; nav.enabled = true; }
			}
		}

		// Ÿ���� ã������, �÷��̾ �������� �ʾҰ�, ���� Agent�� Ȱ��ȭ �Ǿ����� ���
		else if (!ReferenceEquals(target_attack, null) && !PlayerSelect && nav.enabled)
		{
			// Agent�� �������� Ÿ���� ��ġ�� ����.
			nav.destination = target_attack.transform.position;
			// Ÿ���� MaxDistance�̻� �������ִٸ� Ÿ���� ������. null����.
			if (Vector3.Distance(target_attack.transform.position, this.transform.position) > maxAtkRadius)
			{ StartCoroutine(AttackFin()); }

			// ���� Ÿ�ٰ��� �Ÿ��� �ּ� ���� ���� �Ÿ����� ���ٸ� ���� �Լ� ȣ��.
			else if (Vector3.Distance(target_attack.transform.position, this.transform.position) <= minAtkRadius)
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
			if (!target_attack.activeSelf)
				StartCoroutine(AttackFin());

			else if (stats.state == MoonHeader.State.Attack && !PlayerSelect)
			{
				// ���� �ٽ� NavMeshObstacle�� ��� ������ �𸣱⿡ �ش� �κ��� �ּ�ó���� ���ܵξ���.
				//bool dummy_cant_attack = Vector3.Distance(target_attack.transform.position, this.transform.position) > minAtkRadius * (nav.isStopped ? 1f : 0.7f);
				//if (!dummy_cant_attack) { nav.isStopped = true; nav.avoidancePriority = 10; }
				//else { nav.isStopped = false; nav.avoidancePriority = 50; }

				// Ÿ�ٰ��� �Ÿ��� minAtkRadius���� ũ�ٸ� ������ �Ұ���.
				//-> Agent�� �����ִ� ���(Ÿ���� �����ϱ� ���Ͽ� �����̰� �ִ� ���)���� ���� �� �ణ�� �����ӿ� ����Ͽ� �� �� ������ �ٰ����� ����.
				bool dummy_cant_attack = Vector3.Distance(Vector3.Scale(target_attack.transform.position, Vector3.one-Vector3.up), Vector3.Scale(this.transform.position,Vector3.one-Vector3.up))
					> minAtkRadius * (nav.enabled ? 0.7f : 1f);

				// ������ �Ұ����� ���, ������ ��쿡 ���� Obstacl, Agent�� Ȱ��/��Ȱ��.
				// Agent �� Obstacle�� ���ÿ� ����Ѵٸ� ���� �߻� -> �ڽ� ���� ��ֹ��̶� �����ϸ� �ڽ��� �ִ� ���� ���Ϸ��� ���
				// �׷��⿡ Obstacle�� Agent�� ���� Ű�� ���� �ϴ� ����. �㳪 ��Ȱ��ȭ�Ѵٰ� �ٷ� ��Ȱ��ȭ������ ��������.
				// �ణ�� ���� ���� �ʴ´ٸ� ���� �浹�Ͽ� �ðܳ����� ��찡 ������. �����ϰ� �ذ��ϱ� ���Ͽ� ��ȯ�Ǵ� ���� �ӵ��� 0���� ����.
				if (dummy_cant_attack) { nav_ob.enabled = false; nav.enabled = true; }
				else { nav.enabled = false; nav_ob.enabled = true; rigid.velocity = Vector3.zero; }

				// ���� ������ �����ϴٸ�
				if (!dummy_cant_attack)
				{
					// y�� rotation���� ������ ����.
					this.transform.LookAt(target_attack.transform.position);
					this.transform.rotation = Quaternion.Euler(Vector3.Scale(this.transform.rotation.eulerAngles, Vector3.up));
					if (timer_Attack >= ATTACK_DELAY)
					{
						timer_Attack = 0;
						// ���� �ִϸ��̼� ����. ������ ���. ������ �߻�ü�� ����ҰŸ� �̶� ��ȯ.
						switch (target_attack.tag)
						{
							case "Minion":
								Attack_other<LSM_MinionCtrl>(target_attack);

								break;
							case "Turret":
								LSM_TurretSc dummy_Sc = Attack_other<LSM_TurretSc>(target_attack);
								if (dummy_Sc.stats.actorHealth.team == stats.actorHealth.team)
								{CheckingTurretTeam(target_attack.transform.parent.gameObject); StartCoroutine(AttackFin()); }

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
	private T Attack_other<T>(GameObject other) where T : I_Actor {
		T Script = other.GetComponent<T>();
		Script.Damaged(this.stats.actorHealth.Atk, this.transform.position, this.stats.actorHealth.team);
		return Script;
	}

	
	// �̴Ͼ��� �������� ���� �� ����ϴ� �Լ�.
	// dam = �̴Ͼ� Ȥ�� ��ž�� ���ݷ�. �̴Ͼ��� �޴� ������.
	// origin = ������ �ϴ� ��ü�� ��ġ. �̸� �̿��Ͽ� ���� �ڿ������� �˹��� ��������. // ���� �˹��� �����ϰ�����.
	public int Damaged(int dam, Vector3 origin, MoonHeader.Team t)
	{
		// ���� Ȥ�� ���� ������ ��� �������� ��������. �ٷ� return
		if (stats.state == MoonHeader.State.Invincibility || stats.state == MoonHeader.State.Dead)
			return stats.actorHealth.health;
		else if (t == this.stats.actorHealth.team)
			return -1;

		stats.actorHealth.health -= dam;
		StartCoroutine(DamagedEffect(origin));

		//Debug.Log("Minion Damaged!! : " +stats.health);
		// ü���� 0 ���϶�� DeadProcessing
		if (stats.actorHealth.health <= 0 && stats.state != MoonHeader.State.Dead)
		{
			StartCoroutine(DeadProcessing());
		}
		return stats.actorHealth.health;
	}

	// ü���� 0 ������ ��� ȣ��.
	// ������ Ÿ�Կ����� 0.5������ ��Ȱ��ȭ.
	private IEnumerator DeadProcessing()
	{
		stats.state = MoonHeader.State.Dead;
		if (nav.enabled)
		{nav.velocity = Vector3.zero; nav.isStopped = true; }
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
	}

	// ������ ������ �� ȣ��.
	// ���� Ÿ���� null�� �ʱ�ȭ, Obstacle ��Ȱ��ȭ, Agent Ȱ��ȭ. �̶� �ణ�� ���� �����ؾ���.
	// ���� �������� ���� ���, �ڽ��� �ʺ�ŭ �����̵�.
    protected IEnumerator AttackFin()
    {
		if (!PlayerSelect)
		{
			target_attack = null;
			
			nav_ob.enabled = false;
			yield return new WaitForSeconds(0.5f);
			this.stats.state = MoonHeader.State.Normal;
			nav.enabled = true;
			
			//nav.isStopped = false;
			timer_Searching = SEARCHTARGET_DELAY;
		}
    }

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
		this.gameObject.GetComponent<Renderer>().material.color = dummy_color;	//UI���� �Ӹ� �ƴ϶� Scene������ ������ ����
		// Scene �� ���� ȭ�鿡�� ������ ������ ���ϴ� ����... ���߿� ��ƼŬ�̳� ����Ʈ�� �ϴ°� ���? �̿� ���� ������ �ϴ� ����... �㳪 ������ ������ ���.
	}

	// �÷��̾ �ش� �̴Ͼ𿡰� ����
	public void PlayerConnect()
	{
		PlayerSelect = true;
		nav.enabled = false;
		nav_ob.enabled = true;

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
}
