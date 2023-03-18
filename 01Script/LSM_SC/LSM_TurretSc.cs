using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSM_TurretSc : MonoBehaviour
{

	private float ATTACKDELAY = 3f, SEARCHINGDELAY = 0.5f;

    public MoonHeader.TurretStats stats;
	private GameObject mark;
	private float timer, timer_attack;
	private float searchRadius;
	[SerializeField]private GameObject target;

	private void Start()
	{
		mark = GameObject.Instantiate(PrefabManager.Instance.icons[3], transform);
		mark.transform.localPosition = Vector3.up * 10;
		// health, atk
		stats = new MoonHeader.TurretStats(10,6);
		ChangeColor();

		timer = 0;
		searchRadius = 10f;
		target = null;
	}

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
    }

	private void Update()
	{
		if (GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
		{
			SearchingTarget();
			AttackTarget();
		}

	}

	public void Damaged(int dam, MoonHeader.Team t)
	{
		this.stats.Health -= dam;
		if (this.stats.Health <= 0) {
			this.stats.team = t;
			this.stats.Health = 10;
			ChangeColor();
		}
	}

	// 일정 범위 내에 적이 있는지를 확인하는 코드.
	private void SearchingTarget()
	{
		if (ReferenceEquals(target, null)){
			timer += Time.deltaTime;
			if (timer >= SEARCHINGDELAY && ReferenceEquals(target, null))
			{
				timer = 0;
				RaycastHit[] hits = Physics.SphereCastAll(transform.position, searchRadius, Vector3.up, 0, 1 << LayerMask.NameToLayer("Minion"));

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
						dummyMinion.Damaged(stats.Atk);
						if (dummyMinion.stats.health <= 0)
							target = null;
					}
					else { target = null; }
				}
			}
		}
	}

}
