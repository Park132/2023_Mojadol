using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSH_PlayerFPSCtrl : MonoBehaviour, I_Actor
{
    // 근접 플레이어 구현
    // 플레이어 상태
    enum State { Normal, Attacking, Blocking, Casting, Exhausting };
    State state = State.Normal;

    // 플레이어 변수
    public float Health = 100.0f;
    private float basicdamage = 20.0f;
    public float currentdamage = 20.0f;
    private float basicattackDelay = 1.0f;

    // 공격기능 관련 변수
    public GameObject handpos;
    public GameObject sword;
    public GameObject attackRange;
    public GameObject swordball_prefab;
    private bool canAttack = true;

    // 스킬관련 변수
    private float qDamage = 30.0f;
    private bool canUseQ = true;
    public float eDamage = 25.0f;
    private float ePlusDamage = 0.0f;
    private bool ePressed = false;
    private bool canUseE = true;

    // 스킬 레벨업 변수
    private int basicLevel = 1;
    private int qLevel = 1;
    private int eLevel = 1;

    // 이동 관련 변수
    public bool canMove = true; // 움직일 수 있는지 없는지
    public float movespeed = 5.0f; // 이동속도 관련

    // 카메라 관련 변수들
    public Camera playerCamera;
    public GameObject camerapos;
    public bool canSee = true;
    bool cameraCanMove = true;
    bool invertCamera = false;
    float yaw = 0.0f;
    float pitch = 0.0f;
    public float mouseSensitivity = 3f; // 마우스 감도
    public float maxLookAngle = 50f; // 상하 시야각

    // 타이머
    private float timer = 0.0f;

    // LSM 변경 아래 추가 변수 /*

    public string playerName;
    public MoonHeader.S_ActorState actorHealth;
    private GameObject playerIcon;

    private Rigidbody rigid;
    public LSM_PlayerCtrl myPlayerCtrl;
    public MoonHeader.State_P_Minion state_p;

	// LSM */

	private void Awake()
	{
        // LSM
        rigid = GetComponent<Rigidbody>();
        playerIcon = GameObject.Instantiate(PrefabManager.Instance.icons[4], transform);
        playerIcon.transform.localPosition = new Vector3(0, 60, 0);
        //
    }


    // 근접 캐릭 관련해서 만듬
    // Start is called before the first frame update
    void Start()
    {
        // 공격 구 초기화
        attackRange.SetActive(false);

        //Cursor.lockState = CursorLockMode.Locked;
        
    }

    // Update is called once per frame
    void Update()
    {

        // 이동, 카메라 조작
        if (canMove)
            Move();
        if (canSee)
            LookAround();

        // 기본공격
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(BasicAttack(basicattackDelay));
            StartCoroutine(BasicAttackVolume(0.2f));
        }

        // 막기
        if (canAttack)
            Block();

        // 스킬 1
        if (Input.GetKeyDown(KeyCode.Q) && canUseQ)
        {
            StartCoroutine(QskillActive(0.3f));
            StartCoroutine(QskillCool(3.0f));
        }

        // 스킬 2
        if (canUseE)
        {
            EskillActive();
        }

    }

    private void LateUpdate()
    {
        RecoverMoveSpeed();
    }


    // 이동
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // movespeed = Input.GetKey(KeyCode.LeftShift) ? 8.0f : 5.0f; // 달리기
        this.transform.Translate(new Vector3(x, 0, y) * movespeed * Time.deltaTime);
    }

    // 카메라 이동
    void LookAround()
    {
        playerCamera.transform.position = camerapos.transform.position;
        playerCamera.transform.rotation = camerapos.transform.rotation;


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
            camerapos.transform.localEulerAngles = new Vector3(pitch, 0, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, yaw, 0);
        }

    }

    // 상태이상 회복(이동속도)
    void RecoverMoveSpeed()
    {
        if (movespeed <= 2.0f)
        {
            timer += Time.deltaTime;

            if (timer >= 2.0f)
            {
                movespeed = 5.0f;
                timer = 0.0f;
            }
        }
    }

    // 기본공격 코루틴
    IEnumerator BasicAttack(float delay)
    {
        canAttack = false;
        currentdamage = basicdamage;
        state = State.Attacking;
        handpos.transform.localEulerAngles = new Vector3(0, 0, 0);
        handpos.transform.localEulerAngles += new Vector3(70.0f, 0, 0);
        movespeed = 3.0f;

        yield return new WaitForSecondsRealtime(delay);

        canAttack = true;
        state = State.Normal;
        handpos.transform.localEulerAngles = new Vector3(0, 0, 0);
        movespeed = 5.0f;
    }

    IEnumerator BasicAttackVolume(float delay)
    {
        attackRange.SetActive(true);
        yield return new WaitForSecondsRealtime(delay);
        attackRange.SetActive(false);
    }

    // 막기
    void Block()
    {
        if (Input.GetMouseButtonDown(1))
        {
            handpos.transform.localEulerAngles = new Vector3(0, 0, 70);
            state = State.Blocking;
            movespeed = 3.0f;
        }

        if (Input.GetMouseButtonUp(1))
        {
            handpos.transform.localEulerAngles = new Vector3(0, 0, 0);
            state = State.Normal;
            movespeed = 5.0f;
        }
    }

    // 스킬 Q
    IEnumerator QskillActive(float delay)
    {
        currentdamage = qDamage;
        handpos.transform.localPosition = new Vector3(0, 0, 1);
        handpos.transform.localEulerAngles = new Vector3(90, 0, 0);
        canAttack = false;
        sword.GetComponent<Collider>().enabled = true;

        yield return new WaitForSecondsRealtime(delay);

        currentdamage = basicdamage;
        handpos.transform.localPosition = new Vector3(0.6f, -0.2f, 0);
        handpos.transform.localEulerAngles = new Vector3(0, 0, 0);
        canAttack = true;
        sword.GetComponent<Collider>().enabled = false;
    }

    IEnumerator QskillCool(float delay)
    {
        canUseQ = false;
        yield return new WaitForSecondsRealtime(delay);
        canUseQ = true;
    }

    // 스킬 E
    void EskillActive()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            state = State.Casting;
            canMove = false;
            ePressed = true;
            handpos.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        if (Input.GetKey(KeyCode.E))
        {
            timer += Time.deltaTime;
            if (timer <= 7.0f && ePressed)
            {
                ePlusDamage += 3.0f * Time.deltaTime;
            }
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            if (ePressed)
            {
                state = State.Normal;
                canMove = true;
                eDamage += ePlusDamage;
                ePlusDamage = 0.0f;
                handpos.transform.localPosition = new Vector3(0.6f, -0.2f, 0);

                GameObject sprefab = Instantiate(swordball_prefab, attackRange.transform.position, attackRange.transform.rotation);
                sprefab.gameObject.GetComponent<PSH_SwordProjectile>().damage = eDamage;
                sprefab.GetComponent<PSH_SwordProjectile>().script = this.GetComponent<PSH_PlayerFPSCtrl>();

                eDamage = 25.0f;
                timer = 0.0f;
                ePressed = false;
                StartCoroutine(EskillCool(8.0f));
            }
        }
    }

    IEnumerator EskillCool(float delay)
    {
        canUseE = false;
        yield return new WaitForSecondsRealtime(delay);
        canUseE = true;
    }

    void LevelFunc(float attackExp, float qExp, float eExp)
    {
        if (attackExp > 50.0f)
            basicLevel = 2;
        else if (attackExp > 100.0f)
            basicLevel = 3;

        if (qExp > 50.0f)
            qLevel = 2;
        else if (qExp > 70.0f)
            qLevel = 3;

        if (eExp > 30.0f)
            eLevel = 2;
        else if (eExp > 50.0f)
            eLevel = 3;

        switch (basicLevel)
        {
            case 1:
                break;
            case 2:
                basicattackDelay = 0.7f;
                break;
            case 3:
                basicattackDelay = 0.3f;
                break;
        }

        switch (qLevel)
        {
            case 1:
                break;

        }
    }

    // LSM Spawn Setting
    public void SpawnSetting(MoonHeader.Team t, int monHealth, string pname, LSM_PlayerCtrl pctrl)
    {
        //Health = monHealth * 10;
        // 디버그용. 현재 강령하는 미니언의 체력의 10배율로 강령, 공격력을 10으로 디폴트. 이후 플레이어 공격력으로 변경할 예정
        actorHealth = new MoonHeader.S_ActorState(100, 10, t);
        actorHealth.health = monHealth * 10;
        playerName = pname;
        myPlayerCtrl = pctrl;
        ChangeTeamColor(playerIcon);
        state_p = MoonHeader.State_P_Minion.Normal;
    }

    // LSM Damaged 추가.
    public int Damaged(int dam, Vector3 origin, MoonHeader.Team t, GameObject other)
    {
        if (t == actorHealth.team || state_p == MoonHeader.State_P_Minion.Dead)
            return 0;
        actorHealth.health -= dam;
        // 넉백이 되는 방향벡터를 구함.
        //Vector3 direction_knock = Vector3.Scale(this.transform.position - origin, Vector3.one - Vector3.up).normalized;
        //float scale_knock = 100f;
        //rigid.AddForce(direction_knock * scale_knock);
        if (this.actorHealth.health <= 0)
            StartCoroutine(DeadProcessing(other));
        return (int)actorHealth.health;
    }
    // LSM DeadProcessing
    public IEnumerator DeadProcessing(GameObject other)
    {
        state_p = MoonHeader.State_P_Minion.Dead;
        Debug.Log("PlayerMinion Dead");
        GameManager.Instance.PlayerMinionRemover(actorHealth.team, playerName);
        // 마지막 타격이 플레이어라면, 경험치 및 로그창 띄우기.
        if (other.transform.CompareTag("PlayerMinion"))
        {
            other.GetComponent<PSH_PlayerFPSCtrl>().myPlayerCtrl.GetExp( 50);   // 디버깅용으로 현재 경험치를 50으로 고정 지급.
        }
        GameManager.Instance.DisplayAdd(string.Format("{0} Killed {1}",other.gameObject.name, this.name));
        yield return new WaitForSeconds(0.5f);
        this.gameObject.SetActive(false);
        myPlayerCtrl.PlayerMinionDeadProcessing();
    }

    // 플레이어 아이콘 색변경.
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


    // I_Actor 인터페이스에 미리 선언해둔 함수들 구현
    public int GetHealth() { return this.actorHealth.health; }
    public int GetMaxHealth() { return this.actorHealth.maxHealth; }
    public MoonHeader.Team GetTeam() { return this.actorHealth.team; }
}
