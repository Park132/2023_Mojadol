using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PSH_PlayerUniversal : MonoBehaviourPunCallbacks, I_Actor, IPunObservable, I_Characters, I_Playable
{
    GameObject playerCharacter;
    Rigidbody rigid;

    // �̵��ӵ�, ���� �Ǻ� �Լ�
    public float speed = 15.0f;
    bool isGrounded;

    // �ִϸ��̼� �κ�
    Transform myspine;
    Animator anim;
    int attackcode = 0;

    // ���� �κ�
    bool canMove = true;


    // ���� - ���� ���� �κ�
    bool canAttack = true;
    bool canQ = true;
    bool canE = true;

	#region Camera Variants
	// ī�޶� ���� ������
	public Camera playerCamera;
    public GameObject camerapos; // eyes ����
    bool cameraCanMove = false;
    bool invertCamera = false;
    float yaw = 0.0f;
    float pitch = 0.0f;
    public float mouseSensitivity = 3f; // ���콺 ����
    public float maxLookAngle = 50f; // ���� �þ߰�
    #endregion

    // ��Ÿ�� ���� ����.
    private float CoolTime_Q, CoolTime_E;
    private float timer_Q, timer_E;

    private float time;

    #region LSM Variable
    public string playerName;
    public MoonHeader.S_ActorState actorHealth;
    private GameObject playerIcon;

    public LSM_PlayerCtrl myPlayerCtrl;
    public MoonHeader.State_P_Minion state_p;

    private Vector3 networkPosition, networkVelocity;

    private MeshCollider weapon_C;
    private MeshRenderer icon_ren;
    private List<Material> icon_materialL;
    private bool selected_e;

    public float CollectingRadius, timer_collect;
    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // �Ǵ� �� �����ѵ� ������ ����Ǵ����� Ȯ���ϱ� ���� 
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerName);
            
            ulong send_dummy = SendDummyMaker_LSM();
            int dummy_int1 = (int)(send_dummy & (ulong)uint.MaxValue);
            int dummy_int2 = (int)((send_dummy >> 32) & (ulong)uint.MaxValue);
            stream.SendNext(dummy_int1);
            stream.SendNext(dummy_int2);
            stream.SendNext(rigid.velocity);
        }
        else
        {
            this.playerName = (string)stream.ReceiveNext();
            
            int d1 = (int)stream.ReceiveNext();
            int d2 = (int)stream.ReceiveNext();

            ulong receive_dummy = (ulong)(d1) & (ulong)uint.MaxValue;
            receive_dummy += ((ulong)(d2) << 32);
            ReceiveDummyUnZip(receive_dummy);
            networkVelocity = (Vector3)stream.ReceiveNext();
            rigid.velocity = networkVelocity;
        }
    }

    // ��Ŷ�� ���̱� ���Ͽ� �����ؼ� �����͸� ����.
    private ulong SendDummyMaker_LSM()
    {
        ulong send_dummy = 0;
        send_dummy += ((ulong)actorHealth.maxHealth & (ulong)ushort.MaxValue);
        send_dummy += ((ulong)(actorHealth.health) & (ulong)ushort.MaxValue) << 16;

        send_dummy += ((ulong)(actorHealth.team) & (ulong)byte.MaxValue) << 32;
        send_dummy += ((ulong)(actorHealth.Atk) & (ulong)ushort.MaxValue) << 40;
        send_dummy += ((ulong)(state_p) & (ulong)byte.MaxValue) << 56;
        return send_dummy;
    }
    // ����� �����͸� ����
    private void ReceiveDummyUnZip(ulong receive_dummy)
    {
        //actorHealth.maxHealth = (short)(receive_dummy & (ulong)ushort.MaxValue);
        actorHealth.maxHealth = (short)(receive_dummy & (ulong)ushort.MaxValue);
        actorHealth.health = (short)((receive_dummy >> 16) & (ulong)ushort.MaxValue);
        actorHealth.team = (MoonHeader.Team)((receive_dummy >> 32) & (ulong)byte.MaxValue);
        actorHealth.Atk = (short)((receive_dummy >> 40) & (ulong)ushort.MaxValue);
        state_p = (MoonHeader.State_P_Minion)((receive_dummy >> 56) & (ulong)byte.MaxValue);

    }


	private void Awake()
	{
        rigid = this.gameObject.GetComponent<Rigidbody>();
        playerCharacter = this.gameObject;
        //playerCamera = Camera.main;
        anim = this.gameObject.GetComponent<Animator>();
        myspine = anim.GetBoneTransform(HumanBodyBones.Spine);
        cameraCanMove = false;
        invertCamera = false;

        // LSM
        playerIcon = GameObject.Instantiate(PrefabManager.Instance.icons[4], transform);
        playerIcon.transform.localPosition = new Vector3(0, 33, 0);
        weapon_C = transform.GetComponentInChildren<LSM_WeaponSC>().transform.GetComponent<MeshCollider>();
        weapon_C.enabled = false;
        //
        CoolTime_E = 5f;
        CoolTime_Q = 3f;
        icon_materialL = new List<Material>();
        selected_e = false;
        CollectingRadius = 5f;
    }


    // Start is called before the first frame update
    void Start()
    {
        
        
        
    }

    // Update is called once per frame
    void Update() 
    {
        // �������󿡴��� ����.
        if (!photonView.IsMine)
        {
            rigid.velocity = networkVelocity;
            rigid.MovePosition(transform.position + networkVelocity * Time.deltaTime);
            return;
        }

        if (canMove)
            Move();
        AttackFunction();
        CollectingArea();
        // anim.SetBool("skillE_Bool", Input.GetKey(KeyCode.E));

    }
    private void LateUpdate()
    {
        if (!photonView.IsMine)
            return;
        LookAround(); // ô�� �����ӿ� ���� �þ� �������� ����ɷ��� �� �Լ��� LateUpdate()���� ȣ�� �Ǿ����
    }

    

    private void AttackFunction()
    {
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(basicAttackDelay());
        }

        if (Input.GetKeyDown(KeyCode.Q) && canQ && canAttack)
        {
            StartCoroutine(Qskill());
        }

        ESkill();
        CoolManager();
    }

    

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        bool sprint = Input.GetButton("Sprint");

        // �ִϸ��̼�
        /*
        anim.SetBool("isRunFront", Input.GetKey(KeyCode.W));
        anim.SetBool("isRunBack", Input.GetKey(KeyCode.S));
        anim.SetBool("isRunRight", Input.GetKey(KeyCode.D));
        anim.SetBool("isRunLeft", Input.GetKey(KeyCode.A));
        */
        anim.SetFloat("Front", y);
        anim.SetFloat("Right", x);

        Vector3 moveX = transform.right * x;
        Vector3 moveY = transform.forward * y;

        Vector3 thisVelocity = (moveX + moveY).normalized;
        //rigid.MovePosition(transform.position + thisVelocity * Time.deltaTime * speed);       // ���� �𸣰� fps���̿� ���� �ӵ��� �ٸ�...
        this.transform.position = this.transform.position + thisVelocity * speed * Time.deltaTime * (sprint?1.5f:1f);

        // ����
        isGrounded = Physics.Raycast(this.transform.position+new Vector3(0f, 0.5f, 0f), Vector3.down, 1f, 1<<LayerMask.NameToLayer("Map"));
        Debug.DrawRay(this.transform.position + Vector3.up * 0.5f, Vector3.down*1f, Color.red);
        //bool canJump = !isGrounded;

        anim.SetBool("InAir", !isGrounded);


        //anim.SetBool("isJump", !canJump && canQ);
        if(isGrounded)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                photonView.RPC("Jump_RPC", RpcTarget.All);
                rigid.AddForce(Vector3.up * 500f);
            }
        }
    }
    [PunRPC] private void Jump_RPC() { anim.SetTrigger("Jump"); }

    void LookAround()
    {
        if (cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                // Inverted Y
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            this.transform.localEulerAngles = new Vector3(0, yaw, 0);

            myspine.transform.localEulerAngles = new Vector3(-180, 0, pitch); // ô�� ������ ���� �þ� ����
            // camerapos.transform.localEulerAngles = new Vector3(pitch, 0, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, yaw, 0);
            playerCamera.transform.position = camerapos.transform.position + this.transform.forward * 0.2f;
        }
        
        // playerCamera.transform.rotation = camerapos.transform.rotation;
    }

    private void AnimatorLayerReset() { anim.SetLayerWeight(1, 0f); photonView.RPC("WeaponTriggerEnable", RpcTarget.MasterClient, false); }
    private void AnimatorRootMotionReset() { anim.applyRootMotion = false; photonView.RPC("WeaponTriggerEnable", RpcTarget.MasterClient, false); }

    [PunRPC] private void WeaponTriggerEnable (bool b) {
        weapon_C.enabled = b;
    }

    void BasicAttack(bool canAttack)
    {
        if(canAttack)
        {
            float upbody_weight = 1.0f;

            if (Input.GetMouseButtonDown(0))
            {
                attackcode++;
                attackcode %= 2;

                anim.SetLayerWeight(1, 1f);
                
                anim.SetTrigger("basicAttack" + attackcode.ToString());
            }
        }
        #region 2���� �ִϸ��̼� ����
        /*
        float basic_attack_weight = 1.0f;

        if (anim.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.6f) //�ִϸ��̼��� ������ ���� ���� �� ���� ��, ������ �����
        {
            if(basic_attack_weight>=0f)
            {
                basic_attack_weight -= Time.deltaTime;
            }
            anim.SetLayerWeight(1, basic_attack_weight);
        }
        */
        #endregion
    }

    IEnumerator basicAttackDelay()
    {
        canAttack = false;
        bool once = true;
        if(once)
        {
            once = false;
            attackcode++;
            attackcode %= 2;
        }
        //anim.SetLayerWeight(1, 1f);
        //anim.SetTrigger("basicAttack" + attackcode.ToString());
        photonView.RPC("basicAnim_RPC",RpcTarget.All, attackcode);

        yield return new WaitForSeconds(0.5f);
        photonView.RPC("WeaponTriggerEnable", RpcTarget.MasterClient, true);

        yield return new WaitForSecondsRealtime(1.5f);
        //anim.SetLayerWeight(1, 0f);
        canAttack = true;
        //StopCoroutine(basicAttackDelay());
    }
    [PunRPC] private void basicAnim_RPC(int attackcode) {
        anim.SetLayerWeight(1, 1f);
        
        anim.SetTrigger("basicAttack" + attackcode.ToString());
        Invoke("AnimatorLayerReset", 1.5f);
    }

    IEnumerator Qskill()
    {
        canQ = false;
        canAttack = false;
        canMove = false;
        timer_Q = 0;
        //anim.applyRootMotion = true;
        //anim.SetTrigger("skillQ_Trigger");
        photonView.RPC("QAnim_RPC",RpcTarget.All);
        yield return new WaitForSecondsRealtime(2.0f);
        //canQ = true;
        canAttack = true;
        canMove = true;
        //anim.applyRootMotion = false;
    }
    [PunRPC] private void QAnim_RPC() {
        if (photonView.IsMine)
            anim.applyRootMotion = true;
        photonView.RPC("WeaponTriggerEnable", RpcTarget.MasterClient, true);
        anim.SetTrigger("skillQ_Trigger"); Invoke("AnimatorRootMotionReset",1.65f);
    }

    void ESkill() // Ȥ�� Late Upadate��?
    {
        if (Input.GetKeyDown(KeyCode.E) && canAttack && canE)
        {
            //anim.SetLayerWeight(1, 1f);
            //anim.SetTrigger("skillE_Trigger");
            photonView.RPC("EAnim_RPC", RpcTarget.All);
            canMove = false;
            canAttack = false;
            canE = false;
            timer_E = 0;
        }

        if (anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.35f && anim.GetCurrentAnimatorStateInfo(1).IsName("casting1") && 
            Input.GetKey(KeyCode.E))
        {
            photonView.RPC("EAnim_Pause", RpcTarget.All);
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            Invoke("EskillOver", 1f);
            //canE = true;
            photonView.RPC("EAnimE_RPC",RpcTarget.All);
        }
    }
    private void EskillOver() { canMove = true; canAttack = true; }
    [PunRPC] private void EAnim_RPC() {
        anim.SetLayerWeight(1, 1f);
        anim.SetTrigger("skillE_Trigger");
    }
    [PunRPC] private void EAnim_Pause() { anim.speed = 0f; }
    [PunRPC] private void EAnimE_RPC()
    {
        anim.speed = 1f;
        Invoke("AnimatorLayerReset", 1.5f);
        //AnimatorLayerReset();
    }

    IEnumerator Eskill()
    {
        anim.SetLayerWeight(1, 1f);
        anim.SetTrigger("skillE_Trigger");
        canMove = false;
        canAttack = false;
        canE = false;

        if (anim.GetCurrentAnimatorStateInfo(1).normalizedTime >= 0.35f && anim.GetCurrentAnimatorStateInfo(1).IsName("casting1"))
            anim.speed = 0f;

        yield return new WaitForSecondsRealtime(1.0f);

        canMove = true;
        canAttack = true;
        canE = true;
        anim.speed = 1f;
        //StopCoroutine(Eskill());
    }

    private void CoolManager()
    {
        if (!canE)
        {
            timer_E += Time.deltaTime;
            if (CoolTime_E <= timer_E)
            {
                timer_E = 0; canE = true;
            }
        }
        if (!canQ)
        {
            timer_Q += Time.deltaTime;
            if (CoolTime_Q <= timer_Q)
            {
                timer_Q = 0; canQ = true;
            }
        }
    }


    public void AttackThem(GameObject obj)
    {
        if (!ReferenceEquals(obj.GetComponent<I_Actor>(), null))
        {
            obj.GetComponent<I_Actor>().Damaged(this.actorHealth.Atk, this.transform.position, this.actorHealth.team, this.gameObject);
            
            Debug.Log("Attack! : " +obj.name);
        }
    }

    private void DamagedRotation(Vector3 origin)
    {
        GameObject dummy_u = GameObject.Instantiate(PrefabManager.Instance.icons[5], GameManager.Instance.gameUI_SC.DamagedDirection.transform);
        LSM_DamagedDirection d = dummy_u.GetComponent<LSM_DamagedDirection>();
        d.SpawnSetting(this.gameObject, origin);
    }

	#region SpawnSetting
	// LSM Spawn Setting
	public void SpawnSetting(MoonHeader.Team t, short monHealth, string pname, LSM_PlayerCtrl pctrl)
    {
        //Health = monHealth * 10;
        // ����׿�. ���� �����ϴ� �̴Ͼ��� ü���� 10������ ����, ���ݷ��� 10���� ����Ʈ. ���� �÷��̾� ���ݷ����� ������ ����
        this.photonView.RequestOwnership();
        actorHealth = new MoonHeader.S_ActorState(100, 10, t);
        actorHealth.health = (short)(monHealth * 10);
        playerName = pname;
        myPlayerCtrl = pctrl;
        state_p = MoonHeader.State_P_Minion.Normal;

        photonView.RPC("SpawnSetting_RPC", RpcTarget.All, (short)100, (short)(monHealth * 10), pname, (int)t);

        // �ʱ�ȭ
        canAttack = true;
        canMove = true;
        speed = 5.0f;
        canE = true;
        canQ = true;
        cameraCanMove = true;
        invertCamera = false;
        timer_E = 0;
        timer_Q = 0;
    }

    [PunRPC]
    private void SpawnSetting_RPC(short mh, short h, string name, int t)
    {
        this.actorHealth.maxHealth = mh;
        this.actorHealth.health = h;
        this.playerName = name;
        this.actorHealth.team = (MoonHeader.Team)t;
        this.actorHealth.type = MoonHeader.AttackType.Melee;
        //if (selected_e)
            //Unselected();

        this.transform.name = playerName;
        GameManager.Instance.playerMinions[(int)actorHealth.team].Add(this.gameObject);
        ChangeTeamColor(playerIcon);
    }
	#endregion

	#region Damaged()
	// LSM Damaged �߰�.
	public void Damaged(short dam, Vector3 origin, MoonHeader.Team t, GameObject other)
    {

        if (t == actorHealth.team || state_p == MoonHeader.State_P_Minion.Dead)
            return;
        if (this.actorHealth.health - dam <= 0)
        { StartCoroutine(DeadProcessing(other)); }
        else
        {
            photonView.RPC("Dam_RPC", RpcTarget.All, dam, origin);
        }
        return;
    }
    [PunRPC]
    private void Dam_RPC(short dam, Vector3 origin)
    {
        if (photonView.IsMine)
        {
            actorHealth.health -= dam;
            DamagedRotation(origin);
        }
    }
    [PunRPC]
    private void Dead_RPC()
    {
        state_p = MoonHeader.State_P_Minion.Dead;
        if (photonView.IsMine)
            myPlayerCtrl.PlayerMinionDeadProcessing();
    }
    // LSM DeadProcessing
    public IEnumerator DeadProcessing(GameObject other)
    {
        canMove = false;
        photonView.RPC("Dead_RPC", RpcTarget.All);
        Debug.Log("PlayerMinion Dead");
        GameManager.Instance.PlayerMinionRemover(actorHealth.team, playerName);
        // ������ Ÿ���� �÷��̾���, ����ġ �� �α�â ����.
        if (other.transform.CompareTag("PlayerMinion"))
        {
            other.GetComponent<I_Characters>().AddEXP(50);
            //other.GetComponent<PSH_PlayerFPSCtrl>().myPlayerCtrl.GetExp(50);   // ���������� ���� ����ġ�� 50���� ���� ����.
        }
        GameManager.Instance.DisplayAdd(string.Format("{0} Killed {1}", other.gameObject.name, this.name));
        yield return new WaitForSeconds(0.5f);
        MinionDisable();

        cameraCanMove = false;
        playerCamera = null;

    }

    #endregion

    #region ChangeTeamColor(obj)
    // �÷��̾� ������ ������.
    public void ChangeTeamColor() { ChangeTeamColor(playerIcon); }

	public void ChangeTeamColor(GameObject obj)
    {
        Color dummy_color;
        switch (actorHealth.team)
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
        obj.GetComponent<Renderer>().material.color = dummy_color;
    }
	#endregion

	#region ParentSetting
	public void ParentSetting_Pool(int index) { photonView.RPC("ParentSetting_Pool_RPC", RpcTarget.AllBuffered, index); }
    [PunRPC]
    private void ParentSetting_Pool_RPC(int index)
    {
        this.transform.parent = PoolManager.Instance.gameObject.transform;
        PoolManager.Instance.poolList_PlayerMinions[index].Add(this.gameObject);
    }
	#endregion

	#region MinionDisable()
	public void MinionDisable() { photonView.RPC("DeadProcessing", RpcTarget.All); }
    [PunRPC]
    protected void DeadProcessing()
    {
        if (photonView.IsMine)
            myPlayerCtrl.PlayerMinionDeadProcessing();
        this.gameObject.SetActive(false);
    }
	#endregion

	#region MinionEnable()
	public void MinionEnable() { photonView.RPC("MinionEnable_RPC", RpcTarget.All); }
    [PunRPC] protected void MinionEnable_RPC() { this.gameObject.SetActive(true); }
	#endregion

	// I_Actor �������̽��� �̸� �����ص� �Լ��� ����
	public short GetHealth() { return this.actorHealth.health; }
    public short GetMaxHealth() { return this.actorHealth.maxHealth; }
    public MoonHeader.Team GetTeam() { return this.actorHealth.team; }
    public void AddEXP(short exp) { this.myPlayerCtrl.GetExp(exp); }
    public MoonHeader.S_ActorState GetActor() { return this.actorHealth; }
    public GameObject GetCameraPos() { return camerapos; }
    public void Selected()
    {
        /*
        icon_ren = playerIcon.GetComponent<MeshRenderer>();

        icon_materialL.Clear();
        icon_materialL.AddRange(icon_ren.materials);
        icon_materialL.Add(PrefabManager.Instance.outline);

        icon_ren.materials = icon_materialL.ToArray();
        selected_e = true;
        */
        this.playerIcon.GetComponent<Renderer>().material.color = MoonHeader.SelectedColors[(int)this.actorHealth.team];
    }

    public void Unselected()
    {
        MeshRenderer renderer_d = playerIcon.GetComponent<MeshRenderer>();

        icon_materialL.Clear();
        icon_materialL.AddRange(renderer_d.materials);
        //icon_materialL.Remove(PrefabManager.Instance.outline);

        icon_ren.materials = icon_materialL.ToArray();
        selected_e = false;
    }
    public int GetState() { return (int)state_p; }

    #region I_Playable
    public bool IsCanUseE() { return canE; }
    public bool IsCanUseQ() { return canQ; }
    public GameObject CameraSetting(GameObject cam)
    {
        playerCamera = cam.GetComponent<Camera>();
        return camerapos;
    }

    public void CollectingArea()
    {
        timer_collect += Time.deltaTime;
        if (timer_collect >= 0.5f)
        {
            timer_collect = 0;
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, CollectingRadius, Vector3.up, 0, 1 << LayerMask.NameToLayer("Item"));
            foreach(RaycastHit item in hits)
            {
                LSM_ItemSC dummy_i = item.transform.GetComponent<LSM_ItemSC>();
                if (!dummy_i.isCollecting)
                {
                    int size_d = dummy_i.Getting();
                    dummy_i.ItemDisable();
                    GameObject dummy_e = PoolManager.Instance.Get_Local_Item(0);
                    dummy_e.transform.position = item.transform.position + Vector3.up * 1f;
                    dummy_e.GetComponent<LSM_ItemCollectAnim>().TargetLockOn(this.gameObject, size_d);
                }
            }
        }
        
    }

    public void AddCollector(int s) { myPlayerCtrl.GetGold(s); }

    #endregion
}