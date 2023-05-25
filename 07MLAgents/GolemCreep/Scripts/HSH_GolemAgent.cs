using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using JetBrains.Annotations;
using Unity.MLAgents.Actuators;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using Unity.MLAgents.Policies;
using System.Diagnostics.Tracing;
using Unity.Barracuda;

public enum GolemStat
{
    Idle,
    Walk,
    Charge,
    Groggy,
    Death
}

public class HSH_GolemAgent : Agent
{
    float spd;
    bool doOnlyOnce; bool doOnlyOnce2; //bool doOnlyOnce3;
    bool doOnlyOnce4;
    const float WALKSPEED = 3f; const float CHARGESPEED = 20f;
    const float WALKCOOL = 4.5f; const float CHARGECOOL = 2.5f;
    const float HP = 4;
    GolemStat stat;
    public CreepInfo creepinfo;
    PatternInfo patternInfo;

    Vector3 InitPos;

    public Rigidbody rb;
    public Animator anim;

    public override void Initialize()
    {
        spd = WALKSPEED;
        doOnlyOnce = true; doOnlyOnce2 = true; //doOnlyOnce3 = true;
        doOnlyOnce4 = true;

        stat = GolemStat.Walk;

        creepinfo = new CreepInfo();
        creepinfo.hp = HP;   //학습에는 필요하지 않은 데이터
        creepinfo.isHero = false;

        InitPos = this.transform.position;

        patternInfo = new PatternInfo();
        patternInfo.cooltime = WALKCOOL;
        patternInfo.isCool = false; //이 에이전트에서는 사용되지 않음
        patternInfo.dmg = 2f;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        PatCoolCtrl();
        StatCtrl();

        rb.inertiaTensor = Vector3.zero;
        rb.inertiaTensorRotation = Quaternion.Euler(Vector3.zero);

        if(creepinfo.hp <= 0)
        {
            spd = 0;
        }

        if(!creepinfo.isHero)
        {          
            doOnlyOnce4 = true;
            creepinfo.hp = HP;

            if(!(((transform.position.x > InitPos.x - 0.1) && (transform.position.x < InitPos.x + 0.1)) && ((transform.position.z > InitPos.z - 0.1) && (transform.position.z < InitPos.z + 0.1))))
            {
                Debug.Log("1");
                stat = GolemStat.Walk; patternInfo.cooltime = WALKCOOL;
                transform.rotation = Quaternion.LookRotation(InitPos - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, InitPos, spd*Time.fixedDeltaTime);
            }

            else
            {
                Debug.Log("2");
                stat = GolemStat.Idle;
            }
        }

        else
        {
            //doOnlyOnce3 = true;

            if (doOnlyOnce4)
            {
                Debug.Log("3");
                doOnlyOnce4 = false;
                stat = GolemStat.Walk;
                patternInfo.cooltime = WALKCOOL;
            }
        }


    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.transform.CompareTag("PlayerMinion"))
        {
            AddReward(0.1f);
            c.transform.GetComponent<HSH_PatternAvoider_Golem>().Damaged(patternInfo.dmg);

            /*원래 이 부분에 피격 대상을 넉백시키는 코드를 넣었었는데 플레이어랑 어떻게 호환될지 몰라서 주석처리 했습니다.
             
             c.gameObject.GetComponent<Rigidbody>().AddForce(this.transform.position - c.transform.position * 50f, ForceMode.Impulse);
             */
        }

        if (c.transform.CompareTag("iWall") && doOnlyOnce && stat == GolemStat.Charge)
        {
            doOnlyOnce = false;
            StartCoroutine(Groggy(c));
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.0005f);

        WalkAndCharge(actions.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //x축 이동
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 0;
        }

        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 2;
        }

        else
        {
            discreteActionsOut[0] = 1;
        }
    }

    public override void OnEpisodeBegin()
    {
        stat = GolemStat.Walk; patternInfo.cooltime = WALKCOOL;
    }

    void PatCoolCtrl()
    {
        if (stat != GolemStat.Groggy && stat != GolemStat.Idle && creepinfo.hp > 0f)
        {
            patternInfo.cooltime -= Time.fixedDeltaTime;

            if (stat == GolemStat.Walk && patternInfo.cooltime < 0.5f)
            {
                stat = GolemStat.Charge; patternInfo.cooltime = CHARGECOOL;
            }

            else if (stat == GolemStat.Charge && patternInfo.cooltime < 0.5f)
            {
                stat = GolemStat.Walk; patternInfo.cooltime = WALKCOOL;
            }
        }

        else if (creepinfo.hp <= 0)
        {           

            if (doOnlyOnce2)
            {
                doOnlyOnce2 = false;
                stat = GolemStat.Death;
                StartCoroutine(Death());
            }
        }
    }

    void StatCtrl()
    {
        switch (stat)
        {
            case GolemStat.Idle:
                spd = 0;
                anim.ResetTrigger("Charge");
                anim.ResetTrigger("Groggy");
                anim.ResetTrigger("Death");
                anim.ResetTrigger("Walk");
                anim.SetTrigger("Idle");
                break;
            case GolemStat.Walk:
                spd = WALKSPEED;
                anim.ResetTrigger("Charge");
                anim.ResetTrigger("Groggy");
                anim.ResetTrigger("Death");
                anim.ResetTrigger("Idle");
                anim.SetTrigger("Walk");
                break;
            case GolemStat.Charge:
                spd = CHARGESPEED;
                anim.ResetTrigger("Walk");
                anim.ResetTrigger("Groggy");
                anim.ResetTrigger("Death");
                anim.ResetTrigger("Idle");
                anim.SetTrigger("Charge");
                break;
            case GolemStat.Groggy:
                spd = 0;
                anim.ResetTrigger("Walk");
                anim.ResetTrigger("Charge");
                anim.ResetTrigger("Death");
                anim.ResetTrigger("Idle");
                anim.SetTrigger("Groggy");
                break;
            case GolemStat.Death:
                spd = 0;
                anim.ResetTrigger("Walk");
                anim.ResetTrigger("Charge");
                anim.ResetTrigger("Groggy");
                anim.ResetTrigger("Idle");
                anim.SetTrigger("Death");
                break;
        }
    }

    void WalkAndCharge(ActionSegment<int> act)
    {
        var rotateDir = Vector3.zero;

        switch (act[0])
        {
            case 0:
                rotateDir = transform.up * 1f;
                break;
            case 1:
                break;
            case 2:
                rotateDir = transform.up * -1f;
                break;
        }

        if (stat != GolemStat.Groggy && creepinfo.isHero)
        {
            transform.Rotate(rotateDir, Time.fixedDeltaTime * 100f);
        }
        rb.velocity = transform.forward * spd * 50f * Time.fixedDeltaTime;
    }

    IEnumerator Groggy(Collision c)
    {
        stat = GolemStat.Groggy;

        yield return new WaitForSeconds(4f);

        patternInfo.cooltime = WALKCOOL;
        stat = GolemStat.Walk;
        doOnlyOnce = true;

        transform.rotation = Quaternion.LookRotation(InitPos - transform.position);
    }

    IEnumerator Death()
    {
        GetComponent<BehaviorParameters>().Model = null;

        yield return new WaitForSeconds(4.586f);

        Destroy(this.gameObject);
    }
}
