using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LSM_TabUI : MonoBehaviour
{
    public GameObject[] PlayerIcons;
    public TextMeshProUGUI PlayerName, PlayerLevel;
    private GameObject obj;
    private LSM_PlayerCtrl obj_p;

    public void Setting(int t, string name, GameObject o)
    {
        for (int i = 0; i < PlayerIcons.Length; i++) { PlayerIcons[i].SetActive(t==i); }
        PlayerName.text = name;
        obj = o;
        obj_p = o.GetComponent<LSM_PlayerCtrl>();
        PlayerLevel.text = "LV" +obj_p.GetLevel();
    }
    private void Update()
    {
        PlayerLevel.text = "LV" + obj_p.GetLevel();
    }
}
