using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LSM_PlayerCtrl : MonoBehaviour
{
    public GameObject mySpawner;
    private Camera mapCamCamera;

    public MoonHeader.PlayerState state;

    private float wheelSpeed = 15f;
    private float map_move = 1f;
    private bool is_zoomIn;
    private IEnumerator zoomIn;

    public GameObject MainCam, MapCam, MapSubCam;
    public Vector3 mapCamBasePosition;
    public GameObject minionStatsPannel;
    private LSM_MinionCtrl subTarget_minion;
    private TextMeshProUGUI minionStatsPannel_txt;

    private GameObject mapcamSub_Target, mapsubcam_target;
	private void Start()
	{
        mapCamCamera = MapCam.GetComponent<Camera>();
        zoomIn = ZoomInMinion();
        is_zoomIn = false;
        MapCam.transform.position = mapCamBasePosition;
        MapCam.GetComponent<Camera>().orthographicSize = 40;
        minionStatsPannel.SetActive(false);
        minionStatsPannel_txt = minionStatsPannel.GetComponentInChildren<TextMeshProUGUI>();
        
    }

	void Update()
    {
        ClickEv();
        MapEv();
        SubMapCamMove();
    }

    private void MapEv()
    {
        if (state == MoonHeader.PlayerState.None)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
            mapCamCamera.orthographicSize = Mathf.Min(60, Mathf.Max(15, mapCamCamera.orthographicSize - scroll));

            Vector3 mapcampPosition = MapCam.transform.position;
            Vector3 move_f = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * map_move;
            MapCam.transform.position = new Vector3(
                mapcampPosition.x+ move_f.x, mapcampPosition.y,
                mapcampPosition.z+ move_f.z);

            if (!ReferenceEquals(mapcamSub_Target, null) && (scroll != 0 || move_f != Vector3.zero))
            {
                subTarget_minion.CHangeTeamColor();
                mapcamSub_Target = null;
                is_zoomIn = false;
                StopCoroutine(zoomIn);
                minionStatsPannel.SetActive(false);
            }
        }
    }

    private void ClickEv()
    {
        if (Input.GetMouseButtonDown(0))
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
                                    subTarget_minion.CHangeTeamColor();
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
                                subTarget_minion.icon.GetComponent<Renderer>().material.color = Color.green;
                            }

                            //GameManager.Instance.MapCam.GetComponent<Camera>().orthographicSize = 20;
                        }
                        break;
                }

            }
        }
    }

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
                minionStatsPannel_txt.text = string.Format("Minion : {0}\nHealth : {1}\nATK : {2}",
                                "none", subTarget_minion.stats.health, subTarget_minion.stats.Atk);
                MapCam.transform.position = (mapcamSub_Target.transform.position + Vector3.up * 95);
                MapSubCam.transform.position = mapsubcam_target.transform.position;
                MapSubCam.transform.rotation = mapsubcam_target.transform.rotation;
            }
        }
    }

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

    public void SelectPlayerMinion()
    {
        if (!ReferenceEquals(mapcamSub_Target, null))
        {
            subTarget_minion.PlayerConnect();
        }
    }
}
