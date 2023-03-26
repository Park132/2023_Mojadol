using System.Collections;
using System.Collections.Generic;

using System;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

// 3�� ���Ӿ�
// 4�� ���� �Ŵ����� ����
namespace Com.MyCompany.Game
{
    public class PSH_GameManager : MonoBehaviourPunCallbacks
    {

        #region Photon Callbacks
        public override void OnLeftRoom() // �������̵�� �ݹ��Լ�, ���� ������ Launcher ������ �̵���Ų��
        {
            SceneManager.LoadScene(0);
        }

        #endregion

        #region Private Methods

        #region 4�� ���� �Ŵ����� �������� �ۼ�
        void LoadArena()
        {

        }
        #endregion

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom(); // �� ������ �޼ҵ�
        }
        #endregion
    }
}


