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
	public GameObject[] playerMinions;
	public GameObject[] UIs;
	private GameObject alwaysEnableUI;

	// Ǯ ���� ����
	public List<GameObject>[] poolList_Minion;
	public List<GameObject>[] poolList_PlayerMinions;
	public List<GameObject>[] poolList_UIs;

	private void Start()
	{
		alwaysEnableUI = GameObject.Find("AlwaysEnableUI");
		/*
		for (int i = 0; i < minions.Length; i++)
		{
			for (int j = 0; j < 50; j++)
			{
				GameObject dummy = Get_Minion(i);
			}
		}
		for (int i = 0; i < minions.Length; i++)
		{
			for (int j = 0; j < 50; j++)
			{
				poolList_Minion[i][j].SetActive(false);
			}
		}*/
	}

	// �̱������� ���� Awake�� ���� ��ġ�Ͽ��⿡ �̰��� �Ʒ��� �Լ��� ���.
	private void Awake_Function()
	{
		// �̴Ͼ� ������ ������ŭ �迭�� ũ�⸦ ����.
		poolList_Minion = new List<GameObject>[minions.Length];
		for (int i = 0; i < poolList_Minion.Length; i++)
			poolList_Minion[i] = new List<GameObject>();
		poolList_PlayerMinions = new List<GameObject>[playerMinions.Length];
		for (int i = 0; i < poolList_PlayerMinions.Length; i++)
			poolList_PlayerMinions[i] = new List<GameObject>();
		poolList_UIs = new List<GameObject>[UIs.Length];
		for (int i = 0; i < poolList_UIs.Length;i++)
			poolList_UIs[i] = new List<GameObject>();
	}

	// �̴Ͼ��� ������ �´� �̴Ͼ��� ��ȯ.
	public GameObject Get_Minion(int index)
	{
		if (minions.Length <= index || index < 0)
			return null;
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

	// �÷��̾� �̴Ͼ� ��ȯ
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
				break;
			}
		}

		if (ReferenceEquals(result, null))
		{
			result = GameObject.Instantiate(playerMinions[index], this.transform);
			poolList_PlayerMinions[index].Add(result);
		}
		return result;
	}

	// UI ��ȯ
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
	
}
