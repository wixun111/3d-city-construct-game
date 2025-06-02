using System.Collections.Generic;
using Loader;
using Manager;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    public class BuildUIController : Singleton<BuildUIController>
    {
        [SerializeField] private GameObject buildPanel; // 建造面板
        [SerializeField] private RectTransform toggleContainer; // 按钮父级
        [SerializeField] private GameObject buildingTogglePrefab; // 预制体
        [SerializeField] private Transform categoryButtonContainer; // 分类按钮容器
        [SerializeField] private GameObject categoryButtonPrefab;
        [SerializeField] private ToggleGroup toggleGroup; // Toggle 组
        [SerializeField] private List<int> buildingIds;
        [SerializeField] private Dictionary<int, Sprite> buildingIcons;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button exitBuildButton;
        [SerializeField] private List<string> buildingCategories;
        private string currentCategory = null;
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
            Debug.Log(buildingIds.Count);
            buildingIcons = new Dictionary<int, Sprite>();
            foreach (var building in buildingIds)
            {
                var icon = BuildingLoader.Instance.GetBuildingIcon(building);
                if (icon != null)
                {
                    buildingIcons[building] = icon;
                }
                var category = BuildingLoader.Instance.GetBuildingCategory(building);
                if (!buildingCategories.Contains(category))
                {
                    buildingCategories.Add(category);
                }
            }
            GenerateCategoryButtons();
            Debug.Log(buildingIds.Count);
            Debug.Log("BuildUIController Load Successfully!");
        }
        private void GenerateCategoryButtons()
        {
            foreach (Transform child in categoryButtonContainer)
            {
                Destroy(child.gameObject);
            }

            // 添加“全部”按钮
            CreateCategoryButton("all", null);

            // 添加其他分类按钮
            foreach (var category in buildingCategories)
            {
                CreateCategoryButton(category, category);
            }
        }
        private void CreateCategoryButton(string label, string category)
        {
            var btnObj = Instantiate(categoryButtonPrefab, categoryButtonContainer);
            var text = btnObj.GetComponentInChildren<Text>();
            if (text) text.text = label.ToLower();

            var button = btnObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                currentCategory = category;
                GenerateBuildingButtons();
            });
        }
        // 生成建筑按钮
        private void GenerateBuildingButtons()
        {
            // 清除旧按钮
            foreach (Transform child in toggleContainer)
            {
                Destroy(child.gameObject);
            }
            var filteredIds = string.IsNullOrEmpty(currentCategory)
                ? buildingIds
                : buildingIds.FindAll(id =>  BuildingLoader.Instance.GetBuildingCategory(id) == currentCategory);
            // 创建新按钮
            foreach (var buildingId in filteredIds)
            {
                var toggleObj = Instantiate(buildingTogglePrefab, toggleContainer);
                toggleObj.name = BuildingLoader.Instance.GetBuildingName(buildingId);

                var toggleScript = toggleObj.GetComponent<BuildingToggle>();
                if (toggleScript == null) continue;
                var buildingName = BuildingLoader.Instance.GetBuildingName(buildingId);
                buildingIcons.TryGetValue(buildingId, out var icon);

                toggleScript.Initialize(
                    buildingId,
                    buildingName,
                    icon,
                    toggleGroup,
                    OnBuildingSelected
                );
            }
            toggleContainer.sizeDelta = new Vector2(200 * buildingIds.Count, 300);
        }
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
    }
}
    