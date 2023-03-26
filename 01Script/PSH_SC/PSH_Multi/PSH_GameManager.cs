using System.Collections;
using System.Collections.Generic;

using System;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

// 3번 게임씬
// 4번 게임 매니저와 레벨
namespace Com.MyCompany.Game
{
    public class PSH_GameManager : MonoBehaviourPunCallbacks
    {

        #region Photon Callbacks
        public override void OnLeftRoom() // 오버라이드된 콜백함수, 룸을 나가서 Launcher 신으로 이동시킨다
        {
            SceneManager.LoadScene(0);
        }

        #region 4번 게임 매니저와 레벨에서 작성
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEneteredRoom() {0}", other.NickName);

            if(PhotonNetwork.IsMasterClient) // 들어온 플레이어가 MasterClient일 때 호출됩니다 (다른 플레이어가 들어올 때
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);

                LoadArena();
            }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

            if (PhotonNetwork.IsMasterClient) // 플레이어가 나갈 때, MasterClient일 때 호출
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

                LoadArena();
            }
        }

        // OnPlayerEnterRoom, OnPlayerLeftRoom은 플레이어 수에 따라 LoadArena를 호출하기 위해 재정의되어 만들어짐
        // 왜냐면 이 게임은 플레이어 수에 따라 맵의 크기가 변하는 것이기 때문이니까
        #endregion

        #endregion

        #region Private Methods

        #region 4번 게임 매니저와 레벨에서 작성
        void LoadArena()
        {
            if(!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("we are not the masterClient");
                return;
            }

            Debug.LogFormat("Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("PSH_M_Room_for_" + PhotonNetwork.CurrentRoom.PlayerCount); // 플레이어의 수에 따른 맵의 수를 만들음

        }
        #region LoadArena 설명
        /*
         * 1. PhotonNetwork.LoadLevel()은 마스터클라이언트인 경우에만 호출 <- PhotonNetwork.IsMasterClient를 통해 체크
         * 2. PhotonNetwork.automaticallySyncScene을 통해 Photon이 룸안의 모든 클라이언트들에게 레벨을 로드하게함(유니티가 아니라)
         */
        #endregion


        #endregion

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom(); // 방 나가는 메소드
        }
        #endregion
    }
}


