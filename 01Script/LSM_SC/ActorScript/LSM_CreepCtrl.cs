using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Newtonsoft.Json.Bson;

public class LSM_CreepCtrl : MonoBehaviourPunCallbacks, I_Actor, IPunObservable, I_Characters
{
    public MoonHeader.S_CreepStats stat;

    private Rigidbody rigid;
    private GameObject icon;
    private Renderer[] bodies;  // ������ ������ ������.
    private HSH_LichCreepController mainCtrl;
    public int inPlayerNum;
    private bool once;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.gameObject.activeSelf);

            // maxHealth 2byte, health 2byte, team 8bit, atk 8bit, state 8bit
            ulong send_dummy = stat.SendDummyMaker();
            //stream.SendNext(send_dummy);

            int dummy_int1 = (int)(send_dummy & (ulong)uint.MaxValue);
            int dummy_int2 = (int)((send_dummy >> 32) & (ulong)uint.MaxValue);
            stream.SendNext(dummy_int1);
            stream.SendNext(dummy_int2);
            stream.SendNext(inPlayerNum);
        }
        else
        {
            bool isActive_ = (bool)stream.ReceiveNext();
            this.gameObject.SetActive(isActive_);

            ulong receive_dummy = ((ulong)(int)stream.ReceiveNext() & (ulong)uint.MaxValue);
            receive_dummy += (((ulong)(int)stream.ReceiveNext() & (ulong)uint.MaxValue) << 32);

            this.stat.ReceiveDummy(receive_dummy);
            inPlayerNum = (int)stream.ReceiveNext();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigid = this.GetComponent<Rigidbody>();
        once = false;

        icon = GameObject.Instantiate(PrefabManager.Instance.icons[11], transform);
        icon.transform.localPosition = new Vector3(0, 40, 0);
        icon.GetComponent<Renderer>().material.color = Color.yellow;
        bodies = this.transform.GetComponentsInChildren<Renderer>();

        mainCtrl = this.GetComponent<HSH_LichCreepController>();
        stat = new MoonHeader.S_CreepStats();

        // ����׿� maxHealth, Atk, Exp, Gold
        stat.Setting(100, 10, 1000, 1000);
        inPlayerNum = 0;
    }

    public void ResetCreep()
    {
        photonView.RPC("RC_RPC",RpcTarget.All);
    }
    [PunRPC] private void RC_RPC() 
    {
        this.stat.state = MoonHeader.CreepStat.Idle;
        stat.actorHealth.health = stat.actorHealth.maxHealth;
        this.mainCtrl.RegenProcessing();
        foreach (Renderer item in bodies) { item.enabled = true; }
    }

    // Update is called once per frame
    void Update()
    {
        if (!once && GameManager.Instance.onceStart && !PhotonNetwork.IsMasterClient)
        {
            mainCtrl.spellFieldGenerator.GetComponentInChildren<HSH_SpellFieldGenerator>().enabled = false;
        }
    }
    public void Damaged(short dam, Vector3 origin, MoonHeader.Team t, GameObject other)
    {
        // ���� Ȥ�� ���� ������ ��� �������� ��������. �ٷ� return
        if (stat.state == MoonHeader.CreepStat.Death || !PhotonNetwork.IsMasterClient)
            return;

        stat.actorHealth.health -= dam;

        if (stat.actorHealth.health > 0)
            photonView.RPC("DamMinion_RPC", RpcTarget.All);
        //StartCoroutine(DamagedEffect());

        //Debug.Log("Minion Damaged!! : " +stats.health);
        // ü���� 0 ���϶�� DeadProcessing
        if (stat.actorHealth.health <= 0 && stat.state != MoonHeader.CreepStat.Death)
        {
            StartCoroutine(DeadProcessing(other));
        }
        return;
    }

    public void Enable_Generator(bool b) { photonView.RPC("E_RPC", RpcTarget.All, b); }
    [PunRPC] private void E_RPC(bool b) { mainCtrl.spellFieldGenerator.SetActive(false); }

    [PunRPC]
    protected void DamMinion_RPC()
    {StartCoroutine(DamagedEffect());}

    private IEnumerator DamagedEffect()
    {
        Color damagedColor = new Color32(255, 150, 150, 255);

        //Vector3 knockbackDirection = Vector3.Scale(this.transform.position - origin, Vector3.zero - Vector3.up).normalized * 500 + Vector3.up * 100;

        foreach (Renderer r in bodies)
        { r.material.color = damagedColor; }
        //this.rigid.AddForce(knockbackDirection);

        yield return new WaitForSeconds(0.25f);
        foreach (Renderer r in bodies)
        { r.material.color = Color.white; }
    }


    private IEnumerator DeadProcessing(GameObject other)
    {
        stat.state = MoonHeader.CreepStat.Death;
        //nav_ob.enabled = false;

        //anim.SetBool("Dead", true);
        this.mainCtrl.lichstat = LichStat.Death;
        photonView.RPC("DeadAnim", RpcTarget.All);


        if (other.transform.CompareTag("PlayerMinion"))
        {
            other.GetComponent<I_Characters>().AddEXP((short)stat.exp);        // ���� �̴Ͼ��� �÷��̾� �̴Ͼ��̶�� ����ġ�� �ѹ� �� ��.
                                                                                //other.GetComponent<PSH_PlayerFPSCtrl>().myPlayerCtrl.GetExp(50);   // ���������� ���� ����ġ�� 50���� ���� ����.
                                                                                // ������ �÷��̾ �̴Ͼ��� óġ�Ͽ��ٸ�..
            GameManager.Instance.DisplayAdd(string.Format("{0} killed {1}", other.name, this.name));
        }

        else if (other.transform.CompareTag("DamageArea"))
        {
            other.GetComponent<LSM_W_Slash>().orner.GetComponent<I_Characters>().AddEXP((short)stat.exp);
        }

        yield return new WaitForSeconds(2f);
        // ����ִ� ������Ʈ ����.
        //dummy_item.transform.position = this.transform.position;

        for (int i = 0; i < 5; i++)
        {
            GameObject dummy_item = PoolManager.Instance.Get_Item(0);
            dummy_item.GetComponent<LSM_ItemSC>().SpawnSetting(this.stat.gold / 5, this.transform.position, 3f);
        }
        GiveExp();

        yield return new WaitForSeconds(1f);
        //this.gameObject.SetActive(false);
        //photonView.RPC("DeadP", RpcTarget.All);
    }
    [PunRPC]
    protected void DeadAnim()
    { 
        mainCtrl.DeadProcessing();
        stat.state = MoonHeader.CreepStat.Death;
        PoolManager.Instance.Get_Local_Item(1).transform.position = this.transform.position + Vector3.up * 1.5f;
        Invoke("Dead_renderer_disable", 5f);
    }
    private void Dead_renderer_disable() {
        if (stat.state == MoonHeader.CreepStat.Death)
        {
            foreach (Renderer item in bodies) { item.enabled = false; }
        }
    }

    public void GiveExp()
    {
        RaycastHit[] hits;
        float expRadius = 10f;
        hits = Physics.SphereCastAll(transform.position, expRadius, Vector3.up, 0, 1 << LayerMask.NameToLayer("Minion"));
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.CompareTag("PlayerMinion"))
            {
                hit.transform.GetComponent<I_Characters>().AddEXP((short)stat.exp);
            }
        }
    }

    // ������ �� ��ü �� ����.
    #region ChangeTeamColors
    // �����ε�. �Ű������� �������� ������� �̴Ͼ��� �������� ������ ����.
    public void ChangeTeamColor() { photonView.RPC("ChangeTC_RPC", RpcTarget.All); }

    // ���� Ȥ�� ������ �� �̴Ͼ��� ������ ���� ������ ����.
    [PunRPC]
    public void ChangeTC_RPC()
    {
        Color dummy_color;
        switch (stat.actorHealth.team)
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
    #endregion

    #region I_Actor
    public short GetHealth() { return this.stat.actorHealth.health; }
    public short GetMaxHealth() { return this.stat.actorHealth.maxHealth; }
    public MoonHeader.Team GetTeam() { return this.stat.actorHealth.team; }
    public void AddEXP(short exp) { }
    public MoonHeader.S_ActorState GetActor() { return this.stat.actorHealth; }
    public GameObject GetCameraPos() { return this.transform.gameObject; }
    public int GetState() { return (int)stat.state; }
    public void Selected()
    {
        this.icon.GetComponent<Renderer>().material.color = MoonHeader.SelectedColors[(int)this.stat.actorHealth.team];
    }

    public void Unselected()
    {    }
    #endregion
}
