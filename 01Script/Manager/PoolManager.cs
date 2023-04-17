using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class PoolManager : MonoBehaviour
{
	// 싱글톤 //
	private static PoolManager instance;
	public static PoolManager Instance { get { return instance; } }
	private void Awake()
	{
		if (instance == null) instance = this;
		Awake_Function();
	}
	// ///

	// 프리펩 저장 변수
	public GameObject[] minions;		// # 0: LSM 폴더 내의 Minion1, 1: LSM 폴더 내의 Minion2
	public GameObject[] playerMinions;	// # 0: PSH 폴더 내의 MeleeCharacter
	public GameObject[] UIs;            // # 0: Icon 폴더 내의 display
	public GameObject[] particles;
	private GameObject alwaysEnableUI;

	// 풀 저장 변수
	public List<GameObject>[] poolList_Minion;
	public List<GameObject>[] poolList_PlayerMinions;
	public List<GameObject>[] poolList_UIs;
	public List<GameObject>[] poolList_Particles;

	private bool once;

	public int ReadyToStart_SpawnNum;

	private void Start()
	{
		alwaysEnableUI = GameObject.Find("AlwaysEnableUI");
	}

    // 싱글톤으로 인해 Awake를 위로 배치하였기에 미관상 아래의 함수를 사용.
    private void Awake_Function()
	{
		// 미니언 종류의 개수만큼 배열의 크기를 지정.
		poolList_Minion = new List<GameObject>[minions.Length];
		for (int i = 0; i < poolList_Minion.Length; i++)
			poolList_Minion[i] = new List<GameObject>();
		poolList_PlayerMinions = new List<GameObject>[playerMinions.Length];
		for (int i = 0; i < poolList_PlayerMinions.Length; i++)
			poolList_PlayerMinions[i] = new List<GameObject>();
		poolList_UIs = new List<GameObject>[UIs.Length];
		for (int i = 0; i < poolList_UIs.Length;i++)
			poolList_UIs[i] = new List<GameObject>();
        poolList_Particles = new List<GameObject>[particles.Length];
        for (int i = 0; i < poolList_Particles.Length; i++)
            poolList_Particles[i] = new List<GameObject>();

		ReadyToStart_SpawnNum = 30;
    }

	public IEnumerator ReadyToStart_Spawn()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			for (int i = 0; i < minions.Length; i++)
			{
				for (int j = 0; j < ReadyToStart_SpawnNum; j++)
				{
					yield return new WaitForSeconds(Time.deltaTime);
					Get_Minion(i);
                    GameManager.Instance.LoadingUpdate();
                }
				foreach (GameObject item in poolList_Minion[i])
				{
					yield return new WaitForSeconds(Time.deltaTime);
					item.GetComponent<LSM_MinionCtrl>().MinionDisable();
                }
                GameManager.Instance.LoadingUpdate();
            }
			GameManager.Instance.PlayerReady();
		}
		else
		{
			while (true)
			{
				yield return new WaitForSeconds(Time.deltaTime);
				if (GameManager.Instance.LoadingGauge >= GameManager.Instance.ReadyToStart_LoadingGauge)
				{ break; }
			}
			yield return new WaitForSeconds(3f);				// 디버깅용. 삭제해도 무방.
            GameManager.Instance.PlayerReady();
        }
	}

    // 미니언의 종류에 맞는 미니언을 반환.
	public GameObject Get_Minion(int index)
	{
		if (minions.Length <= index || index < 0)
			return null;
		GameObject result = null;

		// 현재 비활성화 되어있는 미니언이 존재하는지 확인
		foreach (GameObject item in poolList_Minion[index])
		{
			if (!item.activeSelf)
			{
				result = item;
				item.SetActive(true);
				break;
			}
		}

		// 비활성화된 미니언이 존재하지 않는다면 새로 생성.
		if (ReferenceEquals(result, null))
		{
			//result = GameObject.Instantiate(minions[index], this.transform);
			result = PhotonNetwork.Instantiate(minions[index].name, Vector3.zero, Quaternion.identity);
			//result.transform.parent = this.transform;
			result.GetComponent<LSM_MinionCtrl>().ParentSetting_Pool(index);
			//poolList_Minion[index].Add(result);
			
		}

		return result;
	}

	// 플레이어 미니언 반환
	public GameObject Get_PlayerMinion(int index)
	{
		if (index >= playerMinions.Length || index < 0)
			return null;
		GameObject result = null;

		foreach (GameObject item in poolList_PlayerMinions[index])
		{
			if (!item.activeSelf)
			{
				result = item;
				item.SetActive(true);
				item.GetComponent<PSH_PlayerFPSCtrl>().MinionEnable();
				break;
			}
		}

		if (ReferenceEquals(result, null))
		{
			//result = GameObject.Instantiate(playerMinions[index], this.transform);
			result = PhotonNetwork.Instantiate(playerMinions[index].name,Vector3.zero, Quaternion.identity);
			result.GetComponent<PSH_PlayerFPSCtrl>().ParentSetting_Pool(index);
			//result.transform.parent = this.transform;
			//poolList_PlayerMinions[index].Add(result);
		}
		return result;
	}

	// UI 반환
	public GameObject Get_UI(int index)
	{
		if (index >= UIs.Length || index < 0)
			return null;
		GameObject result = null;

		foreach (GameObject item in poolList_UIs[index])
		{
			if (!item.activeSelf)
			{
				result = item;
				item.SetActive(true);
				break;
			}
		}
		if (ReferenceEquals(result, null))
		{
			result = GameObject.Instantiate(UIs[index], alwaysEnableUI.transform);
			poolList_UIs[index].Add(result);
		}
		return result;
	}

    // 파티클 반환
    public GameObject Get_Particles(int index)
    {
        if (index >= particles.Length || index < 0)
            return null;
        GameObject result = null;

        foreach (GameObject item in poolList_Particles[index])
        {
            if (!item.activeSelf)
            {
                result = item;
                item.SetActive(true);
                break;
            }
        }
        if (ReferenceEquals(result, null))
        {
            result = GameObject.Instantiate(particles[index], this.transform);
            poolList_Particles[index].Add(result);
        }
        return result;
    }
}
