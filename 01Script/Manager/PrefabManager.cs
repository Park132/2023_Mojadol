using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	// 싱글톤///
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

	// 0: 미니언
	public GameObject[] minions;
	// 0: 미니언마크 1: 공격로마크 2: 공격로UI 3: 포탑마크 4: 플레이어마크
	public GameObject[] icons;
}
