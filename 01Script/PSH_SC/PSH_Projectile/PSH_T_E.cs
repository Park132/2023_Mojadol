using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���Ϸ� ȸ�� �����ϰ� ������ ��
public class PSH_T_E : MonoBehaviour
{
    public float timer1;
    float timer2; // �� ����ӵ� ���� Ÿ�̸�
    float fireRate = 0.5f;

    private void Start()
    {
        timer1 = 0.0f;
    }

    void Update()
    {
        timer1 += Time.deltaTime;
        timer2 += Time.deltaTime;

        RaycastHit hit;
        Debug.DrawRay(this.transform.position, this.transform.forward * 50f, Color.red);

        if (timer1 >= 8.0f)
        {
            this.gameObject.SetActive(false);
        }

        if(timer1 >= 5.0f) // 5�� �� ���� �ʴ� 2���� ����ӵ��� ���� �߻�
        {
            if (timer2 > fireRate)
            {
                if (Physics.Raycast(this.transform.position, this.transform.forward * 50f, out hit))
                {
                    Debug.Log("Check!");
                    if (hit.transform.tag == "")
                    {
                        // ������ ����
                    }
                }
                timer2 = 0;
            }
        }
    }
}
