using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoyStick : MonoBehaviour
{
    [Header("조이스틱 UI")]
    // 스틱과 배경 UI 요소
    public RectTransform stick, backGround;
    
    [Header("참조")]
    // 플레이어 컨트롤 스크립트 참조
    private PlayerCtrl playerCtrl_script;
    
    // 조이스틱 상태 관리 변수
    private bool isDrag;  // 드래그 중인지 여부
    private float limit; // 스틱 이동 제한 범위
    
    // 현재 입력값 (BasicSpawner의 OnInput에서 사용)
    public static Vector2 CurrentJoystickInput { get; private set; }  // 현재 조이스틱 입력 벡터
    public static bool IsJoystickActive { get; private set; }  // 조이스틱 활성화 상태
    
    private void Start()
    {
        playerCtrl_script = GetComponent<PlayerCtrl>();  // PlayerCtrl 컴포넌트 가져오기
        
        if (backGround != null)
        {
            limit = backGround.rect.width * 0.5f;  // 배경 UI 너비의 절반을 이동 제한 범위로 설정
        }

        if (!playerCtrl_script.HasInputAuthority)
        {
            Destroy(this);
        }
    }
    
    private void Update()
    {
        // 드래그 중일 때 조이스틱에서는 캐릭터 움직이는 코드 없다~
        if (isDrag)
        {
            // 현재 마우스/터치 스크린 좌표 (예: 1920x1080 화면 중앙 = 960,540)
            Vector2 mousePos = Mouse.current.position.ReadValue();
            // 조이스틱 배경의 스크린 좌표
            Vector2 bgPos = (Vector2)backGround.position;
            
            
            // 조이스틱 중심 → 마우스 위치 방향 벡터 계산
            Vector2 vec = mousePos - bgPos;
            // 스틱 위치 제한
            stick.localPosition = Vector2.ClampMagnitude(vec, limit);
            Debug.Log("isDrag: " + isDrag + "mousePos: " + mousePos + "bgPos: " + bgPos);
            Debug.Log("IsJoystickActive: " + IsJoystickActive);
            Debug.Log("vec: " + vec);
            Debug.Log("dir1: " + vec.normalized);
            Debug.Log("dir2: " + (stick.position - backGround.position).normalized);
            
            Vector3 dir = (stick.position - backGround.position).normalized;
            
            
            // 전역 변수에 입력값 저장 (BasicSpawner에서 읽음)
            IsJoystickActive = true; // 조이스틱 활성화 상태 업데이트
            
            // 플레이어 이동 (PlayerCtrl에 Move 메서드가 있다면 활용)
            if (playerCtrl_script != null)
            {
                playerCtrl_script.RPC_UpdateJoystickInput(dir); // 방향 벡터 전달
                playerCtrl_script.FacingRight = !(dir.x < 0);
                playerCtrl_script.IsWalking = isDrag;
            }
            
            // 왼쪽으로 이동
            if (dir.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            //오른쪽으로 이동
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            
            
            // 마우스 버튼을 뗐을 때 (드래그 종료)
         
            if (!Mouse.current.leftButton.isPressed)
            {
                StopJoystick(); // 조이스틱 중단 처리
            }
        }
    }
    
    // 조이스틱 중단 처리 메서드 (새로 추가)
    private void StopJoystick()
    {
        isDrag = false;  // 드래그 상태 해제
        stick.localPosition = Vector3.zero; // 스틱 위치 리셋
        CurrentJoystickInput = Vector2.zero;  // 입력값 초기화
        IsJoystickActive = false;  // 비활성화 상태 변경
        
        // 중요 : playerCtrl에 움직임 중단 신호 전달
        if (playerCtrl_script != null)
        {
            playerCtrl_script.RPC_UpdateJoystickInput(Vector2.zero);
            playerCtrl_script.IsWalking = false;
        }
    }
   
    
    
    
    // 스틱을 누르면 호출
    public void ClickStick()
    {
        isDrag = true;  // 드래그 상태 시작
    }
    
    // 정적 메서드: 외부에서 조이스틱 입력을 가져올 때 사용
    public static Vector2 GetJoystickInput()
    {
        return IsJoystickActive ? CurrentJoystickInput : Vector2.zero;  // 활성화 상태에 따라 입력값 반환
    }
    
    // 조이스틱 활성화 상태 확인
    public static bool GetJoystickActive()
    {
        return IsJoystickActive;  // 현재 활성화 상태 반환
    }
}