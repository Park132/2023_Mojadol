using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSM_TurretSc : MonoBehaviour
{

	private float ATTACKDELAY = 3f, SEARCHINGDELAY = 1f;

    public MoonHeader.TurretStats stats;
	private GameObject mark;
	private float timer, timer_attack;
	private float searchRadius;
	[SerializeField]private GameObject target;

	private void Start()
	{
		mark = GameObject.Instantiate(PrefabManager.Instance.icons[3], transform);
		mark.transform.localPosition = Vector3.up * 10;
		stats = new MoonHeader.TurretStats(10,4);
		Color dummy_c = Color.white;
		switch (stats.team) {
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

		timer = 0;
		searchRadius = 10f;
		target = null;
	}

	private void Update()
	{
		if (GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
		{
			SearchingTarget();
			AttackTarget();
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


				foreach (RaycastHit hit in hits)
				{
					float minDistance = float.MaxValue;
					if (hit.transform.CompareTag("Minion"))
					{
						LSM_MinionCtrl dummyCtr = hit.transform.GetComponent<LSM_MinionCtrl>();
						float dummydistance = Vector3.Distance(transform.position, hit.transform.position);
						if (dummyCtr.stats.team != stats.team && minDistance > dummydistance)
						{
							target = hit.transform.gameObject;
							minDistance = dummydistance;
						}
					}
				}

				if (!ReferenceEquals(target, null)) Debug.Log("Minion Searching!!");
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
					Debug.Log("Attack Minion!");
					timer_attack = 0;
					LSM_MinionCtrl dummyMinion = target.GetComponent<LSM_MinionCtrl>();
					dummyMinion.Damaged(stats.Atk);
					if (dummyMinion.stats.health <= 0)
						target = null;
				}
			}
		}
	}

}
