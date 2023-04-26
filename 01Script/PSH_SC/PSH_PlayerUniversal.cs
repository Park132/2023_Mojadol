using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSH_PlayerUniversal : MonoBehaviour
{
    GameObject playerCharacter;
    Rigidbody rigid;

    // �̵��ӵ�, ���� �Ǻ� �Լ�
    float speed = 15.0f;
    bool isGrounded;

    // �ִϸ��̼� �κ�
    Transform myspine;
    Animator anim;
    int attackcode = 0;



    #region Camera Variants
    // ī�޶� ���� ������
    public Camera playerCamera;
    public GameObject camerapos; // eyes ����
    bool cameraCanMove = true;
    bool invertCamera = false;
    float yaw = 0.0f;
    float pitch = 0.0f;
    public float mouseSensitivity = 3f; // ���콺 ����
    public float maxLookAngle = 50f; // ���� �þ߰�
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody>();
        playerCharacter = this.gameObject;
        playerCamera = Camera.main;
        anim = this.gameObject.GetComponent<Animator>();
        myspine = anim.GetBoneTransform(HumanBodyBones.Spine);
    }

    // Update is called once per frame
    void Update() 
    {
        Move();
        // LookAround();
        BasicAttack(true);

        /*
        if (Input.GetMouseButtonDown(0)) // �ٶ󺸴� ������ �°� ���� �ֵѷ�����
        {
            anim.SetTrigger("basicAttack");
        }
        */
    }

    private void LateUpdate()
    {
        LookAround(); // ô�� �����ӿ� ���� �þ� �������� ����ɷ��� �� �Լ��� LateUpdate()���� ȣ�� �Ǿ����
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // �ִϸ��̼�
        anim.SetBool("isRunFront", Input.GetKey(KeyCode.W));
        anim.SetBool("isRunBack", Input.GetKey(KeyCode.S));
        anim.SetBool("isRunRight", Input.GetKey(KeyCode.D));
        anim.SetBool("isRunLeft", Input.GetKey(KeyCode.A));

        Vector3 moveX = transform.right * x;
        Vector3 moveY = transform.forward * y;

        Vector3 thisVelocity = (moveX + moveY).normalized * speed;
        rigid.MovePosition(transform.position + thisVelocity * Time.deltaTime);

        // ����
        isGrounded = Physics.Raycast(this.transform.position-new Vector3(0f, 0.2f, 0f), Vector3.down * 5f);
        bool canJump = !isGrounded;
        anim.SetBool("isJump", !canJump);
        if(canJump)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                rigid.AddForce(Vector3.up * 500f);
            }
        }
    }

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
        }
        playerCamera.transform.position = camerapos.transform.position;
        // playerCamera.transform.rotation = camerapos.transform.rotation;
        // playerCamera.transform.SetPositionAndRotation(camerapos.transform.position, camerapos.transform.rotation);
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

                AnimatorStateInfo stateinfo = anim.GetCurrentAnimatorStateInfo(1);
                Debug.Log($"stateinfo's name : {stateinfo.IsName("slash5")}");

                if (anim.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.99f)
                {
                    
                }
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
}
