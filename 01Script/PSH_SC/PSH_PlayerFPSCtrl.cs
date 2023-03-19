using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSH_PlayerFPSCtrl : MonoBehaviour
{
    // ���� �÷��̾� ����
    // �÷��̾� ����
    enum State { Normal, Attacking, Blocking, Casting, Exhausting};
    State state = State.Normal;

    // �÷��̾� ����
    public float Health = 100.0f;
    private float basicdamage = 20.0f;
    public float currentdamage = 20.0f;

    // ���ݱ�� ���� ����
    public GameObject handpos;
    public GameObject sword;
    public GameObject attackRange;
    public GameObject swordball_prefab;
    private bool canAttack = true;

    // ��ų���� ����
    private float qDamage = 30.0f;
    private bool canUseQ = true;
    public float eDamage = 25.0f;
    private float ePlusDamage = 0.0f;
    private bool ePressed = false;
    private bool canUseE = true;

    // �̵� ���� ����
    public bool canMove = true; // ������ �� �ִ��� ������
    public float movespeed = 5.0f; // �̵��ӵ� ����

    // ī�޶� ���� ������
    public Camera playerCamera;
    public GameObject camerapos;
    public bool canSee = true;
    bool cameraCanMove = true;
    bool invertCamera = false;
    float yaw = 0.0f;
    float pitch = 0.0f;
    public float mouseSensitivity = 3f; // ���콺 ����
    public float maxLookAngle = 50f; // ���� �þ߰�

    // Ÿ�̸�
    private float timer = 0.0f;

    // ���� ĳ�� �����ؼ� ����
    // Start is called before the first frame update
    void Start()
    {
        // ���� �� �ʱ�ȭ
        attackRange.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {

        // �̵�, ī�޶� ����
        if(canMove)
            Move();
        if(canSee)
            LookAround();

        // �⺻����
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(BasicAttack(1.0f));
            StartCoroutine(BasicAttackVolume(0.2f));
        }

        // ����
        if(canAttack)
            Block();

        // ��ų 1
        if (Input.GetKeyDown(KeyCode.Q) && canUseQ)
        {
            StartCoroutine(QskillActive(0.3f));
            StartCoroutine(QskillCool(3.0f));
        }

        // ��ų 2
        if (canUseE)
        {
            EskillActive();
        }

    }

    private void LateUpdate()
    {
        RecoverMoveSpeed();
    }


    // �̵�
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // movespeed = Input.GetKey(KeyCode.LeftShift) ? 8.0f : 5.0f; // �޸���
        this.transform.Translate(new Vector3(x, 0, y) * movespeed * Time.deltaTime);
    }

    // ī�޶� �̵�
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

    // �����̻� ȸ��(�̵��ӵ�)
    void RecoverMoveSpeed()
    {
        if(movespeed <= 2.0f)
        {
            timer += Time.deltaTime;

            if (timer >= 2.0f)
            {
                movespeed = 5.0f;
                timer = 0.0f;
            }
        }
    }

    // �⺻���� �ڷ�ƾ
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

    // ����
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

    // ��ų Q
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

    IEnumerator QskillCool (float delay)
    {
        canUseQ = false;
        yield return new WaitForSecondsRealtime(delay);
        canUseQ = true;
    }

    // ��ų E
    void EskillActive()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            state = State.Casting;
            canMove = false;
            ePressed = true;
            handpos.transform.localPosition = new Vector3(0, 0.1f, 0);
        }

        if(Input.GetKey(KeyCode.E))
        {
            timer += Time.deltaTime;
            if (timer <= 7.0f && ePressed)
            {
                ePlusDamage += 3.0f * Time.deltaTime;
            }
        }

        if(Input.GetKeyUp(KeyCode.E))
        {
            if(ePressed)
            {
                state = State.Normal;
                canMove = true;
                eDamage += ePlusDamage;
                ePlusDamage = 0.0f;
                handpos.transform.localPosition = new Vector3(0.6f, -0.2f, 0);

                GameObject sprefab = Instantiate(swordball_prefab, attackRange.transform.position, attackRange.transform.rotation);
                sprefab.gameObject.GetComponent<PSH_SwordProjectile>().damage = eDamage;

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

    // UI
}
