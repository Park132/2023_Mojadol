using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	// �̱���///
	private static PrefabManager instance;
	private void Awake()
	{
		if (instance == null)
			instance = this;
	}
	public static PrefabManager Instance
	{
		get { return instance; }
	}
	// ///

	// 0: �̴Ͼ�
	public GameObject[] minions;
	// 0: �̴Ͼ�ũ 1: ���ݷθ�ũ 2: ���ݷ�UI 3: ��ž��ũ 4: �÷��̾ũ
	public GameObject[] icons;
}
