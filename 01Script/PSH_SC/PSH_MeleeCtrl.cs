using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSH_MeleeCtrl : MonoBehaviour
{
    public GameObject head;

    private void OnTriggerEnter(Collider other)
    {
        PSH_PlayerFPSCtrl fpsc = head.gameObject.GetComponent<PSH_PlayerFPSCtrl>();
        float thisdamage = fpsc.currentdamage;

        if(other.transform.tag == "Player")
        {
            other.GetComponent<PSH_PlayerFPSCtrl>().Health -= thisdamage;
            // other.GetComponent<PSH_PlayerFPSCtrl>().
        }
    }
}
