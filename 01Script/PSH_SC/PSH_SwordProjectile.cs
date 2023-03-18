using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSH_SwordProjectile : MonoBehaviour
{
    public GameObject head;
    private float timer = 0.0f;
    private float speed = 5.0f;
    public float damage = 0.0f;


    private void Update()
    {
        timer += Time.deltaTime;
        this.transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (timer >= 5.0f)
            Destroy(this.gameObject);

        Debug.Log($"This damage : {damage}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Player")
        {
            Debug.Log("Collision Check1");
            collision.transform.GetComponent<PSH_PlayerFPSCtrl>().Health -= damage;
            Debug.Log("Collision Check2");
        }

        Destroy(this.gameObject);
    }
}
