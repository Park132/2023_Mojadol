using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LSM_AttackPathUI : MonoBehaviour
{
    public TextMeshProUGUI num;
    public Slider sl;
	public LSM_Spawner parentSpawner;
	public LSM_SpawnPointSc spawnPoint;
	private Camera mapcam;

	private void Start()
	{
		num.text = "0";
		num.text = sl.value.ToString();
		mapcam = GameManager.Instance.player[0].MapCam.GetComponent<Camera>();
		transform.SetAsFirstSibling();
	}

	private void OnEnable()
	{
		sl.gameObject.SetActive(true);
	}

	private void Update()
	{

		if (!ReferenceEquals(spawnPoint, null))
		{
			if (GameManager.Instance.gameState == MoonHeader.GameState.SettingAttackPath || 
				GameManager.Instance.gameState == MoonHeader.GameState.StartGame)
			{
				this.transform.position = Camera.main.WorldToScreenPoint(spawnPoint.Paths[0].transform.position);
			}
			else if (GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
			{
				this.transform.position = Camera.main.WorldToScreenPoint(spawnPoint.transform.position);
			}
			this.transform.localScale = Vector3.one * Mathf.Max(0.1f, Mathf.Min(1, 1 - (mapcam.orthographicSize - 40) * 0.015f));
		}
		
	}

	public void SetParent(LSM_SpawnPointSc sp)
	{
		spawnPoint = sp;
		parentSpawner = sp.parentSpawner.GetComponent<LSM_Spawner>();


		sl.maxValue = parentSpawner.MAX_NUM_MINION;

	}

	public void ChangeValue()
	{
		if (!ReferenceEquals(spawnPoint, null))
		{
			parentSpawner.spawnpoints[spawnPoint.number].num = (int)sl.value;
			num.text = sl.value.ToString();
			parentSpawner.PathUI_ChangeMaxValue();
		}
	}

	public void InvisibleSlider(bool change) { sl.gameObject.SetActive(change); }
}
