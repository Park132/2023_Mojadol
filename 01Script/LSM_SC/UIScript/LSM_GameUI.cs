using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


// 게임 진행 중에 플레이어에게 보이는 UI를 관리하는 스크립트.
public class LSM_GameUI : MonoBehaviour
{
    public Image playerHP, targetHP;
    public TextMeshProUGUI targetName;
    public GameObject targetUI, playerUI;
    
    private I_Actor player_ac, target_ac;
    private GameObject target_obj;


	private void OnEnable()
	{
		playerUI.SetActive(false);
        targetUI.SetActive(false);
	}

	// 타겟 UI 온오프.
	public void enableTargetUI(bool on)
    {
        targetUI.SetActive(on);
    }
    public void enableTargetUI(bool on, GameObject obj)
    {
        enableTargetUI(on);
        target_obj = obj;
        target_ac = obj.GetComponent<I_Actor>();
    }
    public void playerHealth(I_Actor a) { playerUI.SetActive(true);  player_ac = a; }
    // 모든 캐릭터들이 갖고있는 공통적인 것. I_Actor를 받아와 구문을 최소화.


    private void LateUpdate()
	{
        if (targetUI.activeSelf)
        {
            targetName.text = string.Format("{0} Team {1}", target_ac.GetTeam().ToString(), target_obj.name);
            targetHP.fillAmount = Mathf.Round((float)target_ac.GetHealth() / target_ac.GetMaxHealth() * 100) / 100;
            if (target_ac.GetHealth() <= 0)
                enableTargetUI(false);
        }

        if (playerUI.activeSelf)
        { playerHP.fillAmount = Mathf.Round((float)player_ac.GetHealth() / player_ac.GetMaxHealth() * 100) / 100; }
    }
}
