using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Mission6 : MonoBehaviour
{
    public bool[] isConnected = new bool[4];
    public Dictionary<Color, bool> isConnectedDict = new Dictionary<Color, bool>();
    public RectTransform[] rights;
    public LineRenderer[] lines;
    private Animator anim;
    private PlayerCtrl PlayerCtrl_script;

    private Vector2 clickPos;
    LineRenderer line;
    Color leftColor, rightColor;

    private bool isDrag;
    float leftY, rightY;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()  
    {   
        //드래그 중일 때
        if (isDrag && line != null)
        {
            // 선 따라가기
           line.SetPosition(1, new Vector3(Mouse.current.position.ReadValue().x- clickPos.x, Mouse.current.position.ReadValue().y -clickPos.y, -10));
        
           //드래그 끝
           if (Mouse.current.leftButton.wasReleasedThisFrame)
           {
               // 마우스 위치에서 레이 쏘기
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                
               
               RaycastHit hit;
               
               if (Physics.Raycast(ray, out hit))
               {
                   Debug.Log("닿은 오브젝트: "+ hit.collider.gameObject.name);
                   //오른쪽 선에 닿았다면
                   RectTransform rightRect = hit.transform.GetComponent<RectTransform>();

                   if (Physics.Raycast(ray, out hit))
                   {
                       GameObject rightLine = hit.transform.gameObject;
                       // // 막대의 anchoredPosition.x, y를 얻어서 선 끝점에 고정
                       //오른쪽 선 y값
                       rightY = rightLine.GetComponent<RectTransform>().anchoredPosition.y;
                       
                       // 오른쪽 선 색상
                       rightColor = rightLine.GetComponent<Image>().color;
                       
                       // x는 오른쪽 막대의 x, y는 막대의 y, z는 -10
                       line.SetPosition(1, new Vector3(500, rightY - leftY , -10));

                       isConnectedDict[leftColor] = (leftColor == rightColor);
                       // 색 비교
                       // if (leftColor == rightColor)
                       // {
                       //     switch (leftY)
                       //     {
                       //         case 225: isConnected[0] = true; break;
                       //         case 75 : isConnected[1] = true; break;
                       //         case -75 : isConnected[2] = true; break;
                       //         case -225: isConnected[3] = true; break;
                       //     }
                       // }
                       // else
                       // {
                       //     switch (leftY)
                       //     {
                       //         case 225: isConnected[0] = false; break;
                       //         case 75 : isConnected[1] = false; break;
                       //         case -75 : isConnected[2] = false; break;
                       //         case -225: isConnected[3] = false; break;
                       //     }
                       // }
                       
                       //성공여부 체크
                       // if (isConnected[0] && isConnected[1] && isConnected[2] && isConnected[3])
                       if (isConnectedDict.Values.All(value => value))
                       {
                           Invoke("MissionSuccess", 0.2f);
                       }
                   }
                   else
                   {
                       Debug.Log("RectTransform이없음");
                       // 혹시 RectTransform이 없으면 원래대로
                       Vector3 worldPos = hit.transform.position;
                       line.SetPosition(1, worldPos);
                   }
                   //성공 시 드래그 종료
                   isDrag = false;
               }
               //닿지 않았다면
               else
               {
                   Debug.Log("ray가 아무것도 안닿음");
                line.SetPosition(1, new Vector3(0, 0, -10));
                isDrag = false;
               }
              
           }
    
        }
    }
    
    //미션 시작
    public void MissionStart()
    {
        anim.SetBool("isUp", true);
        PlayerCtrl_script = FindObjectOfType<PlayerCtrl>();
        
        // 초기화
        for (int i = 0; i < rights.Length; i++)
        {
            // isConnected[i] = false;
            lines[i].SetPosition(1, new Vector3(0, 0, -10));
        }
        
        foreach (Image tmpImage in GameObject.Find("Mission6").transform.Find("Scale/MissionUI/BackGround/Left").GetComponentsInChildren<Image>())
        {
            isConnectedDict[tmpImage.color] = false;
        }
        
        // 랜덤
        for (int i = 0; i < rights.Length; i++)
        {
            Vector3 temp = rights[i].anchoredPosition;
            
            int rand = Random.Range(0, 4);
            rights[i].anchoredPosition = rights[rand].anchoredPosition;
            
            rights[rand].anchoredPosition = temp;
            
        }
    }
    
    //엑스버튼 누르면 호출
    public void ClickCancle()
    {
        anim.SetBool("isUp", false);
        PlayerCtrl_script.MissionEnd();
    }
    
    //선 누르면 호출
    public void ClickLine(LineRenderer click)
    {
        clickPos = Mouse.current.position.ReadValue();
        line = click;

        //왼쪽 선 y값
        leftY = click.transform.parent.GetComponent<RectTransform>().anchoredPosition.y;
        isDrag = true;
        
        // 왼쪽 선 색상
        // leftColor = click.transform.parent.name;
        leftColor = click.transform.parent.GetComponent<Image>().color;
    } 
    
    //미션 성공하면 호출
    public void MissionSuccess()
    {
        ClickCancle();
    }
}
