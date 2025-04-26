using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Entity.Buildings;
using Manager;
using Newtonsoft.Json;
using UnityEngine;

namespace Entity
{
    [System.Serializable]
    public class City : MonoBehaviour
    {
        [SerializeField] private int cityId;
        [SerializeField] private string cityName;
        [SerializeField] private int population;
        [SerializeField] private Dictionary<string,int> resources;
        [SerializeField] private int economy;
        [SerializeField] private int width;
        [SerializeField] private int length;
        [SerializeField] private List<Building> buildingList = new List<Building>();
        [SerializeField] private Building[,] buildings;
        [SerializeField] private bool[,] canBuild;
        [SerializeField] private int buildingLimit;
        [SerializeField] private int cityLevel;
        public Sprite citySprite;

        public void Load(CityData cityData)
        {
            cityId = cityData.CityId;
            cityName = cityData.CityName;
            population = cityData.Population;
            resources = cityData.Resources;
            economy = cityData.Economy;
            width = cityData.Width;
            length = cityData.Length;
            buildings = new Building[length, width];
            canBuild = new bool[length,width];
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    canBuild[i, j] = true;
                }
            }
            foreach (var buildingData in cityData.BuildingsData)
            {
                var pos = buildingData.Position;
                var building = gameObject.AddComponent<Building>();
                building.Load(buildingData);
                buildingList.Add(building);
                AddBuilding(building,pos);
            }
        }

        public bool CanBuild(Dictionary<string, object> buildingInfo,Vector3Int position)
        {
            // 判断是否满足建造条件，比如资源检查等
            var cost = JsonConvert.DeserializeObject<Dictionary<string, int>>(buildingInfo["cost"].ToString());
            var size = JsonConvert.DeserializeObject<int[]>(buildingInfo["size"].ToString());
            if (cost.Keys.Any(key => resources.ContainsKey(key) && resources[key] < cost[key]))
            {
                Debug.Log($"no enough resources to build ");
                return false;
            }
            for (var i = 0; i < size[0]; i++)
            {
                for (var j = 0; j < size[1]; j++)
                {
                    if (position.x - i < 0 || position.x - i>= length || position.z - j < 0 || position.z - j >= width)
                    {
                        Debug.Log($"the position is outside the bounds of the city");
                        return false;
                    }
                    Debug.Log($"the position can build ({position.x - i}, {position.z - j})：{canBuild[position.x - i, position.z - j]}");
                    if (canBuild[position.x - i, position.z - j]) continue;
                    Debug.Log($"the position can't build {position.x - i}, {position.z - j}");
                    return false;
                }
            }
            return true;
        }

        public void Build(Dictionary<string, object> buildingInfo, Vector3Int position,GameObject newBuilding)
        {
            // var buildingName = (string)buildingInfo["buildingName"];  // 获取建筑名称
            // var classType = (string)buildingInfo["classType"];  // 获取建筑名称
            // var buildingType = GetBuildingTypeByName(classType);

            var building = newBuilding.AddComponent<Building>();
            building.InitData(buildingInfo,position);
            buildingList.Add(building);
            AddBuilding(building,position);
            ConsumeResource(buildingInfo);
        }

        public void ConsumeResource(Dictionary<string, object> buildingInfo)
        {
            var cost = JsonConvert.DeserializeObject<Dictionary<string, int>>(buildingInfo["cost"].ToString());
            foreach (var key in cost.Keys.Where(key => resources.ContainsKey(key)))
            {
                resources[key] -= cost[key];
            }
        }

        private void OnMouseDown()  // 点击城市
        {
            CityUIController.Instance.ShowCityPanel(this);
        }

        public void AddBuilding(Building building,Vector3Int position)
        {
            for (var i = 0; i < building.Size[0]; i++)
            {
                for (var j = 0; j < building.Size[1]; j++)
                {
                    canBuild[position.x - i, position.z - j] = false;
                    buildings[position.x - i, position.z - j] = building;
                }
            }
        }
        public void UpdateResources()
        {
            foreach (var building in buildingList)
            {
                if (!building.IsProductive) continue;
                foreach (var resourceName in building.ProduceResourceType)
                {
                    resources[resourceName] += building.ProductionRate;
                }
            }
        }
        public void UpdateBuildingTile()
        {
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var building = buildings[i, j];
                    if (!building) continue;
                    BuildManager.Instance.SetBuilding(building.Position,building.BuildingName);
                    Debug.Log(building.BuildingName);
                }
            }
        }
        public void InitData(Dictionary<string, object> cityData,int id)
        {
            cityId = id;
            cityName = (string)cityData["cityName"];
            population = Convert.ToInt32(cityData["population"]);
            resources = JsonConvert.DeserializeObject<Dictionary<string, int>>(cityData["resources"].ToString());
            economy = Convert.ToInt32(cityData["economy"]);
            length = Convert.ToInt32(cityData["length"]);
            width = Convert.ToInt32(cityData["width"]);
            buildings = new Building[length, width];
            canBuild = new bool[length,width];
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    canBuild[i, j] = true;
                }
            }
        }
        public string CityName
        {
            get => cityName;
            set => cityName = value;
        }
        public int CityId
        {
            get => cityId;
            set => cityId = value;
        }
        public int Population
        {
            get => population;
            set => population = value;
        }
        public Dictionary<string,int> Resources
        {
            get => resources;
            set => resources = value;
        }
        public int Economy
        {
            get => economy;
            set => economy = value;
        }
        public int Length
        {
            get => length;
            set => length = value;
        }
        public int Width
        {
            get => width;
            set => width = value;
        }
        public Building[,] Buildings
        {
            get => buildings;
            set => buildings = value;
        }
        public int CityLevel
        {
            get => cityLevel;
            set => cityLevel = value;
        }
        public Building GetBuilding(Vector3Int position)
        {
            return Buildings[position.x, position.z];
        }

        public void Dismantle(Vector3Int position)
        {
            var building = buildings[position.x, position.z];
            buildingList.Remove(building);
            buildings[position.x, position.z] = null;
            Debug.Log(building.BuildingName + " dismantled!");
            Destroy(building.gameObject);
        }

        public string GetBuildingInfo(Vector3Int getTilePosition)
        {
            return Buildings[getTilePosition.x, getTilePosition.z].GetBuildingInfo();
        }
    }
}