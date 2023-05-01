using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 넥서스가 갖고있는 스크립트. 터렛 스크립트를 상속받게 설정.
public class LSM_NexusSC : LSM_TurretSc
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

	public override void Damaged(short dam, Vector3 origin, MoonHeader.Team t, GameObject other)
	{
		if (t == this.stats.actorHealth.team || !PhotonNetwork.IsMasterClient)
			return;
		this.stats.actorHealth.health -= dam;
		StartCoroutine(DamagedEffect());
		if (this.stats.actorHealth.health <= 0)
		{
			GameManager.Instance.GameEndingProcess(this.stats.actorHealth.team);
		}

		return;
	}
	protected override void DestroyProcessing(GameObject other)
	{
		GameManager.Instance.DisplayAdd(string.Format("{0} Destroyed {1}", other.name, this.name));
	}
}
