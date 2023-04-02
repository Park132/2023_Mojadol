using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ؼ����� �����ִ� ��ũ��Ʈ. �ͷ� ��ũ��Ʈ�� ��ӹް� ����.
public class LSM_NexusSC : LSM_TurretSc, I_Actor
{
	private LSM_Spawner parentSpawner;

	protected override void Start()
	{
		parentSpawner = this.GetComponentInParent<LSM_Spawner>();
		base.Start();
		stats = new MoonHeader.S_TurretStats(100, 10, parentSpawner.team);
		base.ChangeColor();
		ATTACKDELAY = 1.5f;
	}

	public override int Damaged(int dam, Vector3 origin, MoonHeader.Team t)
	{
		if (t == this.stats.actorHealth.team)
			return this.stats.actorHealth.health;
		this.stats.actorHealth.health -= dam;
		StartCoroutine(DamagedEffect());
		return this.stats.actorHealth.health;
	}

	public override int GetHealth() { return this.stats.actorHealth.health; }
	public override int GetMaxHealth() { return this.stats.actorHealth.maxHealth; }
	public override MoonHeader.Team GetTeam() { return this.stats.actorHealth.team; }
}
