using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �� ������ ������ �Ʒ��� �̴Ͼ��� �����Ǵ� ������ ���� ����Ʈ
public class LSM_SpawnPointSc : MonoBehaviour
{
    public GameObject[] Ways;
	public GameObject[] Paths;
	public int number;
	public bool isClicked;
	public GameObject parentSpawner;

	public GameObject pathUI;

	private void OnDrawGizmos()
	{
		for (int i = 0; i < Ways.Length; i++)
		{
			Vector3 one;
			one = ((i == 0) ? this.transform.position : Ways[i - 1].transform.position);

			Gizmos.color = Color.red;
			Gizmos.DrawRay(one, Ways[i].transform.position - one);
		}
	}

	private void Start()
	{
		isClicked = false;
		Paths = new GameObject[Ways.Length];
		for (int i = 0; i < Paths.Length; i++)
		{
			Paths[i] = GameObject.Instantiate(PrefabManager.Instance.icons[1], transform);
			Paths[i].GetComponent<LSM_AttackPath>().SetVariable(this.gameObject, number);
			
		}
		parentSpawner = transform.parent.gameObject;
		pathUI = GameObject.Instantiate(PrefabManager.Instance.icons[2], GameManager.Instance.canvas.transform);
		pathUI.GetComponent<LSM_AttackPathUI>().SetParent(this);
	}

	//public void Click(bool change) { isClicked = change; }

	private void Update()
	{

		// ���ݷ� �������� ��ġ ����.
		for (int i = 0; i < Paths.Length; i++)
		{
			Vector3 origin;
			origin = ((i == 0) ? this.transform.position : Ways[i - 1].transform.position);

			Paths[i].transform.position = (Ways[i].transform.position - origin)*0.5f + origin;
			Paths[i].transform.LookAt(Ways[i].transform.position);
			Paths[i].transform.rotation = Quaternion.Euler(Paths[i].transform.rotation.eulerAngles + (Vector3.right * 90));
			Paths[i].transform.localPosition += Vector3.up * 50;
			float dummy_distance = Vector3.Distance(origin, Ways[i].transform.position);
			Paths[i].transform.localScale = Vector3.one + (Vector3.up * dummy_distance * 0.8f) + (Vector3.right * (dummy_distance * 0.3f)); 

		}
	}
}
