using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
	// ΩÃ±€≈Ê///
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


	public GameObject[] minions;
	public GameObject[] icons;
}
