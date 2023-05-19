using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//흑마법사 크립의 설정 및 제어 스크립트

public enum LichStat
{
    Idle,
    Idle_Combat,
    Attack,
    Death
}

public class HSH_LichCreepController : MonoBehaviour
{
    CreepInfo lichinfo; //크립 관련 정보
    LichStat lichstat;

    public bool doOnlyOnce; //coroutine을 한 번만 실행
    Transform initTrans;

    public GameObject triggerBox;
    public GameObject spellFieldGenerator, fireBallThrower;  //투사체, 장판 패턴을 담당하는 Agent들
    public List<GameObject> Player;

    Animator anim;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        lichinfo = new CreepInfo();
        lichinfo.hp = 100f;
        lichinfo.isHero = false;
        lichstat = LichStat.Idle;

        doOnlyOnce = true;
        initTrans = this.transform;

        Player = new List<GameObject>();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        //플레이어가 없으면 모든 패턴 비활성화
        fireBallThrower.SetActive(false);
        spellFieldGenerator.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        AnimCtrl();
        lichinfo.isHero = triggerBox.GetComponent<HSH_TriggerBox>().isTherePlayer;  //트리거 박스로부터 플레이어 존재 여부를 받아옴

        //크립룸 안에 플레이어가 있는가?
        if (lichinfo.isHero)    //네
        {
            lichstat = LichStat.Idle_Combat;
            LookAtMostCloseOne();

            //패턴 활성화
            fireBallThrower.SetActive(true);
            spellFieldGenerator.SetActive(true);

            if(!fireBallThrower.GetComponent<HSH_FireBallThrower>().pinfo.isCool && doOnlyOnce)
            {
                doOnlyOnce = false;
                StartCoroutine(DelayedAttack());
            }
        }
        else    //아니요
        {
            lichstat = LichStat.Idle;

            //모든 패턴 비활성화
            fireBallThrower.SetActive(false);
            spellFieldGenerator.SetActive(false);
        }

        if(lichinfo.hp <= 0)
        {
            lichstat = LichStat.Death;
        }
    }

    public void AnimCtrl()
    {
        switch (lichstat)
        {
            case LichStat.Idle:
                anim.SetTrigger("Idle");
                break;
            case LichStat.Idle_Combat:
                anim.SetTrigger("Idle_Combat");
                break;
            case LichStat.Attack:
                anim.SetTrigger("Attack");
                break;
            case LichStat.Death:
                anim.SetTrigger("Death");   //사망 조건이 충족했을 때 lichstat = LichStat.Death를 넣어주세요.
                break;
        }
    }

    void LookAtMostCloseOne()
    {
        Vector3 mostClose = Vector3.zero;
        float distance = 100000f;

        foreach (var item in Player)
        {
            if (distance > Vector3.Distance(this.transform.position, item.transform.position))
            {
                distance = Vector3.Distance(this.transform.position, item.transform.position);
                mostClose = item.transform.position;
            }
        }

        transform.rotation = Quaternion.LookRotation(mostClose - transform.position).normalized;
    }

    IEnumerator DelayedAttack() //애니메이션과 공격 패턴이 같은 타이밍에 재생되게끔 하는 함수
    {
        doOnlyOnce = false;
        yield return new WaitForSeconds(fireBallThrower.GetComponent<HSH_FireBallThrower>().pinfo.cooltime - 0.73f);

        if (lichinfo.isHero)
        {
            lichstat = LichStat.Attack;
        }
        doOnlyOnce = true;
    }
}
