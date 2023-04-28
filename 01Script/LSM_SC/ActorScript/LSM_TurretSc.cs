using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

/* 2023_03_20_HSH_수정사항 : 터렛이 팀에 종속 시 Scene에서도 Color가 변경되도록 함.
 * ㄴ 터렛 피격 시 분홍색으로 하이라이트(DamagedEffect())
 */

// 포탑에 대하여 간단하게 구현
public class LSM_TurretSc : MonoBehaviourPunCallbacks, I_Actor, IPunObservable
{
	// 포탑의 탐색, 공격에 대한 딜레이 상수화 혹시 모를 변경에 대비하여 const는 생략
	protected float ATTACKDELAY = 3f, SEARCHINGDELAY = 0.5f;

    public MoonHeader.S_TurretStats stats;			// 터렛의 상태에 대한 구조체
	protected GameObject mark;						// TopView에서 플레이어에게 보여질 아이콘
	protected float timer, timer_attack;				// 탐색, 공격에 사용될 타이머
	protected float searchRadius;						// 탐색 범위
	[SerializeField]protected GameObject target;        // 공격 타겟
	protected I_Actor target_Actor;

	protected Renderer[] bodies;  // 색상을 변경할 렌더러.

	public int TurretBelong;                        // 터렛의 위치
													// # 터렛의 경로에 따라서 번호를 다르게 설정. 해당 경로와 동일하게 숫자가 같도록 설정.
	//protected PhotonView photonView;

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
		if(stream.IsWriting)
        {
			stream.SendNext(stats.actorHealth.maxHealth);
			stream.SendNext(stats.actorHealth.health);
			stream.SendNext(stats.actorHealth.team);
			stream.SendNext(stats.actorHealth.Atk);

		}
		else
        {
			this.stats.actorHealth.maxHealth = (int)stream.ReceiveNext();
			this.stats.actorHealth.health = (int)stream.ReceiveNext();
			this.stats.actorHealth.team = (MoonHeader.Team)stream.ReceiveNext();
			this.stats.actorHealth.Atk = (int)stream.ReceiveNext();
		}
    }

	protected virtual void Start()
	{
		// 초기화
		bodies = this.transform.GetComponentsInChildren<Renderer>();
		mark = GameObject.Instantiate(PrefabManager.Instance.icons[3], transform);
		mark.transform.localPosition = Vector3.up * 10;
		
		// health, atk
		
		// 디버그용으로 미리 설정.
		stats = new MoonHeader.S_TurretStats(100,6);
		ChangeColor();

		timer = 0;
		searchRadius = 10f;
		target = null;

    }

	// 팀에 해당하는 색으로 변경.

	protected void ChangeColor()
	{
		Color dummy_c = Color.white;

		switch (stats.actorHealth.team)
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
		bodies[0].GetComponent<Renderer>().material.color = dummy_c;
	}

	protected void Update()
	{
		if (!PhotonNetwork.IsMasterClient)
			return;
		// 게임 중일때만 실행되도록 설정
		if (GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
		{
			SearchingTarget();
			AttackTarget();
		}

	}

	// I_Actor 인터페이스에 포함되어잇는 함수.
	// 공격을 받을 시 데미지를 입음.
	public virtual void Damaged(int dam, Vector3 origin, MoonHeader.Team t, GameObject other)
	{
		if (!PhotonNetwork.IsMasterClient || t== this.stats.actorHealth.team)
			return;

		this.stats.actorHealth.health -= dam;
		photonView.RPC("Damaged_RPC_Turret", RpcTarget.All);

		if (this.stats.actorHealth.health <= 0) {
			this.stats.actorHealth.team = t;
			this.stats.actorHealth.health = 10;
			DestroyProcessing(other);
			photonView.RPC("DestroyProcessing_RPC", RpcTarget.All);
		}
		return;
		
	}

	// 체력은 동기화되게 설정하였기에, 외적인 면만 보여주면 될듯..
	[PunRPC]protected void Damaged_RPC_Turret()
    {
        StartCoroutine(DamagedEffect());
	}

	[PunRPC] protected void DestroyProcessing_RPC() {
		ChangeColor();
	}


	// 데미지를 입을 경우 색상 변경.
    protected IEnumerator DamagedEffect()
    {
		Color damagedColor = new Color32(255, 150, 150, 255);
		Color recovered = Color.white;

		foreach (Renderer item in bodies)
			item.material.color = damagedColor;

        yield return new WaitForSeconds(0.25f);

		foreach (Renderer item in bodies)
			item.material.color = recovered;
		ChangeColor();
	}

	protected virtual void DestroyProcessing(GameObject other)
	{
		GameManager.Instance.DisplayAdd(string.Format("{0} Destroyed {1}", other.name, this.name));
	}

    // 일정 범위 내에 적이 있는지를 확인하는 코드.
    protected void SearchingTarget()
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
					MoonHeader.S_ActorState dummy_actor = new MoonHeader.S_ActorState();
					bool dummy_bool = false;
					if (hit.transform.CompareTag("Minion"))
					{
						LSM_MinionCtrl dummyCtr = hit.transform.GetComponent<LSM_MinionCtrl>();
						dummy_actor = dummyCtr.stats.actorHealth;
						dummy_bool = true;
					}
					else if (hit.transform.CompareTag("PlayerMinion"))
					{
						Debug.Log("Player Find!");
						PSH_PlayerFPSCtrl dummyCtr = hit.transform.GetComponent<PSH_PlayerFPSCtrl>();
						dummy_actor = dummyCtr.actorHealth;
						dummy_bool = true;
					}


					// 혹시 모를 오류를 방지하기 위하여 논리 변수를 확인.
					if (dummy_bool && dummy_actor.team != this.stats.actorHealth.team)
					{
						float dummydistance = Vector3.Distance(transform.position, hit.transform.position);
						// 가장 거리가 적은 타겟을 찾는 구문
						if (minDistance > dummydistance)
						{
							target = hit.transform.gameObject;
							minDistance = dummydistance;
						}
					}
				}

				if (!ReferenceEquals(target, null))
				{target_Actor = target.GetComponent<I_Actor>();}


				//if (!ReferenceEquals(target, null)) Debug.Log("Minion Searching!!");
			}
		}
		else timer = 0;
	}

	// 공격 함수.
	// 현재 미니언만 공격이 가능하도록 설정되어있음. 후에 플레이어블 미니언 또한 가능하도록 설정할것임.
	protected void AttackTarget()
	{
		if (timer_attack < ATTACKDELAY) timer_attack += Time.deltaTime;
		if (!ReferenceEquals(target, null))
		{
			if (!target.activeSelf || target_Actor.GetTeam() == this.GetTeam())
			{ target = null;}
			else
			{
				if (timer_attack >= ATTACKDELAY)
				{
					//Debug.Log("Attack Minion!");
					timer_attack = 0;

					RaycastHit[] hits=Physics.RaycastAll(this.transform.position, (target.transform.position - this.transform.position).normalized, searchRadius, 1 << LayerMask.NameToLayer("Minion"));

					foreach(RaycastHit hit in hits)
					{
						if (target == hit.transform.gameObject)
						{
							GameObject dummy = PoolManager.Instance.Get_Particles(0, hit.point);
							dummy.transform.position= hit.point;
							break;
						}
					}

					// 팀에 따라 제너릭 함수를 따로 사용.
					switch (target.tag)
					{
						case "Minion":
							Attack_Actor<LSM_MinionCtrl>(target);
							break;
						case "PlayerMinion":
							Debug.Log("PlayerMinion Attack!");
							Attack_Actor<PSH_PlayerFPSCtrl>(target);
							break;
						default:
							break;
					}

				}
			}
		}
	}

	// Generic함수와 Interface를 결합해서 사용.
	// 미니언 및 플레이어의 오브젝트를 받아와서 사용함.
	private void Attack_Actor<T>(GameObject obj) where T : I_Actor
	{
		T script = obj.GetComponent<T>();
		script.Damaged(this.stats.actorHealth.Atk, transform.position, this.stats.actorHealth.team, this.gameObject);
		int remain_health = script.GetHealth();
		if (remain_health <= 0)
			target = null;
	}

	public virtual int GetHealth() { return this.stats.actorHealth.health; }
	public virtual int GetMaxHealth() { return this.stats.actorHealth.maxHealth; }
	public virtual MoonHeader.Team GetTeam() { return this.stats.actorHealth.team; }
}
