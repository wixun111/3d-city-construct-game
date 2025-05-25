using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    public class RoadUIController : Singleton<RoadUIController>
    {
        
        [SerializeField] private Button roadButton; // 进入道路建造模式的按钮
        [SerializeField] private Button exitRoadButton; // 退出道路建造模式的按钮

        private void Start()
        {
            Debug.Log("RoadUIController Start");
            if (roadButton == null)
            {
                Debug.LogError("Road Button is not assigned!");
                return;
            }
            if (exitRoadButton == null)
            {
                Debug.LogError("Exit Road Button is not assigned!");
                return;
            }
           

            
            roadButton.onClick.AddListener(() => {
                Debug.Log("Road Button Clicked");
                ShowRoadPanel();
            });
            exitRoadButton.onClick.AddListener(() => {
                Debug.Log("Exit Road Button Clicked");
                HideRoadPanel();
            });
        }

        // 显示道路建造面板
        public void ShowRoadPanel()
        {
            Debug.Log("ShowRoadPanel called");
            roadButton.gameObject.SetActive(false);
            exitRoadButton.gameObject.SetActive(true);
        }

        // 隐藏道路建造面板
        public void HideRoadPanel()
        {
            Debug.Log("HideRoadPanel called");
            roadButton.gameObject.SetActive(true);
            exitRoadButton.gameObject.SetActive(false);
        }

        // 当其他建造模式激活时，确保退出道路建造模式
        
    }
} 