using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using static MoonHeader;

// 플레이어 스크립트
// TopView에서의 플레이어 컨트롤
public class LSM_PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isMainPlayer;               // 현재 게임중인 플레이어인지 확인.
    public string playerName;               // 멀티에서의 플레이어 이름
                                            // # 이름 설정
    public MoonHeader.S_PlayerState player;   // 플레이어 상태에 대한 구조체
    const float MapCamBaseSize = 60;        // TopView 카메라의 OrthogonalSize

    public GameObject mySpawner;            // 팀의 마스터 스포너
    public Camera mapCamCamera;            // TopView에 사용되는 카메라

    // TopView에서의 이동속도 초기화
    private float wheelSpeed = 15f;
    private float map_move = 90f;
    private bool is_zoomIn;                 // 선택한 미니언에게 확대하고 있는지
    private IEnumerator zoomIn;             // StopCorutine을 사용하기위해 미리 선언.

    public GameObject MainCam, MapCam, MapSubCam, MiniMapCam;       // 플레이어 오브젝트 내에 존재하는 카메라들.
                                                                    
    public Vector3 mapCamBasePosition;                  // TopView카메라의 초기위치
                                                        // # Y축만 95로 설정
    public GameObject minionStatsPannel, minionStatsPannel_SelectButton;                // 플레이어가 선택한 미니언의 스탯을 표기해주는 UI
                                                        // # Canvas의 자식 오브젝트 중 MinionStatpanel
    private LSM_MinionCtrl subTarget_minion;            // 타겟으로 지정한 미니언의 스크립트
    private I_Actor subTarget_Actor;

    private TextMeshProUGUI minionStatsPannel_txt;      // 미니언 스탯을 표기하는 UI - 그 중 텍스트.
    private GameObject playerMinion;                    // 플레이어가 선택한 미니언.
    //private PSH_PlayerFPSCtrl playerMinionCtrl;         // 플레이어 미니언의 스크립트
    private GameObject playerWatchingTarget;            // 플레이어미니언이 보는 미니언 혹은 여러가지.

    private GameObject mapcamSub_Target, mapsubcam_target;  // TopView카메라의 타겟 저장과 메인카메라의 타겟 저장

    private int exp;

    public float this_player_ping;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerName);
            stream.SendNext(player.team);
            //stream.SendNext(player.statep);
            stream.SendNext(exp);
        }
        else
        {
            this.playerName = (string)stream.ReceiveNext();
            this.player.team = (MoonHeader.Team)stream.ReceiveNext();
            //this.player.statep = (MoonHeader.State_P)stream.ReceiveNext();
            this.exp = (int)stream.ReceiveNext();
            this_player_ping = (float)(PhotonNetwork.Time - info.SentServerTime);
        }
    }

    private void Awake()
    {
        
        if(mySpawner == null)
        {
            mySpawner = GameObject.Find("Spawner");
        }
        if (photonView.IsMine)
        {
            //LSM_PlayerCtrl.LocalPlayerInstance = this.gameObject;
        }

    }

    public void Start_fuction()
	{
        if(isMainPlayer)
        { 
            MapCam = GameObject.FindGameObjectWithTag("MapCamera");
            MainCam = GameObject.FindGameObjectWithTag("MainCamera");
            MiniMapCam = GameObject.FindGameObjectWithTag("MiniMapCamera");
            MapSubCam = GameObject.FindGameObjectWithTag("SubCamera");

            mapCamCamera = MapCam.GetComponent<Camera>();
            zoomIn = ZoomInMinion();
            is_zoomIn = false;
            MapCam.transform.position = mapCamBasePosition;
            MapCam.GetComponent<Camera>().orthographicSize = MapCamBaseSize;

            minionStatsPannel = GameObject.Find("MinionStatPanel");


            if (minionStatsPannel != null)
            {
                minionStatsPannel_SelectButton = minionStatsPannel.GetComponentInChildren<Button>().transform.gameObject;
                minionStatsPannel.SetActive(false);
                minionStatsPannel_txt = minionStatsPannel.GetComponentInChildren<TextMeshProUGUI>();
                
            }
                
            playerWatchingTarget = null;
            player.statep = MoonHeader.State_P.None;
            
            if (photonView.IsMine)
            { MapCam.SetActive(true); MapSubCam.SetActive(true); MainCam.SetActive(false);MiniMapCam.SetActive(false); }
            // 모든 스포너를 받아온 후 팀에 해당하는 스포너를 받아옴. 한개밖에 없다는 가정으로 하나의 마스터스포너를 받아옴.
            GameObject[] dummySpawners = GameObject.FindGameObjectsWithTag("Spawner");
            foreach (GameObject s in dummySpawners)
            {
                LSM_Spawner sSC = s.GetComponent<LSM_Spawner>();
                if (sSC.team == this.player.team) { mySpawner = s; break; }
            }
        }
    }
    public void SettingTeamAndName(int t, string n) { photonView.RPC("SettingTN_RPC", RpcTarget.AllBuffered, t,n); }
    [PunRPC] protected void SettingTN_RPC(int t, string n) { this.player.team = (MoonHeader.Team)t; this.playerName = n; }

	void Update()
    {
        if (isMainPlayer && GameManager.Instance.onceStart)
        {
            ClickEv();
            MapEv();
            SubMapCamMove();
            PlayerInMinion();
            debugging();
        }
    }

    private void debugging()
    {
        
    }

    // TopView때의 맵 이벤트
    private void MapEv()
    {
        // 현재 플레이어가 아무 클릭, 이동 등을 하지 않는다면
        if (player.statep == MoonHeader.State_P.None && GameManager.Instance.gameState != MoonHeader.GameState.Ending)
        {
            // 마우스 휠에 따라 확대, 축소
            float scroll = Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
            mapCamCamera.orthographicSize = Mathf.Min(MapCamBaseSize + 40, Mathf.Max(MapCamBaseSize-40, mapCamCamera.orthographicSize - scroll));

            // 방향키 이동에 따라서 맵의 이동
            Vector3 mapcampPosition = MapCam.transform.position;
            Vector3 move_f = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * map_move * Time.deltaTime;
            MapCam.transform.position = new Vector3(
                mapcampPosition.x+ move_f.x, mapcampPosition.y,
                mapcampPosition.z+ move_f.z);

            // 만약 미니언을 클릭하여 해당 미니언의 스탯을 보고있을 때 휠 또는 키보드 버튼을 클릭한다면, 클릭했던 타겟 미니언을 null로 변경. 다시 일반 상태로 변경됨.
            if (!ReferenceEquals(mapcamSub_Target, null) && (scroll != 0 || move_f != Vector3.zero))
            {
                //subTarget_minion.ChangeTeamColor();
                subTarget_Actor.ChangeTeamColor();
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
            if (!playerMinion.activeSelf)
            {
                StartCoroutine(AttackPathSelectSetting());
                Cursor.lockState = CursorLockMode.None;
            }
            // 플레이어 미니언이 살아있을 경우 아래 구문이 실행.
            else
            {
                // 미니맵캠을 플레이어 위치로 이동.
                MiniMapCam.transform.position = Vector3.Scale(playerMinion.transform.position, Vector3.one-Vector3.up) + Vector3.up*95;

                // 메인카메라를 기준. 메인카메라가 보고있는 방향으로 레이를 쏴, 미니언 혹은 플레이어, 터렛 등을 식별.
                // 이후 게임UI에 정보를 전달.
                RaycastHit hit;
                Debug.DrawRay(MainCam.transform.position, MainCam.transform.forward * 10, Color.green, 0.1f);
                if (Physics.Raycast(MainCam.transform.position, MainCam.transform.forward, out hit, 10, 1 << LayerMask.NameToLayer("Minion") | 1 << LayerMask.NameToLayer("Turret")))
                {
                    //Debug.Log("Player Searching! : " +hit.transform.name);
                    if (!ReferenceEquals(hit.transform.gameObject,playerWatchingTarget))
                    {
                        playerWatchingTarget = hit.transform.gameObject;
                        GameManager.Instance.gameUI_SC.enableTargetUI(true, playerWatchingTarget);
                    }
                }
                else if (!ReferenceEquals(playerWatchingTarget, null))
                {
                    playerWatchingTarget = null;
                    GameManager.Instance.gameUI_SC.enableTargetUI(false);
                }

            }
        }
    }

    // 플레이어의 현 상태를 리셋. 공격로 선택턴이 시작하였을때 사용
    public IEnumerator AttackPathSelectSetting()
    {
        if (isMainPlayer)
        {
            player.statep = MoonHeader.State_P.None;
            yield return StartCoroutine(GameManager.Instance.ScreenFade(false));
            playerMinion = null;
            MainCam.SetActive(false);
            MiniMapCam.SetActive(false);
            MapCam.SetActive(true);
            MapCam.transform.position = mapCamBasePosition;
            mapCamCamera.orthographicSize = MapCamBaseSize;
            MapSubCam.SetActive(true);
            is_zoomIn = false;
            //subTarget_minion = null;
            subTarget_Actor = null;
            GameManager.Instance.mapUI.SetActive(true);
            StartCoroutine(GameManager.Instance.ScreenFade(true));
        }
    }

    // TopView에서의 클릭 이벤트
    private void ClickEv()
    {
        // 미니언을 처음 클릭할 경우
        if (Input.GetMouseButtonDown(0) && player.statep == MoonHeader.State_P.None)
        {
            Ray ray = mapCamCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.green, 3f);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, 1 << LayerMask.NameToLayer("Icon")))
            {
                Debug.Log(hit.transform.name + " : " +hit.transform.tag);

                switch (GameManager.Instance.gameState)
                {
                    // 현재 게임이 공격로 지정 턴이라면
                    case MoonHeader.GameState.SettingAttackPath:      
                        break;

                    // 게임 중 플레이어가 미니언 마크를 클릭했다면,
                    case MoonHeader.GameState.Gaming:
                        // 미니언, 플레이어, 포탑 제외 나머지를 클릭시 실행 안됨.
                        //if (!hit.transform.CompareTag("Minion") && !hit.transform.CompareTag("PlayerMinion") && !hit.transform.CompareTag("Turret") && !hit.transform.CompareTag("Nexus")) { return; }
                        I_Actor dummy = hit.transform.GetComponentInParent<I_Actor>();
                        if (ReferenceEquals(dummy, null)) { return; }

                        // 현재 미니언이 클릭되어 있으나, 다른 미니언을 클릭하였다면, 전에 클릭했던 미니언의 아이콘을 원래 상태로 복구
                        if (!ReferenceEquals(mapcamSub_Target, null) && !ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                            subTarget_Actor.ChangeTeamColor();
                        // 클릭된 미니언에 대하여.. 카메라의 위치를 이동 및 고정. 천천히 줌인하는 코루틴 실행
                        if (ReferenceEquals(mapcamSub_Target, null) || (!ReferenceEquals(mapcamSub_Target, null) &&
                            !ReferenceEquals(mapcamSub_Target, hit.transform.gameObject)))
                        {
                            is_zoomIn = true;
                            //subTarget_Actor = hit.transform.GetComponent<I_Actor>();
                            subTarget_Actor = dummy;

                            mapcamSub_Target = hit.transform.gameObject;
                            mapsubcam_target = subTarget_Actor.GetCameraPos();
                            minionStatsPannel.SetActive(false);
                            StopCoroutine(zoomIn);
                            zoomIn = ZoomInMinion();
                            StartCoroutine(zoomIn);
                            subTarget_Actor.Selected();
                        }

                        /*
                        if (hit.transform.CompareTag("Minion"))
                        {
                            // 현재 미니언이 클릭되어 있으나, 다른 미니언을 클릭하였다면, 전에 클릭했던 미니언의 아이콘을 원래 상태로 복구
                            if (!ReferenceEquals(mapcamSub_Target, null) && !ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                            {
                                //if (!ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                                    subTarget_minion.ChangeTeamColor();
                            }

                            // 클릭된 미니언에 대하여.. 카메라의 위치를 이동 및 고정. 천천히 줌인하는 코루틴 실행
                            if (ReferenceEquals(mapcamSub_Target, null) || (!ReferenceEquals(mapcamSub_Target, null)&& 
                                !ReferenceEquals(mapcamSub_Target,hit.transform.gameObject))) {
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
                        */
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
                //minionStatsPannel_SelectButton.SetActive(subTarget_minion.stats.actorHealth.team == this.player.team);
                minionStatsPannel_SelectButton.SetActive(subTarget_Actor.GetActor().team == this.player.team && mapcamSub_Target.CompareTag("Minion"));
                // 미니언 미리보기 창 글씨.
                //minionStatsPannel_txt.text = string.Format("Minion : {0}\nHealth : {1}\nATK : {2}",
                                //subTarget_minion.stats.actorHealth.type, subTarget_minion.stats.actorHealth.health, subTarget_minion.stats.actorHealth.Atk);

                minionStatsPannel_txt.text = string.Format("Type : {0}\nHealth : {1}\nATK : {2}",
                                subTarget_Actor.GetActor().type, subTarget_Actor.GetActor().health, subTarget_Actor.GetActor().Atk);

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

        /*for (int i = 0; i < 50; i++)
        {
            MapCam.transform.position = Vector3.Lerp(originV, (mapcamSub_Target.transform.position + Vector3.up * 95), 0.02f * i);
            mapCamCamera.orthographicSize = Mathf.Lerp(originSize, 20, 0.02f * i);
            yield return new WaitForSeconds(0.01f);
        }*/

        
        while (true)
        {
            if (ReferenceEquals(mapcamSub_Target, null))
                break;
            Vector3 targetPosition = mapcamSub_Target.transform.position + Vector3.up * 95;
            MapCam.transform.position = Vector3.MoveTowards(MapCam.transform.position,
                targetPosition, map_move * 2 * Time.deltaTime);
            mapCamCamera.orthographicSize = (mapCamCamera.orthographicSize > 20) ?
                mapCamCamera.orthographicSize - map_move * Time.deltaTime : 20;
            yield return new WaitForSeconds(Time.deltaTime);
            if (Vector3.Distance(MapCam.transform.position, targetPosition) <= 5 && mapCamCamera.orthographicSize <= 20)
                break;
        }
        is_zoomIn=false;
        minionStatsPannel.SetActive(true);
    }

    // select버튼 클릭 시 
    public void SelectPlayerMinion()
    {
        if (ReferenceEquals(mapcamSub_Target, null) || ReferenceEquals(mapcamSub_Target.GetComponent<LSM_MinionCtrl>(), null)) { return; }

        LSM_MinionCtrl dummy_minion = mapcamSub_Target.GetComponent<LSM_MinionCtrl>();

        
        if (player.statep == MoonHeader.State_P.None && dummy_minion.stats.state != MoonHeader.State.Dead &&
            dummy_minion.stats.actorHealth.team == this.player.team)
        {
            player.statep = MoonHeader.State_P.Possession;
            dummy_minion.PlayerConnect();
            playerMinion = dummy_minion.transform.gameObject;
            StartCoroutine(ZoomPossession());
        }
    }

    // 빙의 코루틴
    private IEnumerator ZoomPossession()
    {
        StartCoroutine(GameManager.Instance.ScreenFade(false));
        float originSize = mapCamCamera.orthographicSize;


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
        MiniMapCam.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        // 기존의 미니언을 비활성화한 후 플레이어 전용 프리펩 소환.

        playerMinion = PoolManager.Instance.Get_PlayerMinion(0);
        //playerMinion = GameObject.Instantiate(PrefabManager.Instance.players[0],PoolManager.Instance.transform);



        //playerMinionCtrl = playerMinion.GetComponent<PSH_PlayerFPSCtrl>();

        // 카메라 지정. 및 초기세팅
        //PSH_PlayerFPSCtrl player_dummy = playerMinion.GetComponent<PSH_PlayerFPSCtrl>();
        I_Playable player_dummy = playerMinion.GetComponent<I_Playable>();
        player_dummy.SpawnSetting(player.team, subTarget_Actor.GetActor().health, playerName, this.GetComponent<LSM_PlayerCtrl>());
        //player_dummy.playerCamera = MainCam.GetComponent<Camera>();
        mapsubcam_target = player_dummy.CameraSetting(MainCam);
        GameManager.Instance.gameUI.SetActive(true);
        GameManager.Instance.gameUI_SC.playerHealth(playerMinion);

        Vector3 dummyPosition = mapcamSub_Target.transform.position;
        Quaternion dummyRotation = mapcamSub_Target.transform.rotation;
        mapcamSub_Target.GetComponent<LSM_MinionCtrl>().MinionDisable();

        playerMinion.transform.position = dummyPosition;
        playerMinion.transform.rotation = dummyRotation;

        StartCoroutine(GameManager.Instance.ScreenFade(true));
        player.statep = MoonHeader.State_P.Selected;
        Cursor.lockState = CursorLockMode.Locked;

        yield return new WaitForSeconds(3f);

        //subTarget_minion.stats.state = MoonHeader.State.Normal;
    }

    public void PlayerMinionDeadProcessing()
    {
        if (isMainPlayer) {
            Cursor.lockState = CursorLockMode.None;
            GameManager.Instance.gameUI.SetActive(false);
        }
    }

    public void GetExp(int exp_dummy)
    {
        exp += exp_dummy;
    }
}
