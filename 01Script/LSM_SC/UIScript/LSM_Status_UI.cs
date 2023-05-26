using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LSM_Status_UI : MonoBehaviour
{
    private Camera map_cam;
    private PointerEventData pe_;
    private GraphicRaycaster _gr;
    private List<RaycastResult> _rrList;

    [SerializeField] private GameObject[] icons;
    [SerializeField] private TextMeshProUGUI playerName, job, level, kd, hp, atk, gold, exp;

    [SerializeField]private GameObject tooltip_pannel;
    private LSM_Pannel_Tooltip tooltip;

    private LSM_PlayerCtrl playerCtrl;

    private void Awake()
    {
        pe_ = new PointerEventData(null);
        _gr= GetComponent<GraphicRaycaster>();
        _rrList= new List<RaycastResult>();
        tooltip = tooltip_pannel.GetComponent<LSM_Pannel_Tooltip>();
    }
    private void Update()
    {
        pe_.position = Input.mousePosition;

        if (map_cam == null && GameManager.Instance.onceStart)
        {
            map_cam = GameManager.Instance.mainPlayer.MapCam.GetComponent<Camera>();
            playerCtrl = GameManager.Instance.mainPlayer;
        }
        else if (GameManager.Instance.onceStart)
        {
            this.transform.SetAsLastSibling();
            SettingPlayerInfo();
            SettingTooltipPannel();
        }
    }

    private void SettingPlayerInfo()
    {
        byte t = playerCtrl.PlayerType;
        for (int i = 0; i < icons.Length; i++) { icons[i].SetActive(t == i); }
        playerName.text = playerCtrl.playerName;
        job.text = ((MoonHeader.ActorType)t).ToString();
        level.text = playerCtrl.GetLevel().ToString() + "LV";
        kd.text = string.Format("Kill {0} / Death {1}", playerCtrl.kd[0], playerCtrl.kd[1]);
        object[] o_d = LSM_SettingStatus.Instance.lvStatus[(int)t].getStatus_LV(playerCtrl.GetLevel());
        short[] add = GameManager.Instance.teamManagers[(int)playerCtrl.player.team].GetAtkHp();

        hp.text = "HP : "+o_d[0].ToString() + add[0];
        atk.text = "ATK : "+o_d[1].ToString() + add[1];
        gold.text = playerCtrl.GetGold().ToString();
        exp.text = playerCtrl.GetExp().ToString();
    }

    private void SettingTooltipPannel()
    {
        _rrList.Clear();
        _gr.Raycast(pe_, _rrList);
        LSM_UI_Tooltip tooltip_d;


        if (_rrList.Count <= 0)
        { tooltip_pannel.SetActive(false); return; }

        if ((tooltip_d = _rrList[0].gameObject.GetComponent<LSM_UI_Tooltip>()) != null)
        {
            tooltip_pannel.SetActive(true);

            tooltip.SetRectPosition(_rrList[0].gameObject.GetComponent<RectTransform>());
            tooltip.SettingContents(tooltip_d.tooltips[(int)playerCtrl.PlayerType]);
        }
    }
}