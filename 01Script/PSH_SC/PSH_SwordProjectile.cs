using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSH_SwordProjectile : MonoBehaviour
{
    public GameObject head;
    private float timer = 0.0f;
    private float power = 500.0f;
    public float damage = 0.0f;

    private void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * power);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 5.0f)
            Destroy(this.gameObject);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Player")
        {
            collision.transform.GetComponent<PSH_PlayerFPSCtrl>().Health -= damage;
        }

        Destroy(this.gameObject);
    }
}
