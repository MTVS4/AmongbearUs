using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject missionView;
    
    // 🔥 추가: BasicSpawner 참조 (네트워크 게임 시작용)
    public BasicSpawner basicSpawner;
    
    // 게임 종료 버튼 누르면 호출
    public void ClickQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // 🔥 수정: 네트워크 게임 시작
    //미션 버튼 누르면 호출
    public void ClickMission()
    {
        gameObject.SetActive(false);
        if (missionView != null)
        {
            missionView.SetActive(true);
        }

        // 🔥 중요: 네트워크 게임을 시작해야 함
        // 로컬로 캐릭터를 생성하는 대신 네트워크 게임을 시작
        if (basicSpawner != null)
        {
            // BasicSpawner의 OnGUI에서 Host 버튼을 눌렀을 때와 같은 효과
            // 또는 자동으로 Host 모드로 시작
            Debug.Log("네트워크 게임을 시작합니다. BasicSpawner의 Host/Join 버튼을 사용하세요.");
        }
        else
        {
            Debug.LogError("BasicSpawner가 설정되지 않았습니다! Inspector에서 BasicSpawner를 연결해주세요.");
        }
    }
    
    // 🔥 추가: 네트워크 게임 시작을 위한 새로운 메서드들
    public void StartAsHost()
    {
        if (basicSpawner != null)
        {
            // Host로 게임 시작하는 로직을 여기에 추가
            // BasicSpawner의 StartGame을 public으로 만들거나 이벤트를 통해 호출
            Debug.Log("Host로 게임을 시작합니다.");
        }
    }
    
    public void JoinAsClient()
    {
        if (basicSpawner != null)
        {
            // Client로 게임 참가하는 로직을 여기에 추가
            Debug.Log("Client로 게임에 참가합니다.");
        }
    }
}