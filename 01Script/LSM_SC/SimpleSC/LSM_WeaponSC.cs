using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSM_WeaponSC : MonoBehaviour
{
    PSH_PlayerUniversal myParent;

    // Start is called before the first frame update
    void Start()
    {
        myParent = GetComponentInParent<PSH_PlayerUniversal>();
    }

    private void OnTriggerEnter(Collider other)
    {
        myParent.AttackThem(other.gameObject);
    }
}
