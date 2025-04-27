using System;
using System.Collections;
using UnityEngine;

namespace Manager
{
    public class AIManager : Singleton<AIManager>
    {
        [SerializeField] private bool isAIEnabled;
        [SerializeField] private Coroutine aiCoroutine;
        public void Update()
        {
            if (!isAIEnabled) return;
            AutoRepairBuilding();
        }

        private void AutoRepairBuilding()
        {
            var currentCity = CityManager.Instance.CurrentCity;
            if (!currentCity) return;
            var buildingList = currentCity.BuildingList;
            foreach (var building in buildingList)
            {
                if (building.CurrentHealth <= building.MaxHealth / 2)
                {
                    currentCity.RepairBuilding(building);
                }
            }
        }
        public void EnableAI()
        {
            isAIEnabled = true;
            aiCoroutine = StartCoroutine(AIUpdateLoop());
        }

        private IEnumerator AIUpdateLoop()
        {
            while (isAIEnabled)
            {
                AutoRepairBuilding();
                yield return new WaitForSeconds(1f); // 每1秒执行一次
            }
        }
        public void DisableAI()
        {
            isAIEnabled = false;
            if (aiCoroutine != null)
                StopCoroutine(aiCoroutine);
        }

        public bool IsAIEnabled
        {
            get => isAIEnabled;
            set => isAIEnabled = value;
        }
    }
}