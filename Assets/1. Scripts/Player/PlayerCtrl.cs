using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; 
using UnityEngine.EventSystems;
using Fusion;

public class PlayerCtrl : NetworkBehaviour
{
    // [조이스틱 관련 변수]
    
    [Networked] public Vector2 joystickInput { get; set; }
    [Networked] public NetworkBool isDraggingJoystick { get; set; }

    // [입력 모드 판별 프로퍼티]
    private bool IsCurrentInputJoystick => Settings_script != null && Settings_script.isJoyStick;
    
    
    [Header("UI 참조")]
    public GameObject joystick, mainView, missionView;// UI 요소들
    public Settings Settings_script; // 설정 관리 스크립트
    public Button btn; // 미션 시작 버튼

    [Header("설정")]
    public float moveSpeed = 3f;

    // 컴포넌트
    private Animator anim; // 애니메이션 컨트롤러
    private Rigidbody2D rb2d; // Rigidbody2D 컴포넌트 참조 추가
    private GameObject coll; // 충돌한 오브젝트 임시 저장
    
    // 네트워크 동기화 변수
    [Networked, OnChangedRender(nameof(NetworkPositionChanged))] public Vector3 NetworkPosition { get; set; } // 동기화될 위치
    [Networked] public bool IsWalking { get; set; } // 걷기 상태
    [Networked] public bool FacingRight { get; set; } = true;  // 보는 방향 (기본 오른쪽)
    [Networked] public bool IsCantMove { get; set; }  // 이동 불가 상태
  
    // 로컬 변수
    private bool localIsCantMove;  // 로컬에서의 이동 불가 상태
 
    public override void Spawned()  // 네트워크 객체 생성시 호출
    {

        // [추가] 조이스틱 초기화 로그
        if (joystick != null)
        {
            Debug.Log("[플레이어] 조이스틱 초기화 완료 | 상태: " + joystick.activeSelf);
        }
        else
        {
            Debug.LogError("[플레이어] 조이스틱이 할당되지 않았습니다!");
        }
        
        
        anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>(); // Rigidbody2D 컴포넌트 가져오기

        // 자신의 캐릭터인 경우에만 카메라 설정
        if (Object.HasInputAuthority)
        {
            Camera.main.transform.parent = transform;  // 카메라를 플레이어에 부착
            Camera.main.transform.localPosition = new Vector3(0, 0, -10);  // 카메라 위치 조정
            
            // 초기 위치 설정
            //NetworkPosition = transform.position;
        }
    }

    public void NetworkPositionChanged()
    {
        Debug.Log($"{name} Position: {NetworkPosition}");
    }
    
    public override void FixedUpdateNetwork()
    {
        // 1. 권한 및 이동 가능 상태 체크
        //if (IsCantMove) return;

        // 2. 조이스틱 모드 처리
        if (IsCurrentInputJoystick)
        {
            if (isDraggingJoystick)
            {
                Debug.Log("Dragging joystick");
                // 이동 벡터 계산
                Vector3 moveVector = new Vector3(joystickInput.x, joystickInput.y, 0) 
                                     * moveSpeed * Runner.DeltaTime;
                
                // 네트워크 위치 업데이트
                NetworkPosition += moveVector;
                
                // 방향 전환
                if (joystickInput.x < 0) FacingRight = false;
                else if (joystickInput.x > 0) FacingRight = true;
                
                IsWalking = true;
            }
            else
            {
                IsWalking = false;
            }
            return; // 🔥 터치 입력 차단
        }

        // 3. 터치/키보드 모드 처리
        if (GetInput<NetworkInputData>(out var input))
        {
            if (input.isWalking)
            {
                Debug.Log($"{name} IsWalking");
                Vector3 moveVector = new Vector3(input.moveDirection.x, input.moveDirection.y, 0) 
                                     * moveSpeed * Runner.DeltaTime;
                //Debug.Log(moveVector);
                NetworkPosition += moveVector;
                
                Debug.Log(NetworkPosition);
                //transform.position += moveVector;
                
                if (input.moveDirection.x < 0) FacingRight = false;
                else if (input.moveDirection.x > 0) FacingRight = true;
                
                IsWalking = true;
            }
            else
            {
                IsWalking = false;
            }
        }
    }
    
    // [RPC로 변환된 조이스틱 입력 메서드]
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_UpdateJoystickInput(Vector2 input)
    {
        joystickInput = input.normalized;
        isDraggingJoystick = input != Vector2.zero;
        Debug.Log($"[조이스틱] 입력값: {joystickInput} | 드래그: {isDraggingJoystick}");
    }
    
    
    // 렌더링 업데이트 (모든 클라이언트)
    public override void Render()
    {
        // 위치 동기화 - Rigidbody2D 인스턴스를 통해 호출
        if (rb2d != null)
        {
            Debug.Log("Rb2d");
            rb2d.MovePosition(NetworkPosition);
        }
        else
        {
            // Rigidbody2D가 없는 경우 직접 transform 위치 설정
            transform.position = NetworkPosition;
        }
        
        // 애니메이션 업데이트
        if (anim != null)
        {
            anim.SetBool("isWalk", IsWalking);
        }
        
        // 캐릭터 방향 업데이트 (좌우 반전)
        Vector3 scale = transform.localScale;
        scale.x = FacingRight ? 1f : -1f;
        transform.localScale = scale;
        
        // UI 업데이트 (자신의 캐릭터만)
        if (Object.HasInputAuthority)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (IsCantMove == localIsCantMove) return; // 1️⃣ 변화가 없으면 바로 종료
    
        localIsCantMove = IsCantMove; // 2️⃣ 상태 업데이트
    
        bool shouldActive = !localIsCantMove         // 3️⃣ 조건 한 번에 계산
                            && Settings_script?.isJoyStick == true;  // (?. 는 "만약 있다면" 이라는 뜻)
    
        if (joystick.activeSelf != shouldActive)    // 4️⃣ 실제로 상태가 다를 때만 변경
        {
            joystick.SetActive(shouldActive);
        }
    }

    // 외부에서 호출할 수 있는 메서드들
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)] 
    public void SetCantMoveRpc(bool cantMove) // 이동 제어 RPC
    {
        IsCantMove = cantMove;  // 네트워크 동기화 변수 업데이트
    }

    // 캐릭터 삭제
    public void DestroyPlayer()
    {
        if (Object.HasInputAuthority && Camera.main != null)  
        {
            Camera.main.transform.parent = null;    // 카메라 부모 해제
        }
        
        if (Object.HasStateAuthority)  // 네트워크 객체 제거
        {
            Runner.Despawn(Object);
        }
    }

    // 트리거 이벤트들 (네트워크 동기화 필요시 RPC 사용)
    private void OnTriggerEnter2D(Collider2D collision)   // 트리거 진입
    {
        if (Object == null || !Object.HasInputAuthority) return;
        
        // null 체크 추가!
        if (collision == null) return;
        
        if (collision.CompareTag("Mission") && coll == null)
        {
            coll = collision.gameObject;  // 미션 오브젝트 저장
            btn.interactable = true;  // 버튼 활성화
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)  // 트리거 탈출
    {
        if (!Object.HasInputAuthority || collision == null) return;
        
        if (collision.CompareTag("Mission"))
        {
            coll = null;  // 미션 오브젝트 초기화
            btn.interactable = false;  // 버튼 비활성화
        }
    }
    
    // 버튼 클릭 이벤트
    public void ClickButton()
    {
        if (!Object.HasInputAuthority || coll == null) return;
        
        coll.SendMessage("MissionStart");  // 미션 시작 신호 전송
        SetCantMoveRpc(true);  // 이동 금지 상태 설정
        btn.interactable = false;  // 버튼 비활성화
    }
    
    // 미션 종료
    public void MissionEnd() 
    {
        if (Object.HasInputAuthority) SetCantMoveRpc(false);
    }
}