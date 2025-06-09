using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    
    public bool isJoyStick;
    public Image touchBtn, joyStickBtn;
    public Color blue;
    public PlayerCtrl playerCtrl_script;
    
    GameObject mainView, missionView;

    private void Start()
    {
        if (playerCtrl_script != null)
        {
            mainView = playerCtrl_script.mainView;
            missionView = playerCtrl_script.missionView;
        }
    }
    
    //설정버튼을 누르면 호출
    public void ClickSetting()
    {
        gameObject.SetActive(true);
        
        // 🔥 수정: 네트워크 RPC를 통해 이동 제한
        if (playerCtrl_script != null && playerCtrl_script.Object.HasInputAuthority)
        {
            playerCtrl_script.SetCantMoveRpc(true);
        }
    }
    
    //게임으로 돌아가기 버튼을 누르면 호출
    public void ClickBack()
    {
        gameObject.SetActive(false);
        
        // 🔥 수정: 네트워크 RPC를 통해 이동 허용
        if (playerCtrl_script != null && playerCtrl_script.Object.HasInputAuthority)
        {
            playerCtrl_script.SetCantMoveRpc(false);
        }
    }

    //터치이동을 누르면 호출
    public void ClickTouch()
    {
        isJoyStick = false;
        if (touchBtn != null) touchBtn.color = blue;
        if (joyStickBtn != null) joyStickBtn.color = Color.white;
        
        // 조이스틱 UI 업데이트
        UpdateJoystickUI();
        Debug.Log("[설정] 터치 모드 활성화 | isJoyStick: " + isJoyStick); // 로그 추가
    }
    
    //조이스틱을 누르면 호출
    public void ClickJoyStick()
    {
        isJoyStick = true;
        if (touchBtn != null) touchBtn.color = Color.white;
        if (joyStickBtn != null) joyStickBtn.color = blue;

        
        // // 조이스틱 UI 업데이트
        UpdateJoystickUI();
        Debug.Log("[설정] 조이스틱 모드 활성화 | isJoyStick: " + isJoyStick); // 로그 추가
    }

    public void UpdateJoystickUI()
    {
        //조이스틱을 화면에 보여줄지 말지
        if (playerCtrl_script != null && playerCtrl_script.joystick != null)
        {
            bool shouldShowJoystick = isJoyStick;
            playerCtrl_script.joystick.SetActive(shouldShowJoystick);
            Debug.Log("[설정] 조이스틱 UI 상태: " + shouldShowJoystick 
                                          + " | isJoyStick: " + isJoyStick 
                                          + " | IsCantMove: " + playerCtrl_script.IsCantMove);
        }
        else
        {
            Debug.LogError("[설정] playerCtrl_script 또는 joystick이 없습니다!");
        }
    }
    
    //게임 나가기 버튼을 누르면 호출
    public void ClickQuit()
    {
        if (mainView != null) mainView.SetActive(true);
        if (missionView != null) missionView.SetActive(false);

        
        
        // 캐릭터 삭제
        if (playerCtrl_script != null)
        {
            playerCtrl_script.DestroyPlayer();
        }
    }
}