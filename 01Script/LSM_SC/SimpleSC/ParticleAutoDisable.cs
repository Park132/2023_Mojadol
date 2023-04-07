using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAutoDisable : MonoBehaviour
{
    ParticleSystem ps;
    bool alive;
    private void Start()
    {
        ps = this.GetComponent<ParticleSystem>();
        StartCoroutine(CheckAlive());
    }
    private void OnEnable()
    {
        StartCoroutine(CheckAlive());
    }
    private IEnumerator CheckAlive()
    {
        while (alive)
        {
            yield return new WaitForSeconds(1f);
            if (!ps.isPlaying)
            {
                this.gameObject.SetActive(false);
                break;
            }
        }
    }

}
