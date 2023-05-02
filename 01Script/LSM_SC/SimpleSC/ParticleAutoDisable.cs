using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ParticleAutoDisable : MonoBehaviourPunCallbacks
{
    ParticleSystem ps;
    bool alive;
    private void Start()
    {
        ps = this.GetComponent<ParticleSystem>();
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(CheckAlive());
    }
    private void OnEnable()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(CheckAlive());
    }
    private IEnumerator CheckAlive()
    {
        while (alive)
        {
            yield return new WaitForSeconds(0.5f);
            if (!ps.isPlaying)
            {
                Debug.Log("particle disable!");
                photonView.RPC("ParticleDisable_RPC", RpcTarget.All);
                break;
            }
        }
    }
    public void ParticleDisable() { photonView.RPC("ParticleDisable_RPC", RpcTarget.All); }
    [PunRPC] private void ParticleDisable_RPC() { this.gameObject.SetActive(false); }

    public void ParentSetting_Pool(int index) { photonView.RPC("ParentSP_RPC", RpcTarget.AllBuffered, index); }
    [PunRPC]
    private void ParentSP_RPC(int index)
    {
        this.transform.parent = PoolManager.Instance.gameObject.transform;
        PoolManager.Instance.poolList_Particles[index].Add(this.gameObject);
    }

    public void ParticleEnable(Vector3 position_)
    {photonView.RPC("ParticleEnable_RPC", RpcTarget.All, position_);}
    [PunRPC]private void ParticleEnable_RPC(Vector3 position_)
    {
        this.gameObject.SetActive(true);
        this.transform.position = position_;
    }
}
