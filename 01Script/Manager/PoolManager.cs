using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
	// ������ ���� ����
	public GameObject[] minions;

	// Ǯ ���� ����
	public List<GameObject>[] poolList_Minion;

	// �̱��� //
	private static PoolManager instance;
	public static PoolManager Instance { get { return instance; } }
	private void Awake()
	{
		if (instance == null) instance = this;

		poolList_Minion = new List<GameObject>[minions.Length];
		
		for (int i = 0; i < poolList_Minion.Length; i++)
			poolList_Minion[i] = new List<GameObject>();
	}
	// ///

	public GameObject Get_Minion(int index)
	{
		GameObject result = null;

		foreach (GameObject item in poolList_Minion[index])
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
			result = GameObject.Instantiate(minions[index], this.transform);
			poolList_Minion[index].Add(result);
		}

		return result;
	}


}
