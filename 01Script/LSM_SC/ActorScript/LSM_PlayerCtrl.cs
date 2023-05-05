using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using static MoonHeader;

// �÷��̾� ��ũ��Ʈ
// TopView������ �÷��̾� ��Ʈ��
public class LSM_PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isMainPlayer;               // ���� �������� �÷��̾����� Ȯ��.
    public string playerName;               // ��Ƽ������ �÷��̾� �̸�
                                            // # �̸� ����
    public MoonHeader.S_PlayerState player;   // �÷��̾� ���¿� ���� ����ü
    const float MapCamBaseSize = 60;        // TopView ī�޶��� OrthogonalSize

    public GameObject mySpawner;            // ���� ������ ������
    public Camera mapCamCamera;            // TopView�� ���Ǵ� ī�޶�

    // TopView������ �̵��ӵ� �ʱ�ȭ
    private float wheelSpeed = 15f;
    private float map_move = 90f;
    private bool is_zoomIn;                 // ������ �̴Ͼ𿡰� Ȯ���ϰ� �ִ���
    private IEnumerator zoomIn;             // StopCorutine�� ����ϱ����� �̸� ����.

    public GameObject MainCam, MapCam, MapSubCam, MiniMapCam;       // �÷��̾� ������Ʈ ���� �����ϴ� ī�޶��.
                                                                    
    public Vector3 mapCamBasePosition;                  // TopViewī�޶��� �ʱ���ġ
                                                        // # Y�ุ 95�� ����
    public GameObject minionStatsPannel, minionStatsPannel_SelectButton;                // �÷��̾ ������ �̴Ͼ��� ������ ǥ�����ִ� UI
                                                        // # Canvas�� �ڽ� ������Ʈ �� MinionStatpanel
    private LSM_MinionCtrl subTarget_minion;            // Ÿ������ ������ �̴Ͼ��� ��ũ��Ʈ
    private I_Actor subTarget_Actor;

    private TextMeshProUGUI minionStatsPannel_txt;      // �̴Ͼ� ������ ǥ���ϴ� UI - �� �� �ؽ�Ʈ.
    private GameObject playerMinion;                    // �÷��̾ ������ �̴Ͼ�.
    //private PSH_PlayerFPSCtrl playerMinionCtrl;         // �÷��̾� �̴Ͼ��� ��ũ��Ʈ
    private GameObject playerWatchingTarget;            // �÷��̾�̴Ͼ��� ���� �̴Ͼ� Ȥ�� ��������.

    private GameObject mapcamSub_Target, mapsubcam_target;  // TopViewī�޶��� Ÿ�� ����� ����ī�޶��� Ÿ�� ����

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
            // ��� �����ʸ� �޾ƿ� �� ���� �ش��ϴ� �����ʸ� �޾ƿ�. �Ѱ��ۿ� ���ٴ� �������� �ϳ��� �����ͽ����ʸ� �޾ƿ�.
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

    // TopView���� �� �̺�Ʈ
    private void MapEv()
    {
        // ���� �÷��̾ �ƹ� Ŭ��, �̵� ���� ���� �ʴ´ٸ�
        if (player.statep == MoonHeader.State_P.None && GameManager.Instance.gameState != MoonHeader.GameState.Ending)
        {
            // ���콺 �ٿ� ���� Ȯ��, ���
            float scroll = Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
            mapCamCamera.orthographicSize = Mathf.Min(MapCamBaseSize + 40, Mathf.Max(MapCamBaseSize-40, mapCamCamera.orthographicSize - scroll));

            // ����Ű �̵��� ���� ���� �̵�
            Vector3 mapcampPosition = MapCam.transform.position;
            Vector3 move_f = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * map_move * Time.deltaTime;
            MapCam.transform.position = new Vector3(
                mapcampPosition.x+ move_f.x, mapcampPosition.y,
                mapcampPosition.z+ move_f.z);

            // ���� �̴Ͼ��� Ŭ���Ͽ� �ش� �̴Ͼ��� ������ �������� �� �� �Ǵ� Ű���� ��ư�� Ŭ���Ѵٸ�, Ŭ���ߴ� Ÿ�� �̴Ͼ��� null�� ����. �ٽ� �Ϲ� ���·� �����.
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

    // �÷��̾ �ش� �̴Ͼ� ����/���� �ϰ��ִٸ� ���� 
    private void PlayerInMinion()
    {
        if (player.statep == MoonHeader.State_P.Selected)
        {
            //MainCam.transform.position = mapsubcam_target.transform.position;
            //MainCam.transform.rotation = mapsubcam_target.transform.rotation;
            // �÷��̾ ������ ���¿�����, �÷��̾��� �̴Ͼ��� ��󠺴ٸ� �ʱ�ȭ
            if (!playerMinion.activeSelf)
            {
                StartCoroutine(AttackPathSelectSetting());
                Cursor.lockState = CursorLockMode.None;
            }
            // �÷��̾� �̴Ͼ��� ������� ��� �Ʒ� ������ ����.
            else
            {
                // �̴ϸ�ķ�� �÷��̾� ��ġ�� �̵�.
                MiniMapCam.transform.position = Vector3.Scale(playerMinion.transform.position, Vector3.one-Vector3.up) + Vector3.up*95;

                // ����ī�޶� ����. ����ī�޶� �����ִ� �������� ���̸� ��, �̴Ͼ� Ȥ�� �÷��̾�, �ͷ� ���� �ĺ�.
                // ���� ����UI�� ������ ����.
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

    // �÷��̾��� �� ���¸� ����. ���ݷ� �������� �����Ͽ����� ���
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

    // TopView������ Ŭ�� �̺�Ʈ
    private void ClickEv()
    {
        // �̴Ͼ��� ó�� Ŭ���� ���
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
                    // ���� ������ ���ݷ� ���� ���̶��
                    case MoonHeader.GameState.SettingAttackPath:      
                        break;

                    // ���� �� �÷��̾ �̴Ͼ� ��ũ�� Ŭ���ߴٸ�,
                    case MoonHeader.GameState.Gaming:
                        // �̴Ͼ�, �÷��̾�, ��ž ���� �������� Ŭ���� ���� �ȵ�.
                        //if (!hit.transform.CompareTag("Minion") && !hit.transform.CompareTag("PlayerMinion") && !hit.transform.CompareTag("Turret") && !hit.transform.CompareTag("Nexus")) { return; }
                        I_Actor dummy = hit.transform.GetComponentInParent<I_Actor>();
                        if (ReferenceEquals(dummy, null)) { return; }

                        // ���� �̴Ͼ��� Ŭ���Ǿ� ������, �ٸ� �̴Ͼ��� Ŭ���Ͽ��ٸ�, ���� Ŭ���ߴ� �̴Ͼ��� �������� ���� ���·� ����
                        if (!ReferenceEquals(mapcamSub_Target, null) && !ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                            subTarget_Actor.ChangeTeamColor();
                        // Ŭ���� �̴Ͼ� ���Ͽ�.. ī�޶��� ��ġ�� �̵� �� ����. õõ�� �����ϴ� �ڷ�ƾ ����
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
                            // ���� �̴Ͼ��� Ŭ���Ǿ� ������, �ٸ� �̴Ͼ��� Ŭ���Ͽ��ٸ�, ���� Ŭ���ߴ� �̴Ͼ��� �������� ���� ���·� ����
                            if (!ReferenceEquals(mapcamSub_Target, null) && !ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                            {
                                //if (!ReferenceEquals(mapcamSub_Target, hit.transform.gameObject))
                                    subTarget_minion.ChangeTeamColor();
                            }

                            // Ŭ���� �̴Ͼ� ���Ͽ�.. ī�޶��� ��ġ�� �̵� �� ����. õõ�� �����ϴ� �ڷ�ƾ ����
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

    // TopView ���¿��� Ŭ���� �̴Ͼ��� ������ ����ϴ� ī�޶� ���� �Լ�
    private void SubMapCamMove()
    {
        if (!ReferenceEquals(mapcamSub_Target, null) && !is_zoomIn){
            // �̴Ͼ� Ȯ�� �� ���� ����� ���� ó���� ����.
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
                // �̴Ͼ� �̸����� â �۾�.
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

    // �ʿ��� �ش� �̴Ͼ𿡰� õõ�� ��������� �ڵ�
    // ���� Lerp�� ����Ͽ�����, �ӵ��� ���� �̵��ϰ� �������� ���.
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

    // select��ư Ŭ�� �� 
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

    // ���� �ڷ�ƾ
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
        // ī�޶� ����ī�޶� ���� ��� ����.
        GameManager.Instance.mapUI.SetActive(false);
        mapCamCamera.transform.gameObject.SetActive(false);
        MapSubCam.transform.gameObject.SetActive(false);
        MainCam.SetActive(true);
        MiniMapCam.SetActive(true);

        yield return new WaitForSeconds(0.5f);
        // ������ �̴Ͼ��� ��Ȱ��ȭ�� �� �÷��̾� ���� ������ ��ȯ.

        playerMinion = PoolManager.Instance.Get_PlayerMinion(0);
        //playerMinion = GameObject.Instantiate(PrefabManager.Instance.players[0],PoolManager.Instance.transform);



        //playerMinionCtrl = playerMinion.GetComponent<PSH_PlayerFPSCtrl>();

        // ī�޶� ����. �� �ʱ⼼��
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
