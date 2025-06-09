using UnityEngine;

public class Mission1 : MonoBehaviour
{
    private Animator anim;
    private PlayerCtrl PlayerCtrl_script;
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // 미션 시작
    
    // PlayerCtrl을 미션은 중간에 호출되기때문에 스타트가 아닌 미션스타트에 넣어준거임
    public void MissionStart()
    {
        anim.SetBool("isUp", true);
        PlayerCtrl_script = FindAnyObjectByType<PlayerCtrl>();
    }
    
    // 엑스버튼 누르면 호출
    public void ClickCancle()
    {
        anim.SetBool("isUp", false);
        PlayerCtrl_script.MissionEnd();
    }
}
