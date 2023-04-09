using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;

// �÷��̾� ��ũ��Ʈ
// TopView������ �÷��̾� ��Ʈ��
public class LSM_PlayerCtrl : MonoBehaviourPunCallbacks
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
    private float map_move = 1f;
    private bool is_zoomIn;                 // ������ �̴Ͼ𿡰� Ȯ���ϰ� �ִ���
    private IEnumerator zoomIn;             // StopCorutine�� ����ϱ����� �̸� ����.

    public GameObject MainCam, MapCam, MapSubCam, MiniMapCam;       // �÷��̾� ������Ʈ ���� �����ϴ� ī�޶��.
                                                                    // # MainCam    -> Player �ڽ� ������Ʈ �� Camera
                                                                    // # MapCam     -> Player �ڽ� ������Ʈ �� MapCamera
                                                                    // # MapSubCam  -> Player �ڽ� ������Ʈ �� MapSubCamera
                                                                    // # MiniMapCam -> Player �ڽ� ������Ʈ �� MiniMapCam
    public Vector3 mapCamBasePosition;                  // TopViewī�޶��� �ʱ���ġ
                                                        // # Y�ุ 95�� ����
    public GameObject minionStatsPannel;                // �÷��̾ ������ �̴Ͼ��� ������ ǥ�����ִ� UI
                                                        // # Canvas�� �ڽ� ������Ʈ �� MinionStatpanel
    private LSM_MinionCtrl subTarget_minion;            // Ÿ������ ������ �̴Ͼ��� ��ũ��Ʈ
    private TextMeshProUGUI minionStatsPannel_txt;      // �̴Ͼ� ������ ǥ���ϴ� UI - �� �� �ؽ�Ʈ.
    private GameObject playerMinion;                    // �÷��̾ ������ �̴Ͼ�.
    private PSH_PlayerFPSCtrl playerMinionCtrl;         // �÷��̾� �̴Ͼ��� ��ũ��Ʈ
    private GameObject playerWatchingTarget;            // �÷��̾�̴Ͼ��� ���� �̴Ͼ� Ȥ�� ��������.

    private GameObject mapcamSub_Target, mapsubcam_target;  // TopViewī�޶��� Ÿ�� ����� ����ī�޶��� Ÿ�� ����

    private int exp;

    // �̱��� �� ����
    // public static GameObject LocalPlayerInstance;

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
            Vector3 move_f = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")) * map_move;
            MapCam.transform.position = new Vector3(
                mapcampPosition.x+ move_f.x, mapcampPosition.y,
                mapcampPosition.z+ move_f.z);

            // ���� �̴Ͼ��� Ŭ���Ͽ� �ش� �̴Ͼ��� ������ �������� �� �� �Ǵ� Ű���� ��ư�� Ŭ���Ѵٸ�, Ŭ���ߴ� Ÿ�� �̴Ͼ��� null�� ����. �ٽ� �Ϲ� ���·� �����.
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
                Debug.Log("Minion active false");
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
            subTarget_minion = null;
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

            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log(hit.transform.name + " : " +hit.transform.tag);

                switch (GameManager.Instance.gameState)
                {
                    // ���� ������ ���ݷ� ���� ���̶��
                    case MoonHeader.GameState.SettingAttackPath:      
                        break;

                    // ���� �� �÷��̾ �̴Ͼ� ��ũ�� Ŭ���ߴٸ�,
                    case MoonHeader.GameState.Gaming:
                        
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
                // �̴Ͼ� �̸����� â �۾�.
                minionStatsPannel_txt.text = string.Format("Minion : {0}\nHealth : {1}\nATK : {2}",
                                subTarget_minion.stats.type, subTarget_minion.stats.actorHealth.health, subTarget_minion.stats.actorHealth.Atk);
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

        for (int i = 0; i < 50; i++)
        {
            MapCam.transform.position = Vector3.Lerp(originV, (mapcamSub_Target.transform.position + Vector3.up * 95), 0.02f * i);
            mapCamCamera.orthographicSize = Mathf.Lerp(originSize, 20, 0.02f * i);
            yield return new WaitForSeconds(0.01f);
        }
        is_zoomIn=false;
        minionStatsPannel.SetActive(true);
    }

    // select��ư Ŭ�� �� 
    public void SelectPlayerMinion()
    {
        if (!ReferenceEquals(mapcamSub_Target, null) && player.statep == MoonHeader.State_P.None && subTarget_minion.stats.state != MoonHeader.State.Dead &&
            subTarget_minion.stats.actorHealth.team == this.player.team)
        {
            player.statep = MoonHeader.State_P.Possession;
            subTarget_minion.PlayerConnect();
            playerMinion = subTarget_minion.transform.gameObject;
            StartCoroutine(ZoomPossession());
        }
    }

    // ���� �ڷ�ƾ
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
        playerMinion.transform.position = subTarget_minion.transform.position;
        playerMinion.transform.rotation = subTarget_minion.transform.rotation;
        playerMinion.name = playerName;
        GameManager.Instance.playerMinions[(int)player.team].Add(playerMinion);
        playerMinionCtrl = playerMinion.GetComponent<PSH_PlayerFPSCtrl>();

        // ī�޶� ����. �� �ʱ⼼��
        PSH_PlayerFPSCtrl player_dummy = playerMinion.GetComponent<PSH_PlayerFPSCtrl>();
        player_dummy.SpawnSetting(player.team, subTarget_minion.stats.actorHealth.health, playerName, this.GetComponent<LSM_PlayerCtrl>());
        player_dummy.playerCamera = MainCam.GetComponent<Camera>();
        mapsubcam_target = player_dummy.camerapos;
        GameManager.Instance.gameUI.SetActive(true);
        GameManager.Instance.gameUI_SC.playerHealth(playerMinionCtrl);

        subTarget_minion.transform.gameObject.SetActive(false);
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
