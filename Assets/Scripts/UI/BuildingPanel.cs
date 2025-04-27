using System;
using Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BuildingPanel : MonoBehaviour
    {
        [SerializeField] private Text buildingInfo;
        [SerializeField] private Button repairButton;
        [SerializeField] private Button upgradeButton; 
        [SerializeField] private Button dismantleButton;
        public void Start()
        {
            upgradeButton.onClick.AddListener(Upgrade);
            repairButton.onClick.AddListener(Repair);
            dismantleButton.onClick.AddListener(Dismantle);
        }

        private void Dismantle()
        {
            BuildManager.Instance.Dismantle();
            gameObject.SetActive(false);
        }

        private void Upgrade()
        {
            BuildManager.Instance.Upgrade();
        }
        private void Repair()
        {
            CityManager.Instance.RepairBuilding();
            UIManager.Instance.UpdateBuildingPanel();
        }
    }
}