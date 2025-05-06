using System;
using System.Collections.Generic;
using Controller;
using Loader;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Manager
{
    public class BuildManager : Singleton<BuildManager>
    {
        [SerializeField] private GameObject previewInstance;
        [SerializeField] private GameObject buildingPrefab;
        [SerializeField] private Dictionary<int, Dictionary<string, object>> buildingData = new Dictionary<int, Dictionary<string, object>>();
        [SerializeField] private int selectedBuildingId;
        private void Start()
        {
            // 获取建筑数据
            buildingData = BuildingLoader.Instance.GetBuildingData();
            selectedBuildingId = -1;
        }
        public void SelectBuilding(GameObject buildingPrefab)
        {
            if (previewInstance != null) Destroy(previewInstance);
            previewInstance = Instantiate(buildingPrefab);
            previewInstance.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.5f); // 半透明
        }

        public void Build()
        {
            // 获取建筑的数据
            if (!buildingData.TryGetValue(selectedBuildingId, value: out var buildingInfo)) return;

            var currentCity = CityManager.Instance.CurrentCity;
            var position = PlaneManager.Instance.GetTilePosition();
            Debug.Log(position);
            // 判断是否可以建造（例如，检查是否有足够资源等）
            if (!currentCity.CanBuild(buildingInfo, position)) return;
            currentCity.Build(buildingInfo,position, SetBuilding(position, (string)buildingInfo["buildingName"]));
            UIManager.Instance.UpdateCityUI(currentCity);
        }

        public GameObject SetBuilding(Vector3 position,string buildingName)
        {
            buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/" + buildingName);
            if (buildingPrefab == null)
            {
                buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/default");
            }
            var newBuilding = Instantiate(buildingPrefab, position, Quaternion.identity);
            newBuilding.transform.SetParent(CityManager.Instance.CurrentCity.gameObject.transform);
            return newBuilding;
        }

        public void StartBuildingMode(int buildingId)
        {
            selectedBuildingId = buildingId;
            Debug.Log("进入建造模式: " + buildingId);
            // 这里可以显示建筑预览，或者改变鼠标光标等
        }
        public void ExitBuildingMode()
        {
            selectedBuildingId = -1;
            Debug.Log("退出建造模式");
            // 这里可以隐藏建筑预览等
        }

        public void BuildJudge()
        {
            if (selectedBuildingId != -1)
            {
                Build();
            }
        }

        public bool IsBuildMode()
        {
            return selectedBuildingId != -1;
        }
        private Type GetBuildingTypeByName(string typeName)
        {
            return Type.GetType(typeName);
        }

        public void Dismantle()
        {
            var position = PlaneManager.Instance.GetTilePosition();
            var currentCity = CityManager.Instance.CurrentCity;
            currentCity.Dismantle(position);
        }

        public void Upgrade()
        {
            var position = PlaneManager.Instance.GetTilePosition();
            CityManager.Instance.GetBuilding(position).Upgrade();
            UIManager.Instance.UpdateBuildingPanel();
        }
    }
}