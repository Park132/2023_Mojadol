using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


// ���� ���� �߿� �÷��̾�� ���̴� UI�� �����ϴ� ��ũ��Ʈ.
public class LSM_GameUI : MonoBehaviour
{
                                                // # Canvas �� GameUI�� �ڽĿ�����Ʈ
    public Image playerHP, targetHP;            // # Player�� �ڽ� ������Ʈ �� CurrentHP        -> playerHP
                                                // # Enemy�� �ڽ� ������Ʈ �� CurrentHP         -> targetHP
    public TextMeshProUGUI targetName;          // # Enemy�� �ڽ� ������Ʈ �� TargetName
    public GameObject targetUI, playerUI;       // # Enemy      -> targetUI
                                                // # Player     -> playerUI
    public Image QSkillCool, ESkillCool;        // # QSkillCool -> Qcool    ESkillCool -> Ecool
    
    private I_Actor player_ac, target_ac;
    private I_Playable player_playable;
    private GameObject target_obj;


	private void OnEnable()
	{
		playerUI.SetActive(false);
        targetUI.SetActive(false);
	}

	// Ÿ�� UI �¿���.
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
    public void playerHealth(GameObject ctrl) { playerUI.SetActive(true);  player_ac = ctrl.GetComponent<I_Actor>(); player_playable = ctrl.GetComponent<I_Playable>(); }
    // ��� ĳ���͵��� �����ִ� �������� ��. I_Actor�� �޾ƿ� ������ �ּ�ȭ.


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
        { 
            playerHP.fillAmount = Mathf.Round((float)player_ac.GetHealth() / player_ac.GetMaxHealth() * 100) / 100;
            QSkillCool.color = new Color32(0, 0, 0, (byte)(player_playable.IsCanUseQ() ? 0 : 150));
            ESkillCool.color = new Color32(0, 0, 0, (byte)(player_playable.IsCanUseE() ? 0 : 150));
        }
    }
}
