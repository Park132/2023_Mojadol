using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PSH_PlayerUniversal : MonoBehaviourPunCallbacks, I_Actor, IPunObservable, I_Characters, I_Playable
{
    GameObject playerCharacter;
    Rigidbody rigid;

    // 이동속도, 점프 판별 함수
    public float speed = 15.0f;
    bool isGrounded;

    // 애니메이션 부분
    Transform myspine;
    Animator anim;
    int attackcode = 0;

    // 조작 부분
    bool canMove = true;


    // 조작 - 공격 관련 부분
    bool canAttack = true;
    bool canQ = true;
    bool canE = true;

	#region Camera Variants
	// 카메라 관련 변수들
	public Camera playerCamera;
    public GameObject camerapos; // eyes 연결
    bool cameraCanMove = false;
    bool invertCamera = false;
    float yaw = 0.0f;
    float pitch = 0.0f;
    public float mouseSensitivity = 3f; // 마우스 감도
    public float maxLookAngle = 50f; // 상하 시야각
    #endregion

    // 쿨타임 관련 변수.
    float CoolTime_Q, CoolTime_E;
    float timer_Q, timer_E;

    float time;

    #region LSM Variable
    public string playerName;
    public MoonHeader.S_ActorState actorHealth;
    private GameObject playerIcon;

    public LSM_PlayerCtrl myPlayerCtrl;
    public MoonHeader.State_P_Minion state_p;

    private Vector3 networkPosition, networkVelocity;

    private MeshCollider weapon_C;

    #endregion

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) // 되는 것 같긴한데 실제로 적용되는지는 확인하기 힘듬 
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

    // 패킷을 줄이기 위하여 압축해서 데이터를 전송.
    private ulong SendDummyMaker_LSM()
    {
        ulong send_dummy = 0;
        send_dummy += ((ulong)actorHealth.maxHealth & (ulong)ushort.MaxValue);
        send_dummy += ((ulong)(actorHealth.health) & (ulong)ushort.MaxValue) << 16;

        send_dummy += ((ulong)(actorHealth.team) & (ulong)byte.MaxValue) << 32;
        send_dummy += ((ulong)(actorHealth.Atk) & (ulong)byte.MaxValue) << 40;
        send_dummy += ((ulong)(state_p) & (ulong)byte.MaxValue) << 48;
        return send_dummy;
    }
    // 압축된 데이터를 언집
    private void ReceiveDummyUnZip(ulong receive_dummy)
    {
        //actorHealth.maxHealth = (short)(receive_dummy & (ulong)ushort.MaxValue);
        actorHealth.maxHealth = (short)(receive_dummy & (ulong)ushort.MaxValue);
        actorHealth.health = (short)((receive_dummy >> 16) & (ulong)ushort.MaxValue);
        actorHealth.team = (MoonHeader.Team)((receive_dummy >> 32) & (ulong)byte.MaxValue);
        actorHealth.Atk = (short)((receive_dummy >> 40) & (ulong)byte.MaxValue);
        state_p = (MoonHeader.State_P_Minion)((receive_dummy >> 48) & (ulong)byte.MaxValue);

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
        playerIcon.transform.localPosition = new Vector3(0, 60, 0);
        weapon_C = transform.GetComponentInChildren<LSM_WeaponSC>().transform.GetComponent<MeshCollider>();
        weapon_C.enabled = false;
        //
        CoolTime_E = 5f;
        CoolTime_Q = 3f;
    }


    // Start is called before the first frame update
    void Start()
    {
        
        
        
    }

    // Update is called once per frame
    void Update() 
    {
        // 지연보상에대한 내용.
        if (!photonView.IsMine)
        {
            rigid.velocity = networkVelocity;
            rigid.MovePosition(transform.position + networkVelocity * Time.deltaTime);
            return;
        }

        if (canMove)
            Move();
        AttackFunction();
        
        // anim.SetBool("skillE_Bool", Input.GetKey(KeyCode.E));

    }
    private void LateUpdate()
    {
        if (!photonView.IsMine)
            return;
        LookAround(); // 척추 움직임에 따른 시야 움직임이 적용될려면 이 함수가 LateUpdate()에서 호출 되어야함
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

        // 애니메이션
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
        //rigid.MovePosition(transform.position + thisVelocity * Time.deltaTime * speed);       // 웬지 모르게 fps차이에 따라서 속도가 다름...
        this.transform.position = this.transform.position + thisVelocity * speed * Time.deltaTime;

        // 점프
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

            myspine.transform.localEulerAngles = new Vector3(-180, 0, pitch); // 척추 움직에 따른 시야 변경
            // camerapos.transform.localEulerAngles = new Vector3(pitch, 0, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, yaw, 0);
            playerCamera.transform.position = camerapos.transform.position;
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
        #region 2개의 애니메이션 섞기
        /*
        float basic_attack_weight = 1.0f;

        if (anim.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.6f) //애니메이션이 절판쯤 진행 됐을 때 참이 됨, 끝까지 재생됨
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
        anim.SetTrigger("skillQ_Trigger"); Invoke("AnimatorRootMotionReset",1.6f);
    }

    void ESkill() // 혹시 Late Upadate에?
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


	#region SpawnSetting
	// LSM Spawn Setting
	public void SpawnSetting(MoonHeader.Team t, short monHealth, string pname, LSM_PlayerCtrl pctrl)
    {
        //Health = monHealth * 10;
        // 디버그용. 현재 강령하는 미니언의 체력의 10배율로 강령, 공격력을 10으로 디폴트. 이후 플레이어 공격력으로 변경할 예정
        this.photonView.RequestOwnership();
        actorHealth = new MoonHeader.S_ActorState(100, 10, t);
        actorHealth.health = (short)(monHealth * 10);
        playerName = pname;
        myPlayerCtrl = pctrl;
        state_p = MoonHeader.State_P_Minion.Normal;

        photonView.RPC("SpawnSetting_RPC", RpcTarget.All, (short)100, (short)(monHealth * 10), pname, (int)t);

        // 초기화
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

        this.transform.name = playerName;
        GameManager.Instance.playerMinions[(int)actorHealth.team].Add(this.gameObject);
        ChangeTeamColor(playerIcon);
    }
	#endregion

	#region Damaged()
	// LSM Damaged 추가.
	public void Damaged(short dam, Vector3 origin, MoonHeader.Team t, GameObject other)
    {

        if (t == actorHealth.team || state_p == MoonHeader.State_P_Minion.Dead)
            return;
        if (this.actorHealth.health - dam <= 0)
        { StartCoroutine(DeadProcessing(other)); }
        else
        {
            photonView.RPC("Dam_RPC", RpcTarget.All, dam);
        }
        return;
    }
    [PunRPC]
    private void Dam_RPC(short dam)
    {
        if (photonView.IsMine)
        {
            actorHealth.health -= dam;
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
        // 마지막 타격이 플레이어라면, 경험치 및 로그창 띄우기.
        if (other.transform.CompareTag("PlayerMinion"))
        {
            other.GetComponent<I_Characters>().AddEXP(50);
            //other.GetComponent<PSH_PlayerFPSCtrl>().myPlayerCtrl.GetExp(50);   // 디버깅용으로 현재 경험치를 50으로 고정 지급.
        }
        GameManager.Instance.DisplayAdd(string.Format("{0} Killed {1}", other.gameObject.name, this.name));
        yield return new WaitForSeconds(0.5f);
        MinionDisable();

        cameraCanMove = false;
        playerCamera = null;

    }

    #endregion

    #region ChangeTeamColor(obj)
    // 플레이어 아이콘 색변경.
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

	// I_Actor 인터페이스에 미리 선언해둔 함수들 구현
	public short GetHealth() { return this.actorHealth.health; }
    public short GetMaxHealth() { return this.actorHealth.maxHealth; }
    public MoonHeader.Team GetTeam() { return this.actorHealth.team; }
    public void AddEXP(short exp) { }
    public MoonHeader.S_ActorState GetActor() { return this.actorHealth; }
    public GameObject GetCameraPos() { return camerapos; }
    public void Selected() { }
    public int GetState() { return (int)state_p; }

    #region I_Playable
    public bool IsCanUseE() { return canE; }
    public bool IsCanUseQ() { return canQ; }
    public GameObject CameraSetting(GameObject cam)
    {
        playerCamera = cam.GetComponent<Camera>();
        return camerapos;
    }
    #endregion
}
