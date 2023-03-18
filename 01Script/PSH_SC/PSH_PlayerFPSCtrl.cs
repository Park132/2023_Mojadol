using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSH_PlayerFPSCtrl : MonoBehaviour
{
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
    private bool canAttack = true;

    // ��ų���� ����
    private float qDamage = 30.0f;
    private bool canUseQ = true;

    // �̵� ���� ����
    public bool canMove = true; // ������ �� �ִ��� ������
    public float movespeed = 5.0f; // �̵��ӵ� ����

    // ī�޶� ���� ������
    public Camera playerCamera;
    public GameObject camerapos;
    bool cameraCanMove = true;
    bool invertCamera = false;
    float yaw = 0.0f;
    float pitch = 0.0f;
    public float mouseSensitivity = 3f; // ���콺 ����
    public float maxLookAngle = 50f; // ���� �þ߰�

    // Ÿ�̸�
    private float timer = 0.0f;

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
        // �� �̰� �ȸ���?
        if (state == State.Blocking || state == State.Attacking)
        {
            movespeed = 2.0f;
        }
        else if (state == State.Normal)
        {
            movespeed = 5.0f;
        }

        // �̵�, ī�޶� ����
        if(canMove)
            Move();

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

        Debug.Log($"Player Health : {Health}");
    }


    // �̵�
    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        movespeed = Input.GetKey(KeyCode.LeftShift) ? 8.0f : 5.0f; // �޸���
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

    // �⺻���� �ڷ�ƾ
    IEnumerator BasicAttack(float delay)
    {
        canAttack = false;
        state = State.Attacking;
        handpos.transform.localEulerAngles = new Vector3(0, 0, 0);
        handpos.transform.localEulerAngles += new Vector3(70.0f, 0, 0);
        
        yield return new WaitForSecondsRealtime(delay);

        canAttack = true;
        state = State.Normal;
        handpos.transform.localEulerAngles = new Vector3(0, 0, 0);
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
        }

        if (Input.GetMouseButtonUp(1))
        {
            handpos.transform.localEulerAngles = new Vector3(0, 0, 0);
            state = State.Normal;
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

    


}
