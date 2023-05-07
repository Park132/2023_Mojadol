using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LSM_ItemSC : MonoBehaviourPunCallbacks
{
    public int size;
    public bool isCollecting;
    private Rigidbody rigid;
    private IEnumerator discard_IE;
	private void Awake()
	{
        rigid = GetComponent<Rigidbody>();
        size = 0; isCollecting = false;
        discard_IE = Discard();
    }

    private void SpawnAnim() 
    {
        rigid.AddExplosionForce(500, this.transform.position + new Vector3(Random.Range(-5f,5f),-1f,Random.Range(-5f,5f)), 8, 5);
    }

    public void SpawnSetting(int s)
    {
        photonView.RPC("SpawnS_RPC",RpcTarget.All, s);
        Invoke("SpawnAnim", 0.5f);
        StopCoroutine(discard_IE);
        discard_IE = Discard();
        StartCoroutine(discard_IE);
        
    }
    private IEnumerator Discard()
    {
        yield return new WaitForSeconds(5f);
        if (!isCollecting)
            ItemDisable();
    }
    [PunRPC] private void SpawnS_RPC(int s) { size = s; isCollecting = false; }

    public void ItemEnable() { photonView.RPC("ItemE_RPC", RpcTarget.All); }
    [PunRPC] private void ItemE_RPC() { this.gameObject.SetActive(true); }

    public int Getting() { photonView.RPC("Getting_RPC", RpcTarget.All); return size; }
    [PunRPC] private void Getting_RPC() { isCollecting = true; Invoke("ItemDisable", 1f); }
    private void ItemDisable() { photonView.RPC("ItemD_RPC",RpcTarget.All); }
    [PunRPC] private void ItemD_RPC() { this.transform.gameObject.SetActive(false); }

    public void ParentSetting_Pool(int index) { photonView.RPC("ParentSetting_Pool_RPC", RpcTarget.AllBuffered, index); }
    [PunRPC]private void ParentSetting_Pool_RPC(int index)
    {
        this.transform.parent = PoolManager.Instance.gameObject.transform;
        PoolManager.Instance.poolList_Items[index].Add(this.gameObject);
    }
}
