using System.Collections.Generic;
using Loader;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    public class BuildUIController : Singleton<BuildUIController>
    {
        [SerializeField] private GameObject buildPanel; // 建造面板
        [SerializeField] private Transform toggleContainer; // 按钮父级
        [SerializeField] private GameObject buildingTogglePrefab; // 预制体
        [SerializeField] private ToggleGroup toggleGroup; // Toggle 组
        [SerializeField] private List<int> buildingIds;
        [SerializeField] private Dictionary<int, Sprite> buildingIcons;
        [SerializeField] private ScrollRect scrollRect; // 滚动组件
        [SerializeField] private Button buildButton;
        [SerializeField] private Button exitBuildButton;
        private Dictionary<string, List<int>> buildingCategories = new Dictionary<string, List<int>>();
        private void Start()
        {
            buildPanel.SetActive(false); // 默认隐藏建筑面板
            LoadBuildings();
            exitBuildButton.onClick.AddListener(HideBuildPanel);
            buildButton.onClick.AddListener(ShowBuildPanel);
        }
        
        private void LoadBuildings()
        {
            buildingIds = BuildingLoader.Instance.GetBuildingIds(); // 直接获取建筑名称
            buildingIcons = new Dictionary<int, Sprite>();
            buildingCategories = new Dictionary<string, List<int>>();
            foreach (var building in buildingIds)
            {
                var icon = BuildingLoader.Instance.GetBuildingIcon(building);
                if (icon != null)
                {
                    buildingIcons[building] = icon;
                }
                // buildingCategories[BuildingLoader.Instance.GetBuildingType(building)].Add(building);
            }
            Debug.Log(buildingIds.Count);
            Debug.Log("BuildUIController Load Successfully!");
            // buildingCategories["全部"] = new List<int>(buildingIds);
        }
        // 生成建筑按钮
        private void GenerateBuildingButtons()
        {
            // 清除旧按钮
            foreach (Transform child in toggleContainer)
            {
                Destroy(child.gameObject);
            }

            // 创建新按钮
            foreach (var buildingId in buildingIds)
            {
                var toggleObj  = Instantiate(buildingTogglePrefab, toggleContainer);
                var buildingName = BuildingLoader.Instance.GetBuildingName(buildingId);
                toggleObj.name = buildingName;

                var toggle = toggleObj.GetComponent<Toggle>();
                toggle.group = toggleGroup;  // 让 Toggle 互斥
                toggle.onValueChanged.AddListener((isSelected) => OnBuildingSelected(buildingId, isSelected));

                
                // 设置文本
                var toggleText = toggleObj.GetComponentInChildren<Text>();
                if (toggleText) toggleText.text = buildingName;

                // 设置图标
                var toggleImage = toggleObj.GetComponentInChildren<Image>();
                if (toggleImage && buildingIcons.TryGetValue(buildingId, out var icon))
                    toggleImage.sprite = icon;
            }
        }
        // private void GenerateCategoryButtons()
        // {
        //     foreach (Transform child in categoryBar)
        //         Destroy(child.gameObject);
        //
        //     foreach (var category in buildingCategories.Keys)
        //     {
        //         var btnObj = Instantiate(categoryButtonPrefab, categoryBar);
        //         var btnText = btnObj.GetComponentInChildren<Text>();
        //         if (btnText) btnText.text = category;
        //
        //         var button = btnObj.GetComponent<Button>();
        //         string capturedCategory = category;
        //         button.onClick.AddListener(() =>
        //         {
        //             currentCategory = capturedCategory;
        //             GenerateBuildingButtons();
        //         });
        //     }
        // }
        // 点击建筑按钮
        private void OnBuildingSelected(int buildingId, bool isSelected)
        {
            if (isSelected)
            {
                Debug.Log("选择了建筑: " + BuildingLoader.Instance.GetBuildingName(buildingId));
                BuildManager.Instance.StartBuildingMode(buildingId);
            }
            else
            {
                Debug.Log("取消选择建筑");
                BuildManager.Instance.ExitBuildingMode();
            }
        }

        // 关闭面板
        public void HideBuildPanel()
        {
            buildButton.gameObject.SetActive(true);
            exitBuildButton.gameObject.SetActive(false);
            buildPanel.SetActive(false);
            BuildManager.Instance.ExitBuildingMode();
        }
        // 显示建筑面板
        public void ShowBuildPanel()
        {
            GenerateBuildingButtons();
            buildButton.gameObject.SetActive(false);
            exitBuildButton.gameObject.SetActive(true);
            buildPanel.SetActive(true);
            BuildManager.Instance.ExitBuildingMode();
        }
        private void Update()
        {
            // // 滚轮控制左右滑动
            // if (Input.GetAxis("Mouse ScrollWheel") != 0)
            // {
            //     scrollRect.horizontalNormalizedPosition += Input.GetAxis("Mouse ScrollWheel") * 1f;
            // }

            // 禁止纵向滑动：强制 vertical 为 0
            // scrollRect.horizontalNormalizedPosition = Mathf.Clamp(scrollRect.horizontalNormalizedPosition, -1000f, 1000f);
            if (scrollRect.vertical)
                scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
    