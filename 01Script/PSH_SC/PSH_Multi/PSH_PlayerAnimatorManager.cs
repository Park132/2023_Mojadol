using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 5번 플레이어 만들기
// 이건 그냥 얘네들이 PlayerCtrl스크립트 만드는 건데
// 작성방법이 나랑은 많이 다르게 만들길래 신기해서 넣어놓고 주석달았음
// 카메라는 중요합니다. 이거 이상하게하면 게임 화면이 저기갔다 여기로갔다 저사람으로 갔다 하더라구요
// 여기서는 중간에 CameraWork스크립트는 그냥 카메라 동작과 관련된 스크립트더라구요
namespace Com.MyCompany.Game
{
    public class PSH_PlayerAnimatorManager : MonoBehaviour
    {
        #region Private Fields
        [SerializeField]
        private float directionDampTime = 0.25f; // 회전하는데 걸리는 시간

        #endregion

        #region MonoBehaviour Callbacks

        private Animator animator;
        void Start()
        {
            animator = GetComponent<Animator>(); // animator를 가져옵니당
            if(!animator)
            {
                Debug.LogError("PlayerAnimator is Missing", this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(!animator)
            {
                return;
            }

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // state정보를 가져옵니다
            if (stateInfo.IsName("Base Layer.Run")) // 캐릭터가 달리고 있는지를 판단, Base Layer안의 Run상태
            {
                if (Input.GetButtonDown("Fire2")) //Fire2 Input을 감지, Jump트리거 발생
                {
                    animator.SetTrigger("Jump");
                }
            }

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");// 일반적인 입력받기
            if (v < 0)
            {
                v = 0;
            }
            
            animator.SetFloat("Speed", h * h + v * v); // animator의 apply motion root를 적용하여 그걸로 이동
            animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime); // 이건 방향회전
                                                                                  // damping Time은 원하는 값까지 도달하는데 걸리는 시간
                                                                                  // deltaTime은 Update한수 때문에 
        }

        #endregion

    }
}

