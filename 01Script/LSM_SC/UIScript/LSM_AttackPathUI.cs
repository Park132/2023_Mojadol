using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// ���ݷ� ���� ������ ���Ǵ� ������ ��ũ��Ʈ.
public class LSM_AttackPathUI : MonoBehaviour
{
    public TextMeshProUGUI num;				// ǥ�õǴ� ����.
    public Slider sl;						// �����̴�
	public LSM_Spawner parentSpawner;		// �ش� UI�� ���Ǵ� �����ͽ�����
	public LSM_SpawnPointSc spawnPoint;		// �ش� UI�� ���Ǵ� ��������Ʈ
	private Camera mapcam;                  // ���� ���� ���Ǵ� ī�޶�
	private bool once;

	private void Start_function()
	{
		//num.text = "0";
		once = true;
		num.text = sl.value.ToString();
		mapcam = GameManager.Instance.mainPlayer.MapCam.GetComponent<Camera>();
		//transform.SetAsFirstSibling();	// �ش� UI�� �ٸ� UI�� ������ �ʱ� ���� ���� ������� ��ġ. �ش� �κ��� EmptyObject���� �ڽ� ������Ʈ�� �����ϱ⿡ �ʿ���� �κ��� �Ǿ���.
	}

	// �ٽ� UI�� Ȱ��ȭ �� ���, ���� ���� ���� �� ���ݷ� ���� �Ͽ� �����̴��� ǥ���ϵ��� ����.
	private void OnEnable()
	{
		sl.gameObject.SetActive(GameManager.Instance.gameState == MoonHeader.GameState.SettingAttackPath);
	}

	private void Update()
	{
		// ���� �� �ʱ�ȭ�� �ϱ� ���� ����Ǵ� ���� ����.
		if (!ReferenceEquals(spawnPoint, null) && GameManager.Instance.onceStart
			&& GameManager.Instance.onceStart)
		{
			if (!once)
				Start_function();

			// ���� ���� ��, ���ݷ� ���� �߿� ��������Ʈ�� ù��° ��������Ʈ ���̿� UI�� ��ġ�ϵ��� ����.
			if (GameManager.Instance.gameState == MoonHeader.GameState.SettingAttackPath || 
				GameManager.Instance.gameState == MoonHeader.GameState.StartGame)
			{
				//this.transform.position = Camera.main.WorldToScreenPoint(spawnPoint.Paths[0].transform.position);
				this.transform.position = mapcam.WorldToScreenPoint(spawnPoint.Paths[0].transform.position);
				//num.text = GameManager.Instance.teamManagers[(int)parentSpawner.team].AttackPathNumber[spawnPoint.number].ToString();
				num.text = sl.value.ToString();
			}
			// ���� ��, ��������Ʈ�� ��ġ�� UI�� ��ġ�ϵ��� ����.
			else if (GameManager.Instance.gameState == MoonHeader.GameState.Gaming)
			{
				//this.transform.position = Camera.main.WorldToScreenPoint(spawnPoint.transform.position);
				this.transform.position = mapcam.WorldToScreenPoint(spawnPoint.transform.position);
				num.text = parentSpawner.spawnpoints[spawnPoint.number].num.ToString();
			}
			this.transform.localScale = Vector3.one * Mathf.Max(0.1f, Mathf.Min(1, 1 - (mapcam.orthographicSize - 40) * 0.015f));
		}
		
	}

	// �ʱ�ȭ�Լ�. ��������Ʈ���� �ش� UI�� ���� �� UI�� �Ҽӵ� ������ �����ʿ�, ��������Ʈ�� �����ϵ��� ����.
	public void SetParent(LSM_SpawnPointSc sp)
	{
		spawnPoint = sp;
		parentSpawner = sp.parentSpawner.GetComponent<LSM_Spawner>();


		sl.maxValue = GameManager.Instance.teamManagers[(int)parentSpawner.team].MaximumSpawnNum;

	}

	// �����̴��� ���� ���� ��� �ߵ��Ǵ� �Լ�.
	// �ش��ϴ� ���� �Ŵ������� ���� �����ϵ��� ����.
	public void ChangeValue()
	{
		if (!ReferenceEquals(spawnPoint, null))
		{
			GameManager.Instance.teamManagers[(int)parentSpawner.team].AttackPathNumber[spawnPoint.number] = (int)sl.value;
			//parentSpawner.spawnpoints[spawnPoint.number].num = (int)sl.value;
			num.text = sl.value.ToString();
			// ���Ŵ����� �����ϴ� �ִ밪 ���� �Լ� ȣ��.
			GameManager.Instance.teamManagers[(int)parentSpawner.team].PathUI_ChangeMaxValue();
		}
	}

	// �����̴� ��Ȱ��ȭ
	public void InvisibleSlider(bool change) { sl.gameObject.SetActive(change); }
}
