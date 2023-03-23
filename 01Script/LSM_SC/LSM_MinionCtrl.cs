using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* 2023_03_20_HSH_�������� : �̴Ͼ��� �Ҽӵ� ���� ���� Scene������ Color�� ����ǵ��� ��(CHangeTeamColor).
 * �� �̴Ͼ� �ǰ� �� ��ȫ������ ���̶���Ʈ + �˹� �߰�(DamagedEffect)
 *
 */

public class LSM_MinionCtrl : MonoBehaviour
{
	public MoonHeader.MinionStats stats;
	public LSM_Spawner mySpawner;
	private bool PlayerSelect;
	[SerializeField]private int way_index;

	float MAXIMUMVELOCITY = 3f, SEARCHTARGET_DELAY = 1.5f, ATTACK_DELAY = 2f;

	private Rigidbody rigid;
	private NavMeshAgent nav;
	private NavMeshObstacle nav_ob;
	private Renderer[] bodies;

	public GameObject CameraPosition;
	public GameObject icon, playerIcon;

	[SerializeField]protected GameObject target_attack;
	[SerializeField]
	protected float searchRadius, minAtkRadius, maxAtkRadius;
	private float timer_Searching, timer_Attack;

	public int minionBelong;    //Spawner.cs���� �ڱⰡ �� �� ���ݷ� �Ҽ����� �޾ƿ�
	public int minionType;	//0�̸� ���Ÿ�, 1�̸� �ٰŸ� �̴Ͼ�


	private void OnEnable()
	{
		
		nav_ob.enabled = false;
		nav.enabled = false;
	}

	private void Awake()
	{
		PlayerSelect = false;
		rigid = this.GetComponent<Rigidbody>();
		nav = this.GetComponent<NavMeshAgent>();
		nav_ob = this.GetComponent<NavMeshObstacle>();
		timer_Searching = 0;
		timer_Attack = 0;
		target_attack = null;
		nav.stoppingDistance = 3f;
		stats = new MoonHeader.MinionStats();
		bodies = this.transform.GetComponentsInChildren<Renderer>();

        icon = GameObject.Instantiate(PrefabManager.Instance.icons[0], transform);
        icon.transform.localPosition = new Vector3(0, 60, 0);
		playerIcon = GameObject.Instantiate(PrefabManager.Instance.icons[4], transform);
		playerIcon.SetActive(false);

		// ����׿� �̸� ����. ���� Melee
		searchRadius = 14f;
		minAtkRadius = 8f;
		maxAtkRadius = 16f;
    }
	private void Start()
	{

	}
	private void LateUpdate()
	{
		if (stats.state != MoonHeader.State.Dead && !ReferenceEquals(mySpawner, null))
		{
			// ���� ������ ���� ���°� ��� �Ǵ��� Ȯ�� ��, ���¸� ����.
			if (GameManager.Instance.gameState != MoonHeader.GameState.Gaming)
			{
				if(nav.enabled)
					nav.isStopped = true;
				rigid.velocity = Vector3.zero;
				rigid.angularVelocity = Vector3.zero;
			}
			else
			{
				if (nav.enabled)
					nav.isStopped = false;
			}

			// ����׿�. ���� ��ȣ�ۿ� ������ �������� �ʾ� �̴Ͼ���� �ൿ�� �̻��Ͽ� �ִ� �ӵ��� ����.
			if (MAXIMUMVELOCITY < Vector3.Magnitude(rigid.velocity))
			{
				//rigid.velocity = rigid.velocity.normalized * MAXIMUMVELOCITY;
				rigid.velocity = Vector3.zero;
			}

			SearchingTarget();
			Attack();
			MyDestination();
		}

		

	}
	
	// �̴Ͼ��� �⺻ ���Ȱ� �������� ���ϴ� �Լ�. Spawner.cs���� ���
	// ��� ���� �ʱ�ȭ
	public void MonSetting(GameObject[] way, MoonHeader.Team t, LSM_Spawner spawn)
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
        stats.Setting(10,50f,3, way, t, MoonHeader.MonType.Melee);
		//stats = new MoonHeader.MinionStats(10, 50f, 10, way, t);

		transform.LookAt(stats.destination[way_index].transform);
		nav.destination = stats.destination[way_index].transform.position;
		nav.avoidancePriority = 50;
		nav.isStopped = false;

		mySpawner = spawn;
		icon.SetActive(true);
		playerIcon.SetActive(false);
		rigid.angularVelocity = Vector3.zero;
		rigid.velocity = Vector3.zero;

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


    // �̴Ͼ��� ���� ��� �Ѿ�� ���� ������ �Լ�
    public void MyDestination()
	{
		if (ReferenceEquals(target_attack, null) && nav.enabled)
		{
			nav.destination = stats.destination[way_index].transform.position;
		}
	}

	private void CheckingTurretTeam(GameObject obj)
	{
        if (stats.destination[way_index].Equals(obj))
        {
            LSM_TurretSc dummySc = obj.transform.GetComponentInChildren<LSM_TurretSc>();
            if (dummySc.stats.Health <= 0 || dummySc.stats.team == this.stats.team)
            {
                way_index++;
            }
        }
    }

	// �̴Ͼ��� �ֺ��� Ž���ϴ� �Լ�.
	private void SearchingTarget()
	{
		// ���� �̴Ͼ��� Ÿ���� Ȯ�� �Ͽ�����.
		if (ReferenceEquals(target_attack, null) && !PlayerSelect)
		{

			timer_Searching += Time.deltaTime;
			if (timer_Searching >= SEARCHTARGET_DELAY)
			{
				timer_Searching = 0;

				// ���Ǿ�ĳ��Ʈ�� ����Ͽ� ���� ������ ���� ���� �ִ��� Ȯ��.
				RaycastHit[] hits;
				hits = Physics.SphereCastAll(transform.position, searchRadius, Vector3.up, 0);
				float dummyDistance = float.MaxValue;
				foreach (RaycastHit hit in hits)
				{
					float hit_dummy_distance = Vector3.Distance(transform.position, hit.transform.position);
					if (dummyDistance > hit_dummy_distance)
					{
						bool different_Team = false;
						if (hit.transform.CompareTag("Minion"))	
						{ different_Team = (stats.team != hit.transform.GetComponent<LSM_MinionCtrl>().stats.team); }	//�ڽŰ� ���� ���ݷ��� �̴Ͼ� ������� ����
						else if (hit.transform.CompareTag("Turret"))
						{different_Team = (stats.team != hit.transform.GetComponent<LSM_TurretSc>().stats.team) 
								&& hit.transform.GetComponent<LSM_TurretSc>().TurretBelong == minionBelong;}	//�ڽŰ� ���� ���ݷ��� �ͷ��� ������� ����

						if (different_Team)
						{
							dummyDistance = hit_dummy_distance;
							target_attack = hit.transform.gameObject;
							if (nav.enabled)
								nav.destination = target_attack.transform.position;
						}

					}
				}
			}
		}

		if (!ReferenceEquals(target_attack, null) && !PlayerSelect && nav.enabled)
		{
			
			nav.destination = target_attack.transform.position;
			// Ÿ���� MaxDistance�̻� �������ִٸ� null
			if (Vector3.Distance(target_attack.transform.position, this.transform.position) > maxAtkRadius)
			{ StartCoroutine(AttackFin()); }

			else if (Vector3.Distance(target_attack.transform.position, this.transform.position) <= minAtkRadius)
			{
				stats.state = MoonHeader.State.Attack;
			}

		}

	}

	// �̴Ͼ��� ���������, Ÿ���� ã�Ҵٸ� �ش� �Լ��� ����.
	// ���� �����Ҷ� NavAgent�� ��Ȱ��ȭ, NavObstacle�� Ȱ��ȭ �Ͽ�����, �̸� �����ϸ� NavMesh�� ���� ��ã�⸦ �ǽð����� �ٽ� �ݺ��ϴ� ����������.
	// �׷��Ƿ� NavAgent�� Priority�� �ϰ���Ű�� ������ �ش� �̴Ͼ��� ��ġ�� �ʰ� ����.
	private void Attack()
	{
		if (timer_Attack <= ATTACK_DELAY) { timer_Attack += Time.deltaTime; }

		if (!ReferenceEquals(target_attack, null))
		{
			if (!target_attack.activeSelf)
				StartCoroutine(AttackFin());

			else if (stats.state == MoonHeader.State.Attack && !PlayerSelect)
			{
				// ���� Ÿ���� ��ġ�� ���� ���� �������� �ָ� �ִٸ�, navmesh�� Ȱ��ȭ, navObstacle�� ��Ȱ��ȭ
				bool dummy_cant_attack = Vector3.Distance(target_attack.transform.position, this.transform.position) > minAtkRadius * (nav.isStopped ? 1f : 0.7f);

				//if (dummy_cant_attack) { nav_ob.enabled = false; nav.enabled = true; }
				//else { nav.enabled = false; nav_ob.enabled = true; }
				if (!dummy_cant_attack) { nav.isStopped = true; nav.avoidancePriority = 30; }
				else { nav.isStopped = false; nav.avoidancePriority = 50; }


				if (!dummy_cant_attack)
				{
					this.transform.LookAt(target_attack.transform.position);
					this.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
					if (timer_Attack >= ATTACK_DELAY)
					{
						timer_Attack = 0;
						// ���� �ִϸ��̼� ����. ������ ���. ������ �߻�ü�� ����ҰŸ� �̶� ��ȯ.
						switch (target_attack.tag)
						{
							case "Minion":
								LSM_MinionCtrl dummy_ctrl = target_attack.GetComponent<LSM_MinionCtrl>();
								dummy_ctrl.Damaged(this.stats.Atk);

								break;
							case "Turret":
								LSM_TurretSc dummy_Sc = target_attack.GetComponent<LSM_TurretSc>();
								if (dummy_Sc.stats.team != stats.team)
									dummy_Sc.Damaged(this.stats.Atk, this.stats.team);
								else
								{
									CheckingTurretTeam(target_attack.transform.parent.gameObject); StartCoroutine(AttackFin());
								}

								break;
						}

					}
				}
			}
		}

	}

	

	public int Damaged(int dam)
	{
		if (stats.state == MoonHeader.State.Invincibility || stats.state == MoonHeader.State.Dead)
			return stats.health;

		stats.health -= dam;
		StartCoroutine(DamagedEffect());

		//Debug.Log("Minion Damaged!! : " +stats.health);
		if (stats.health <= 0 && stats.state != MoonHeader.State.Dead)
		{
			StartCoroutine(DeadProcessing());
		}
		return stats.health;
	}

	private IEnumerator DeadProcessing()
	{
		stats.state = MoonHeader.State.Dead;
		if(nav.enabled)
			nav.isStopped = true;
		yield return new WaitForSeconds(0.5f);
		this.gameObject.SetActive(false);
	}

	/// <summary>
	/// //////////
	/// </summary>
	/// <returns></returns>
	private IEnumerator DamagedEffect()
	{
		Color damaged = new Color(255/255f, 150/255f, 150/255f);
		Color recovered = Color.white;
        Vector3 knockbackDirection = new Vector3(0f, 1000f, -1000f);

        transform.Find("Cylinder").gameObject.GetComponent<Renderer>().material.color = damaged;
        transform.Find("Head").gameObject.GetComponent<Renderer>().material.color = damaged;
        this.GetComponent<Rigidbody>().AddRelativeForce(knockbackDirection * 300 * Time.deltaTime);


        yield return new WaitForSeconds(0.25f);

        transform.Find("Cylinder").gameObject.GetComponent<Renderer>().material.color = recovered;
        transform.Find("Head").gameObject.GetComponent<Renderer>().material.color = recovered;
    }

    protected IEnumerator AttackFin()
    {
		if (!PlayerSelect)
		{
			target_attack = null;
			this.stats.state = MoonHeader.State.Normal;
			nav_ob.enabled = false;
			yield return new WaitForSeconds(0.5f);
			nav.enabled = true;
		}
    }
	public void ChangeTeamColor() { ChangeTeamColor(icon); }

    public void ChangeTeamColor(GameObject obj)
	{
		Color dummy_color;
		switch (stats.team)
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
}
