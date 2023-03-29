using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

// 플레이어 스크립트
// TopView에서의 플레이어 컨트롤
public class LSM_PlayerCtrl : MonoBehaviour
{
    public string playerName;               // 멀티에서의 플레이어 이름
    public MoonHeader.S_PlayerState player;   // 플레이어 상태에 대한 구조체
    const float MapCamBaseSize = 40;        // TopView 카메라의 OrthogonalSize

    public GameObject mySpawner;            // 팀의 마스터 스포너
    private Camera mapCamCamera;            // TopView에 사용되는 카메라

    // TopView에서의 이동속도 초기화
    private float wheelSpeed = 15f;
    private float map_move = 1f;
    private bool is_zoomIn;                 // 선택한 미니언에게 확대하고 있는지
    private IEnumerator zoomIn;             // StopCorutine을 사용하기위해 미리 선언.

    public GameObject MainCam, MapCam, MapSubCam;       // 플레이어 오브젝트 내에 존재하는 카메라들.
    public Vector3 mapCamBasePosition;                  // TopView카메라의 초기위치
    public GameObject minionStatsPannel;                // 플레이어가 선택한 미니언의 스탯을 표기해주는 UI
    private LSM_MinionCtrl subTarget_minion;            // 타겟으로 지정한 미니언의 스크립트
    private TextMeshProUGUI minionStatsPannel_txt;      // 미니언 스탯을 표기하는 UI - 그 중 텍스트.
    private GameObject playerMinion;                    // 플레이어가 선택한 미니언.

    private GameObject mapcamSub_Target, mapsubcam_target;  // TopView카메라의 타겟 저장과 메인카메라의 타겟 저장
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

    // TopView때의 맵 이벤트
    private void MapEv()
    {
        // 현재 플레이어가 아무 클릭, 이동 등을 하지 않는다면
        if (player.statep == MoonHeader.State_P.None)
        {
            // 마우스 휠에 따라 확대, 축소
            float scroll = Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
            mapCamCamera.orthographicSize = Mathf.Min(60, Mathf.Max(15, mapCamCamera.orthographicSize - scroll));

            // 방향키 이동에 따라서 맵의 이동
            Vector3 mapcampPosition = MapCam.transform.position;
            Vector3 move_f = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * map_move;
            MapCam.transform.position = new Vector3(
                mapcampPosition.x+ move_f.x, mapcampPosition.y,
                mapcampPosition.z+ move_f.z);

            // 만약 미니언을 클릭하여 해당 미니언의 스탯을 보고있을 때 휠 또는 키보드 버튼을 클릭한다면, 클릭했던 타겟 미니언을 null로 변경. 다시 일반 상태로 변경됨.
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
            // 플레이어가 선택한 상태였으나, 플레이어의 미니언이 사라졋다면 초기화
            if (ReferenceEquals(playerMinion,null))
            {
                Debug.Log("Minion active false");
                StartCoroutine(AttackPathSelectSetting());

            }
        }
    }

    // 플레이어의 현 상태를 리셋. 공격로 선택턴이 시작하였을때 사용
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

    // TopView에서의 클릭 이벤트
    private void ClickEv()
    {
        // 미니언을 처음 클릭할 경우
        if (Input.GetMouseButtonDown(0) && player.statep == MoonHeader.State_P.None)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 3f);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log(hit.transform.name + " : " +hit.transform.tag);

                switch (GameManager.Instance.gameState)
                {
                    // 현재 게임이 공격로 지정 턴이라면
                    case MoonHeader.GameState.SettingAttackPath:      
                        break;

                    // 게임 중 플레이어가 미니언 마크를 클릭했다면,
                    case MoonHeader.GameState.Gaming:
                        
                        if (hit.transform.CompareTag("Minion"))
                        {
                            // 현재 미니언이 클릭되어 있으나, 다른 미니언을 클릭하였다면, 전에 클릭했던 미니언의 아이콘을 원래 상태로 복구
                            if (!ReferenceEquals(mapcamSub_Target, null) && !ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                            {
                                //if (!ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                                    subTarget_minion.ChangeTeamColor();
                            }
                            // 클릭된 미니언에 대하여.. 카메라의 위치를 이동 및 고정. 천천히 줌인하는 코루틴 실행
                            else if (//ReferenceEquals(mapcamSub_Target, null) || (!ReferenceEquals(mapcamSub_Target, null)&& 
                                !ReferenceEquals(mapcamSub_Target,hit.transform.gameObject)) {
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

    // TopView 상태에서 클릭한 미니언의 시점을 담당하는 카메라에 대한 함수
    private void SubMapCamMove()
    {
        if (!ReferenceEquals(mapcamSub_Target, null) && !is_zoomIn){
            // 미니언 확대 중 죽을 경우의 예외 처리도 포함.
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

    // 맵에서 해당 미니언에게 천천히 가까워지는 코드
    // 현재 Lerp를 사용하였지만, 속도에 따라 이동하게 설정할지 고민.
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
