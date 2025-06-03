using System;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Entity.Buildings;

namespace UI
{
    public class BuildingPanel : MonoBehaviour
    {
        [SerializeField] private Text buildingInfo;
        [SerializeField] private Button repairButton;
        [SerializeField] private Button upgradeButton; 
        [SerializeField] private Button dismantleButton;
        [SerializeField] private Button styleButton;
        // [SerializeField] private Slider healthBar;  // 添加耐久度条

        private Building currentBuilding;

        public void Start()
        {
            upgradeButton.onClick.AddListener(Upgrade);
            repairButton.onClick.AddListener(Repair);
            dismantleButton.onClick.AddListener(Dismantle);
            styleButton.onClick.AddListener(ChangeStyle);
        }

        public void SetBuilding(Building building)
        {
            currentBuilding = building;
            UpdateBuildingInfo();
        }

        public void UpdateBuildingInfo()
        {
            var building = currentBuilding;
            if (!building) return;

            Debug.Log($"更新建筑物信息: {building.BuildingName}, 当前健康值: {building.CurrentHealth}, 最大健康值: {building.MaxHealth}");

            // 更新建筑物信息文本
            buildingInfo.text = building.GetBuildingInfo();

            // // 更新耐久度条
            // if (healthBar)
            // {
            //     healthBar.maxValue = building.MaxHealth;
            //     healthBar.value = building.CurrentHealth;
            //     Debug.Log($"设置耐久度条: maxValue={healthBar.maxValue}, value={healthBar.value}");
            // }
            // else
            // {
            //     Debug.LogWarning("HealthBar 组件未设置！");
            // }

            // 根据建筑物状态更新按钮状态
            repairButton.interactable = !building.IsRepairing && building.CurrentHealth < building.MaxHealth;
            upgradeButton.interactable = building.Level < building.MaxLevel;
            styleButton.interactable = BuildManager.Instance.GetBuildingStyleCount(currentBuilding.BuildingName) != 1;
        }

        private void Dismantle()
        {
            BuildManager.Instance.Dismantle();
            gameObject.SetActive(false);
        }

        private void Upgrade()
        {
            BuildManager.Instance.Upgrade();
            UpdateBuildingInfo();
        }

        private void Repair()
        {
            CityManager.Instance.RepairBuilding();
            UpdateBuildingInfo();
        }
        
        private void ChangeStyle()
        {
            var count = BuildManager.Instance.GetBuildingStyleCount(currentBuilding.BuildingName);
            currentBuilding.Style = (currentBuilding.Style + 1) % count;
            BuildManager.Instance.ReplaceBuildingStyle(currentBuilding.gameObject,currentBuilding.BuildingName, currentBuilding.Style);
            UpdateBuildingInfo();
        }
    }
}