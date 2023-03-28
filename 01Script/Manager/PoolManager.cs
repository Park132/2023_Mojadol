using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public GameObject[] minions;

	// 풀 저장 변수
	public List<GameObject>[] poolList_Minion;

	

	// 싱글톤으로 인해 Awake를 위로 배치하였기에 미관상 아래의 함수를 사용.
	private void Awake_Function()
	{
		// 미니언 종류의 개수만큼 배열의 크기를 지정.
		poolList_Minion = new List<GameObject>[minions.Length];
		for (int i = 0; i < poolList_Minion.Length; i++)
			poolList_Minion[i] = new List<GameObject>();
	}

	// 미니언의 종류에 맞는 미니언을 반환.
	public GameObject Get_Minion(int index)
	{
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
			result = GameObject.Instantiate(minions[index], this.transform);
			poolList_Minion[index].Add(result);
		}

		return result;
	}


}
