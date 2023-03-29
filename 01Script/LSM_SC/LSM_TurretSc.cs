using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/* 2023_03_20_HSH_수정사항 : 터렛이 팀에 종속 시 Scene에서도 Color가 변경되도록 함.
 * ㄴ 터렛 피격 시 분홍색으로 하이라이트(DamagedEffect())
 */

// 포탑에 대하여 간단하게 구현
public class LSM_TurretSc : MonoBehaviour, I_Actor
{
	// 포탑의 탐색, 공격에 대한 딜레이 상수화 혹시 모를 변경에 대비하여 const는 생략
	private float ATTACKDELAY = 3f, SEARCHINGDELAY = 0.5f;

    public MoonHeader.S_TurretStats stats;			// 터렛의 상태에 대한 구조체
	private GameObject mark;						// TopView에서 플레이어에게 보여질 아이콘
	private float timer, timer_attack;				// 탐색, 공격에 사용될 타이머
	private float searchRadius;						// 탐색 범위
	[SerializeField]private GameObject target;		// 공격 타겟

	public int TurretBelong;						// 터렛의 위치

	private void Start()
	{
		// 초기화
		mark = GameObject.Instantiate(PrefabManager.Instance.icons[3], transform);
		mark.transform.localPosition = Vector3.up * 10;
		// health, atk
		
		// 디버그용으로 미리 설정.
		stats = new MoonHeader.S_TurretStats(10,6);
		ChangeColor();

		timer = 0;
		searchRadius = 10f;
		target = null;

    }

	// 팀에 해당하는 색으로 변경.
	private void ChangeColor()
	{
        Color dummy_c = Color.white;

        switch (stats.team)
        {
            case MoonHeader.Team.Red:
                dummy_c = Color.red;
                break;
            case MoonHeader.Team.Blue:
                dummy_c = Color.blue;
                break;
            default:
                dummy_c = Color.yellow;
                break;
        }
        mark.GetComponent<Renderer>().material.color = dummy_c;
		transform.Find("Sphere").gameObject.GetComponent<Renderer>().material.color = dummy_c;	//소속 변경 시 UI에서 뿐만 아니라 Scene에서도 색상 변경
    }

	private void Update()
	{
		// 게임 중일때만 실행되도록 설정
		if (GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
		{
			SearchingTarget();
			AttackTarget();
		}

	}

	// I_Actor 인터페이스에 포함되어잇는 함수.
	// 공격을 받을 시 데미지를 입음.
	public int Damaged(int dam, Vector3 origin, MoonHeader.Team t)
	{
		if (t == this.stats.team)
			return this.stats.Health;
		this.stats.Health -= dam;
		StartCoroutine(DamagedEffect());

		if (this.stats.Health <= 0) {
			this.stats.team = t;
			this.stats.Health = 10;
			ChangeColor();
		}
		return this.stats.Health;
	}

	// 데미지를 입을 경우 색상 변경.
    private IEnumerator DamagedEffect()
    {
        Color damaged = new Color(255 / 255f, 150 / 255f, 150 / 255f);
        Color recovered = Color.white;
        
        transform.gameObject.GetComponent<Renderer>().material.color = damaged;

        yield return new WaitForSeconds(0.25f);

        transform.gameObject.GetComponent<Renderer>().material.color = recovered;
    }

    // 일정 범위 내에 적이 있는지를 확인하는 코드.
    private void SearchingTarget()
	{
		// 타겟이 존재하지 않을 때만 탐색.
		if (ReferenceEquals(target, null)){
			timer += Time.deltaTime;
			if (timer >= SEARCHINGDELAY && ReferenceEquals(target, null))
			{
				timer = 0;
				// 구형 캐스트를 사용하여 탐지.
				RaycastHit[] hits = Physics.SphereCastAll(transform.position, searchRadius, Vector3.up, 0, 1 << LayerMask.NameToLayer("Minion"));

				// 가장 가까운 미니언을 찾는 함수.
                float minDistance = float.MaxValue;
                foreach (RaycastHit hit in hits)
				{
					if (hit.transform.CompareTag("Minion"))
					{
						LSM_MinionCtrl dummyCtr = hit.transform.GetComponent<LSM_MinionCtrl>();
						if (dummyCtr.stats.team != this.stats.team)
						{
							float dummydistance = Vector3.Distance(transform.position, hit.transform.position);
							if (dummyCtr.stats.team != stats.team && minDistance > dummydistance)
							{
								target = hit.transform.gameObject;
								minDistance = dummydistance;
							}
						}
					}
				}

				//if (!ReferenceEquals(target, null)) Debug.Log("Minion Searching!!");
			}
		}
		else timer = 0;
	}

	// 공격 함수.
	// 현재 미니언만 공격이 가능하도록 설정되어있음. 후에 플레이어블 미니언 또한 가능하도록 설정할것임.
	private void AttackTarget()
	{
		if (timer_attack < ATTACKDELAY) timer_attack += Time.deltaTime;
		if (!ReferenceEquals(target, null))
		{
			if (!target.activeSelf)
			{ target = null;}
			else
			{
				if (timer_attack >= ATTACKDELAY)
				{
					//Debug.Log("Attack Minion!");
					timer_attack = 0;
					LSM_MinionCtrl dummyMinion = target.GetComponent<LSM_MinionCtrl>();
					if (dummyMinion.stats.team != this.stats.team)
					{
						dummyMinion.Damaged(stats.Atk, transform.position, this.stats.team);
						if (dummyMinion.stats.health <= 0)
							target = null;
					}
					else { target = null; }
				}
			}
		}
	}

}
