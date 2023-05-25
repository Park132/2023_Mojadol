using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HSH_TriggerBox_Golem : MonoBehaviour
{
    public GameObject GolemCreep;
    public bool isTherePlayer;
    int playerCount;
    // Start is called before the first frame update
    void Start()
    {
        isTherePlayer = false;
        playerCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCount > 0)
        {
            isTherePlayer = true;
        }

        else
        {
            isTherePlayer = false;
        }

        GolemCreep.GetComponent<HSH_GolemAgent>().creepinfo.isHero = isTherePlayer;
    }

    private void OnTriggerExit(Collider c)
    {
        //TriggerExit할 때 플레이어의 위치가 방 안쪽인가?
        //크립 룸 각도에 따라 새로 설정해야 할 수도 있습니다.
        //if ((c.CompareTag("RedTeam") || c.CompareTag("BlueTeam")) && c.transform.position.z >this.transform.position.z)
        if (c.CompareTag("PlayerMinion") && Mathf.Sign(Vector3.Dot(this.transform.forward, (c.transform.position - this.transform.position).normalized)) > 0)
        {
            playerCount++;
            //GolemCreep.GetComponent<HSH_LichCreepController>().Player.Add(c.gameObject);
        }

        else if (c.CompareTag("PlayerMinion") && Mathf.Sign(Vector3.Dot(this.transform.forward, (c.transform.position - this.transform.position).normalized)) < 0)
        {
            playerCount--;
            //GolemCreep.GetComponent<HSH_LichCreepController>().Player.Remove(c.gameObject);
        }
    }
}
