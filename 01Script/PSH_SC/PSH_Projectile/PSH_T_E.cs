using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���Ϸ� ȸ�� �����ϰ� ������ ��
public class PSH_T_E : MonoBehaviour
{
    public float timer1;
    float timer2; // �� ����ӵ� ���� Ÿ�̸�
    float fireRate = 0.5f;
    LSM_PlayerBase myCtrl;
    GameObject thisObj;

    private void Awake()
    {
        timer1 = 0;
        timer2 = 0;
        myCtrl = this.GetComponentInParent<LSM_PlayerBase>();
        thisObj = myCtrl.gameObject;
    }
    private void Start()
    {
        timer1 = 0.0f;
    }

    void Update()
    {
        timer1 += Time.deltaTime;
        timer2 += Time.deltaTime;

        //Debug.DrawRay(this.transform.position, this.transform.forward * 50f, Color.red);

        if (timer1 >= 8.0f)
        {
            this.gameObject.SetActive(false);
        }

        //if (!PhotonNetwork.IsMasterClient)
            //return;

        if(timer1 >= 5.0f) // 5�� �� ���� �ʴ� 2���� ����ӵ��� ���� �߻�
        {
            if (timer2 > fireRate)
            {
                timer2 = 0;
                Debug.DrawRay(this.transform.position, this.transform.forward * 35f, Color.blue, 1f);
                RaycastHit[] hits = Physics.RaycastAll(this.transform.position, this.transform.forward,50f,
                    1<< LayerMask.NameToLayer("Minion") | 1 << LayerMask.NameToLayer("Turret"));
                foreach(RaycastHit hit in hits)
                {
                    I_Actor dummy_ac = hit.transform.GetComponent<I_Actor>();
                    if (!ReferenceEquals(dummy_ac, null) && dummy_ac.GetTeam() != myCtrl.actorHealth.team)
                    {
                        PoolManager.Instance.Get_Local_Item(2).transform.position = hit.point;
                        if (PhotonNetwork.IsMasterClient)
                            dummy_ac.Damaged(myCtrl.actorHealth.Atk, this.transform.position, myCtrl.actorHealth.team, thisObj);
                    }

                }

                timer2 = 0;
            }
        }
    }


}
