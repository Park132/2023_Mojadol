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

        #endregion

        #region Private Methods

        #region 4번 게임 매니저와 레벨에서 작성
        void LoadArena()
        {

        }
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


