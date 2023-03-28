using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class LSM_PlayerCtrl : MonoBehaviour
{
    public string playerName;
    public MoonHeader.PlayerState player;
    private float MapCamBaseSize = 40;

    public GameObject mySpawner;
    private Camera mapCamCamera;

    private float wheelSpeed = 15f;
    private float map_move = 1f;
    private bool is_zoomIn;
    private IEnumerator zoomIn;

    public GameObject MainCam, MapCam, MapSubCam;
    public Vector3 mapCamBasePosition;
    public GameObject minionStatsPannel;
    private LSM_MinionCtrl subTarget_minion;
    private TextMeshProUGUI minionStatsPannel_txt;
    private GameObject playerMinion;

    private GameObject mapcamSub_Target, mapsubcam_target;
	private void Start()
	{
        mapCamCamera = MapCam.GetComponent<Camera>();
        zoomIn = ZoomInMinion();
        is_zoomIn = false;
        MapCam.transform.position = mapCamBasePosition;
        MapCam.GetComponent<Camera>().orthographicSize = MapCamBaseSize;
        minionStatsPannel.SetActive(false);
        minionStatsPannel_txt = minionStatsPannel.GetComponentInChildren<TextMeshProUGUI>();
        player.statep = MoonHeader.State_P.None;

    }

	void Update()
    {
        ClickEv();
        MapEv();
        SubMapCamMove();
        PlayerInMinion();
    }

    private void MapEv()
    {
        if (player.statep == MoonHeader.State_P.None)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
            mapCamCamera.orthographicSize = Mathf.Min(60, Mathf.Max(15, mapCamCamera.orthographicSize - scroll));

            Vector3 mapcampPosition = MapCam.transform.position;
            Vector3 move_f = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * map_move;
            MapCam.transform.position = new Vector3(
                mapcampPosition.x+ move_f.x, mapcampPosition.y,
                mapcampPosition.z+ move_f.z);

            if (!ReferenceEquals(mapcamSub_Target, null) && (scroll != 0 || move_f != Vector3.zero))
            {
                subTarget_minion.ChangeTeamColor();
                mapcamSub_Target = null;
                is_zoomIn = false;
                StopCoroutine(zoomIn);
                minionStatsPannel.SetActive(false);
            }
        }
    }

    // 플레이어가 해당 미니언에 빙의/강림 하고있다면 실행 
    private void PlayerInMinion()
    {
        if (player.statep == MoonHeader.State_P.Selected)
        {
            //MainCam.transform.position = mapsubcam_target.transform.position;
            //MainCam.transform.rotation = mapsubcam_target.transform.rotation;

            if (ReferenceEquals(playerMinion,null))
            {
                Debug.Log("Minion active false");
                StartCoroutine(AttackPathSelectSetting());

            }
        }
    }

    public IEnumerator AttackPathSelectSetting()
    {
        player.statep = MoonHeader.State_P.None;
        yield return StartCoroutine(GameManager.Instance.ScreenFade(false));
        playerMinion = null;
        MainCam.SetActive(false);
        MapCam.SetActive(true);
        MapCam.transform.position = mapCamBasePosition;
        mapCamCamera.orthographicSize = MapCamBaseSize;
        MapSubCam.SetActive(true);
        is_zoomIn = false;
        subTarget_minion = null;
        GameManager.Instance.mapUI.SetActive(true);
        StartCoroutine(GameManager.Instance.ScreenFade(true));
    }

    private void ClickEv()
    {
        if (Input.GetMouseButtonDown(0) && player.statep == MoonHeader.State_P.None)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 3f);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.transform.name + " : " +hit.transform.tag);

                switch (GameManager.Instance.gameState)
                {
                    // 현재 게임이 공격로 지정 턴이라면
                    case MoonHeader.GameState.SettingAttackPath:

                        
                        break;

                    // 게임 중 플레이어가 미니언 마크를 클릭했다면,
                    case MoonHeader.GameState.Gaming:
                        
                        if (hit.transform.CompareTag("Minion"))
                        {
                            if (!ReferenceEquals(mapcamSub_Target, null))
                            {
                                if (!ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                                    subTarget_minion.ChangeTeamColor();
                            }

                            if (ReferenceEquals(mapcamSub_Target, null) || 
                                (!ReferenceEquals(mapcamSub_Target, null) && !ReferenceEquals(mapcamSub_Target,hit.transform.gameObject))) {
                                is_zoomIn = true;
                                subTarget_minion = hit.transform.GetComponent<LSM_MinionCtrl>();
                                mapcamSub_Target = hit.transform.gameObject;
                                mapsubcam_target = subTarget_minion.CameraPosition;
                                minionStatsPannel.SetActive(false);
                                StopCoroutine(zoomIn);
                                zoomIn = ZoomInMinion();
                                StartCoroutine(zoomIn);
                                //subTarget_minion.icon.GetComponent<Renderer>().material.color = Color.green;
                                subTarget_minion.PlayerSelected();
                            }

                            //GameManager.Instance.MapCam.GetComponent<Camera>().orthographicSize = 20;
                        }
                        break;
                }

            }
        }
    }

    // TopView 상태의 미리보기창 카메라 구문.
    private void SubMapCamMove()
    {
        if (!ReferenceEquals(mapcamSub_Target, null) && !is_zoomIn){
            if (!mapcamSub_Target.activeSelf)
            {
                mapcamSub_Target = null;
                is_zoomIn = false;
                StopCoroutine(zoomIn);
                minionStatsPannel.SetActive(false);
            }
            else
            {
                // 미니언 미리보기 창 글씨.
                minionStatsPannel_txt.text = string.Format("Minion : {0}\nHealth : {1}\nATK : {2}",
                                subTarget_minion.stats.type, subTarget_minion.stats.health, subTarget_minion.stats.Atk);
                MapCam.transform.position = (mapcamSub_Target.transform.position + Vector3.up * 95);
                MapSubCam.transform.position = mapsubcam_target.transform.position;
                MapSubCam.transform.rotation = mapsubcam_target.transform.rotation;
            }
        }
    }

    // 맵에서 해당 미니언에게 가까워지는 코드
    private IEnumerator ZoomInMinion()
    {
        
        Vector3 originV = MapCam.transform.position;
        float originSize = mapCamCamera.orthographicSize;

        for (int i = 0; i < 50; i++)
        {
            MapCam.transform.position = Vector3.Lerp(originV, (mapcamSub_Target.transform.position + Vector3.up * 95), 0.02f * i);
            mapCamCamera.orthographicSize = Mathf.Lerp(originSize, 20, 0.02f * i);
            yield return new WaitForSeconds(0.01f);
        }
        is_zoomIn=false;
        minionStatsPannel.SetActive(true);
    }

    // select버튼 클릭 시 
    public void SelectPlayerMinion()
    {
        if (!ReferenceEquals(mapcamSub_Target, null) && player.statep == MoonHeader.State_P.None)
        {
            player.statep = MoonHeader.State_P.Possession;
            subTarget_minion.PlayerConnect();
            playerMinion = subTarget_minion.transform.gameObject;
            StartCoroutine(ZoomPossession());
        }
    }

    // 빙의 코루틴
    private IEnumerator ZoomPossession()
    {
        StartCoroutine(GameManager.Instance.ScreenFade(false));
        float originSize = mapCamCamera.orthographicSize;
        subTarget_minion.stats.state = MoonHeader.State.Invincibility;

        minionStatsPannel.SetActive(false);
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(0.01f);
            mapCamCamera.orthographicSize = Mathf.Lerp(originSize, 5, 0.01f * i);
        }
        // 카메라 메인카메라 제외 모두 끄기.
        GameManager.Instance.mapUI.SetActive(false);
        mapCamCamera.transform.gameObject.SetActive(false);
        MapSubCam.transform.gameObject.SetActive(false);
        MainCam.SetActive(true);

        StartCoroutine(GameManager.Instance.ScreenFade(true));
        player.statep = MoonHeader.State_P.Selected;
        yield return new WaitForSeconds(0.5f);
        // 기존의 미니언을 비활성화한 후 플레이어 전용 프리펩 소환.
        playerMinion = GameObject.Instantiate(PrefabManager.Instance.players[0],PoolManager.Instance.transform);
        playerMinion.transform.position = subTarget_minion.transform.position;
        playerMinion.transform.rotation = subTarget_minion.transform.rotation;
        PSH_PlayerFPSCtrl player_dummy = playerMinion.GetComponent<PSH_PlayerFPSCtrl>();
        player_dummy.playerCamera = MainCam.GetComponent<Camera>();
        mapsubcam_target = player_dummy.camerapos;

        subTarget_minion.transform.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);

        //subTarget_minion.stats.state = MoonHeader.State.Normal;
    }
}
