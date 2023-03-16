using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LSM_MinionCtrl : MonoBehaviour
{
	public MoonHeader.MinionStats stats;
	public LSM_Spawner mySpawner;
	private bool PlayerSelect;
	private int way_index;

	float MAXIMUMVELOCITY = 3f, SEARCHTARGET_DELAY = 1.5f, ATTACK_DELAY = 2f;

	private Rigidbody rigid;
	private NavMeshAgent nav;
	private NavMeshObstacle nav_ob;

	public GameObject CameraPosition;
	public GameObject icon;

	[SerializeField]protected GameObject target_attack;
	[SerializeField]
	protected float searchRadius, minAtkRadius, maxAtkRadius;
	private float timer_Searching, timer_Attack;
	


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

        icon = GameObject.Instantiate(PrefabManager.Instance.icons[0], transform);
        icon.transform.localPosition = new Vector3(0, 60, 0);
		searchRadius = 17f;
		minAtkRadius = 14f;
		maxAtkRadius = 18f;
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
	
	// �̴Ͼ��� �⺻ ���Ȱ� �������� ���ϴ� �Լ�.
	public void MonSetting(GameObject[] way, MoonHeader.Team t, LSM_Spawner spawn)
	{
		nav_ob.enabled = false;
		nav.enabled = true;
		PlayerSelect = false;
		timer_Searching = 0;
		timer_Attack = 0;
		target_attack = null;

		// maxhealth, speed, atk, paths, team
		stats.Setting(10,50f,10, way, t);
		//stats = new MoonHeader.MinionStats(10, 50f, 10, way, t);
		way_index = 0;
		transform.LookAt(stats.destination[way_index].transform);
		nav.destination = stats.destination[way_index].transform.position;
		mySpawner= spawn;
		CHangeTeamColor();
		
	}

    private void OnTriggerEnter(Collider other)
    {
		if (other.CompareTag("WayPoint"))
		{
			if (stats.destination[way_index].Equals(other.transform.gameObject))
			{
				if (other.transform.GetComponentInChildren<LSM_TurretSc>().stats.Health <= 0)
				{
					way_index++;
				}
			}
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
				foreach (RaycastHit hit in hits)
				{
					if (hit.transform.CompareTag("Minion"))
					{
						if (stats.team != hit.transform.GetComponent<LSM_MinionCtrl>().stats.team)
						{
							target_attack = hit.transform.gameObject;
							nav.destination = target_attack.transform.position;
							//Debug.Log("Search Minion! : " + target_attack.name + " " + target_attack.GetComponent<LSM_MinionCtrl>().stats.team);
							break;
						}
					}
					else if (hit.transform.CompareTag("Turret"))
					{
						if (stats.team != hit.transform.GetComponent<LSM_TurretSc>().stats.team)
						{
							target_attack = hit.transform.gameObject;
							nav.destination = target_attack.transform.position;//////

							break;
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
			{target_attack = null;}

			else if (Vector3.Distance(target_attack.transform.position, this.transform.position) <= minAtkRadius)
			{
				stats.state = MoonHeader.State.Attack;
			}

			//Debug.Log("searchingSuccess " + target_attack.name);
		}
	}

	private void Attack()
	{
		if (timer_Attack <= ATTACK_DELAY) timer_Attack += Time.deltaTime;

		if (!ReferenceEquals(target_attack, null))
		{
			if (!target_attack.activeSelf)
				target_attack = null;
			else if (stats.state == MoonHeader.State.Attack )
			{
				// ���� Ÿ���� ��ġ�� ���� ���� �������� �ָ� �ִٸ�, navmesh�� Ȱ��ȭ, navObstacle�� ��Ȱ��ȭ
				bool dummy_cant_attack = Vector3.Distance(target_attack.transform.position, this.transform.position) > minAtkRadius * (nav.enabled? 0.7f : 1f);

				// ���� �̴Ͼ��� �̵� ��ΰ� �̻���... �����ϰ��ִ� �̴Ͼ��� �о���ʰ� ���ذ����� ���ڴµ�...
				if (dummy_cant_attack) { nav_ob.enabled = false; nav.enabled = true;}
				else { nav.enabled = false; nav_ob.enabled = true; }
				//nav.avoidancePriority = (!dummy_cant_attack ? 30 : 50);
				//nav.isStopped = !dummy_cant_attack;
				

				if (!dummy_cant_attack)
				{
					this.transform.LookAt(target_attack.transform.position);
					this.transform.rotation = Quaternion.Euler(0,this.transform.rotation.eulerAngles.y,0);
					if (timer_Attack >= ATTACK_DELAY)
					{
						timer_Attack = 0;
						// ���� �ִϸ��̼� ����. ������ ���. ������ �߻�ü�� ����ҰŸ� �̶� ��ȯ.
						switch (target_attack.tag)
						{
							case "Minion":
								LSM_MinionCtrl dummy_ctrl = target_attack.GetComponent<LSM_MinionCtrl>();
								dummy_ctrl.Damaged(this.stats.Atk);
								Debug.Log("Minion Attack!! : Minion");
								break;
							case "Turret":
								LSM_TurretSc dummy_Sc = target_attack.GetComponent<LSM_TurretSc>();
								dummy_Sc.stats.Health -= this.stats.Atk;
								Debug.Log("Minion Attack! : Turret");
								break;
						}
					}
				}
			}
		}
	}

	public int Damaged(int dam)
	{
		stats.health -= dam;
		Debug.Log("Minion Damaged!! : " +stats.health);
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

	public void CHangeTeamColor()
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
		icon.GetComponent<Renderer>().material.color = dummy_color;
	}

	// �÷��̾ �ش� �̴Ͼ𿡰� ����
	public void PlayerConnect()
	{
		PlayerSelect = true;
		nav.enabled = false;
		nav_ob.enabled = true;
		
		//stats.team = MoonHeader.Team.Blue;
	}

	// �÷��̾ �ش� �̴Ͼ𿡰Լ� ����.
	// ��𿡼� ������������ �𸣴�, navmesh�� �������� �� �����ؾ���.
	public void PlayerDisConnect()
	{
		PlayerSelect = false;
		nav_ob.enabled = false;
		nav.enabled = true;
		
	}
}
