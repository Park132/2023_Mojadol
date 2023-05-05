using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LSM_HPUI : MonoBehaviour
{
    public Image[] currentHpBar;
    public TextMeshProUGUI[] currentHpTxt;
    private LSM_NexusSC[] teamSpawners;
    // Start is called before the first frame update
    void Start()
    {
        teamSpawners = new LSM_NexusSC[2];
        for (int i = 0; i < 2; i++) {
            teamSpawners[i] = GameManager.Instance.teamManagers[i].this_teamSpawner.thisNexus;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            float HP = (float)teamSpawners[i].stats.actorHealth.health / teamSpawners[i].stats.actorHealth.maxHealth;
            currentHpBar[i].fillAmount = HP;
            currentHpTxt[i].text = Mathf.CeilToInt(HP * 100) + "%";
        }
    }
}
