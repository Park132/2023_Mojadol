using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 상하로 회전 가능하게 만들어야 함
public class PSH_T_E : MonoBehaviour
{
    public float timer1;
    float timer2; // 블럭 연사속도 관련 타이머
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

        if(timer1 >= 5.0f) // 5초 후 부터 초당 2발의 연사속도로 블럭을 발사
        {
            if (timer2 > fireRate)
            {
                if (Physics.Raycast(this.transform.position, this.transform.forward * 50f, out hit))
                {
                    Debug.Log("Check!");
                    if (hit.transform.tag == "")
                    {
                        // 데미지 전달
                    }
                }
                timer2 = 0;
            }
        }
    }
}
