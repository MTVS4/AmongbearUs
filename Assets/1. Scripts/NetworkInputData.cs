using UnityEngine;
using Fusion;

// 1단계: 네트워크로 전송할 입력 데이터 구조체 만들기
public struct NetworkInputData : INetworkInput
{
    public UnityEngine.Vector2 moveDirection;  // 조이스틱이나 마우스 입력 방향
    public NetworkBool isWalking;          // 움직이고 있는지 상태
    public NetworkButtons buttons; // 버튼 입력들
}

// 버튼 입력용 enum
public enum InputButtons
{
    InteractButton = 0,  // 상호작용 버튼
    // 필요한 다른 버튼들 추가 가능
}