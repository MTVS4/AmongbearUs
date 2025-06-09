using Fusion;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("플레이어 설정")]
    public float moveSpeed = 4f;
    
    [Header("컴포넌트 참조")]
    private Animator anim;
    
    // 네트워크 동기화되는 변수들
    [Networked] public Vector3 NetworkPosition { get; set; }
    [Networked] public bool IsWalking { get; set; }
    [Networked] public bool FacingRight { get; set; } = true;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public override void Spawned()
    {
        // 네트워크 오브젝트가 생성될 때 호출
        // 자신의 캐릭터인 경우 카메라 설정
        if (Object.HasInputAuthority)
        {
            Camera.main.transform.parent = transform;
            Camera.main.transform.localPosition = new Vector3(0, 0, -10);
        }
    }

    // 🔥 가장 중요한 부분! 네트워크 업데이트
    public override void FixedUpdateNetwork()
    {
        // 입력 권한이 있는 클라이언트만 입력을 처리
        if (GetInput<NetworkInputData>(out var input) == false) 
            return;

        // 네트워크 입력 기반으로 이동 처리
        if (input.isWalking)
        {
            // 이동 벡터 계산
            Vector3 moveVector = new Vector3(input.moveDirection.x, input.moveDirection.y, 0) 
                                * moveSpeed * Runner.DeltaTime;
            
            // 위치 업데이트 (네트워크 동기화됨)
            NetworkPosition += moveVector;
            
            // 방향 전환 처리
            if (input.moveDirection.x < 0 && FacingRight)
            {
                FacingRight = false;
            }
            else if (input.moveDirection.x > 0 && !FacingRight)
            {
                FacingRight = true;
            }
        }
        
        // 걷기 상태 업데이트
        IsWalking = input.isWalking;
    }

    // 실제 위치와 애니메이션을 업데이트 (모든 클라이언트에서 실행)
    public override void Render()
    {
        // 네트워크 위치를 실제 Transform에 적용
        transform.position = NetworkPosition;
        
        // 애니메이션 상태 업데이트
        if (anim != null)
        {
            anim.SetBool("isWalk", IsWalking);
        }
        
        // 스케일로 방향 전환
        Vector3 scale = transform.localScale;
        scale.x = FacingRight ? 1f : -1f;
        transform.localScale = scale;
    }

    // 초기 위치 설정
    public void SetInitialPosition(Vector3 position)
    {
        NetworkPosition = position;
        transform.position = position;
    }
}