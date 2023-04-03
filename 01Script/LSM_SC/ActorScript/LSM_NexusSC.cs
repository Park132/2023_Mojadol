using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ؼ����� �����ִ� ��ũ��Ʈ. �ͷ� ��ũ��Ʈ�� ��ӹް� ����.
public class LSM_NexusSC : LSM_TurretSc
{
	private LSM_Spawner parentSpawner;

	protected override void Start()
	{
		parentSpawner = this.GetComponentInParent<LSM_Spawner>();
		base.Start();
		stats = new MoonHeader.S_TurretStats(100, 10, parentSpawner.team);
		base.ChangeColor();
		base.ChangeColor(bodies[0].gameObject);
		ATTACKDELAY = 1.5f;

	}

	public override int Damaged(int dam, Vector3 origin, MoonHeader.Team t, GameObject other)
	{
		if (t == this.stats.actorHealth.team)
			return this.stats.actorHealth.health;
		this.stats.actorHealth.health -= dam;
		StartCoroutine(DamagedEffect());
		if (this.stats.actorHealth.health <= 0)
		{
			GameManager.Instance.GameEndingProcess(this.stats.actorHealth.team);
		}

		return this.stats.actorHealth.health;
	}
	protected override void DestroyProcessing(GameObject other)
	{
		GameManager.Instance.DisplayAdd(string.Format("{0} Destroyed {1}", other.name, this.name));
	}
}