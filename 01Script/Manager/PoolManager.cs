using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
	// �̱��� //
	private static PoolManager instance;
	public static PoolManager Instance { get { return instance; } }
	private void Awake()
	{
		if (instance == null) instance = this;
		Awake_Function();
	}
	// ///

	// ������ ���� ����
	public GameObject[] minions;

	// Ǯ ���� ����
	public List<GameObject>[] poolList_Minion;

	

	// �̱������� ���� Awake�� ���� ��ġ�Ͽ��⿡ �̰��� �Ʒ��� �Լ��� ���.
	private void Awake_Function()
	{
		// �̴Ͼ� ������ ������ŭ �迭�� ũ�⸦ ����.
		poolList_Minion = new List<GameObject>[minions.Length];
		for (int i = 0; i < poolList_Minion.Length; i++)
			poolList_Minion[i] = new List<GameObject>();
	}

	// �̴Ͼ��� ������ �´� �̴Ͼ��� ��ȯ.
	public GameObject Get_Minion(int index)
	{
		GameObject result = null;

		// ���� ��Ȱ��ȭ �Ǿ��ִ� �̴Ͼ��� �����ϴ��� Ȯ��
		foreach (GameObject item in poolList_Minion[index])
		{
			if (!item.activeSelf)
			{
				result = item;
				item.SetActive(true);
				break;
			}
		}

		// ��Ȱ��ȭ�� �̴Ͼ��� �������� �ʴ´ٸ� ���� ����.
		if (ReferenceEquals(result, null))
		{
			result = GameObject.Instantiate(minions[index], this.transform);
			poolList_Minion[index].Add(result);
		}

		return result;
	}


}
