using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSM_W_Slash : MonoBehaviour
{
    public GameObject orner;
    public I_Actor orner_ac;
    public int dam;
    private float speed = 1f;
    private void Update()
    {
        this.transform.position = this.transform.position + this.transform.forward * Time.deltaTime * speed;
    }
    public void Setting(GameObject obj, int d, I_Actor ac, float v) { orner = obj; dam = d; orner_ac = ac; speed = v; }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player Slash Effect Dectected : " +other.name);
        if (!other.gameObject.Equals(orner) && PhotonNetwork.IsMasterClient && !ReferenceEquals(null, other.GetComponent<I_Actor>()))
        {
            Debug.Log("Player Effect Detect Other");
            other.GetComponent<I_Actor>().Damaged((short)dam, this.transform.position, orner_ac.GetTeam(), orner );
        }
    }
}
