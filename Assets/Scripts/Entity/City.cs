using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Entity.Buildings;
using Loader;
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
        [SerializeField] private Dictionary<string,float> resources;
        [SerializeField] private int economy;
        [SerializeField] private int width;
        [SerializeField] private int length;
        [SerializeField] private List<Building> buildingList = new List<Building>();
        [SerializeField] private Building[,] buildings;
        [SerializeField] private bool[,] canBuild;
        [SerializeField] private int buildingLimit;
        [SerializeField] private int cityLevel;
        public Sprite citySprite;

        public void Load(CityData cityData,int currentCityId)
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
            if (cityId == currentCityId)
            {
                CityManager.Instance.CurrentCity = this;
            }
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
                var rot = Quaternion.Euler(buildingData.Rotation[0], buildingData.Rotation[1], buildingData.Rotation[2]);
                var buildingObject = cityId == currentCityId ? BuildManager.Instance.SetBuilding(pos,rot,buildingData.BuildingName) : gameObject;
                var building = buildingObject.AddComponent<Building>();
                building.Load(buildingData);
                buildingList.Add(building);
                AddBuilding(building,pos);
            }
        }
        public void InitBuilding(List<Dictionary<string,object>> cityBuildingData)
        {
            buildings = new Building[length, width];
            canBuild = new bool[length,width];
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    canBuild[i, j] = true;
                }
            }
            foreach (var buildingData in cityBuildingData)
            {
                var posData = JsonConvert.DeserializeObject<int[]>(buildingData["position"].ToString());
                var pos = new Vector3Int(posData[0],0,posData[1]);
                var rotData = JsonConvert.DeserializeObject<float[]>(buildingData["rotation"].ToString());
                var rot = Quaternion.Euler(rotData[0], rotData[1], rotData[2]);
                if (buildings[pos.x,pos.z])
                {
                    continue;
                }
                var buildingObject = BuildManager.Instance.SetBuilding(pos,rot, JsonConvert.DeserializeObject<int>(buildingData["buildingId"].ToString()));
                var building = buildingObject.AddComponent<Building>();
                building.InitData(BuildingLoader.Instance.BuildingsData[JsonConvert.DeserializeObject<int>(buildingData["buildingId"].ToString())],pos);
                buildingList.Add(building);
                AddBuilding(building,pos);
            }
            BuildManager.Instance.SetDefaultFloor(width, length);
        }
        public bool CanBuild(Dictionary<string, object> buildingInfo,Vector3Int position)
        {
            // 判断是否满足建造条件，比如资源检查等
            var cost = JsonConvert.DeserializeObject<Dictionary<string, float>>(buildingInfo["cost"].ToString());
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
                        return false;
                    }
                    if (canBuild[position.x - i, position.z - j]) continue;
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

        public void ConsumeResource(Dictionary<string, object> buildingInfo,float scale = 1)
        {
            var cost = JsonConvert.DeserializeObject<Dictionary<string, float>>(buildingInfo["cost"].ToString());
            foreach (var key in cost.Keys.Where(key => resources.ContainsKey(key)))
            {
                resources[key] -= cost[key] * scale;
            }
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
            var weather = WeatherManager.Instance.CurrentWeather;
            if (weather == null) return;
            var center = weather.Center;
            var radius = weather.Radius;
            var weatherName = weather.WeatherName;
            float scale = 1;
            for(var i = 0; i < buildingList.Count; i++){
                var building = buildingList[i];
                if (Vector3Int.Distance(building.Position, center) <= radius)
                {
                    switch (weatherName)
                    {
                        case "snowy":
                            scale = 0.8f;
                            break;
                        case "stormy":
                            scale = 0.7f;
                            if (ModifyBuildingHealth(-10, building))
                            {
                                i--;
                            }
                            break;
                    }
                }
                if (!building.IsProductive) continue;
                if (building.CurrentHealth <= building.MaxHealth / 2)
                {
                    scale /= 2;
                }
                foreach (var resourceName in building.ProduceResourceType)
                {
                    resources[resourceName] += building.ProductionRate * scale;
                }
            }
        }

        public bool ModifyBuildingHealth(int value,Building building)
        {
            building.CurrentHealth += value;
            if (building.CurrentHealth > building.MaxHealth)
            {
                building.CurrentHealth = building.MaxHealth;
            }
            else if (building.CurrentHealth < 0)
            {
                buildingList.Remove(building);
                var position = building.Position;
                for (var i = 0; i < building.Size[0]; i++)
                {
                    for (var j = 0; j < building.Size[1]; j++)
                    {
                        canBuild[position.x - i, position.z - j] = true;
                        buildings[position.x - i, position.z - j] = null;
                    }
                }
                Destroy(building.gameObject);
                return true;
            }
            return false;
        }
        public void InitData(Dictionary<string, object> cityData,int id)
        {
            cityId = id;
            cityName = (string)cityData["cityName"];
            population = Convert.ToInt32(cityData["population"]);
            resources = JsonConvert.DeserializeObject<Dictionary<string, float>>(cityData["resources"].ToString());
            economy = Convert.ToInt32(cityData["economy"]);
            length = Convert.ToInt32(cityData["length"]);
            width = Convert.ToInt32(cityData["width"]);
            buildings = new Building[length, width];
            canBuild = new bool[length,width];
            cityLevel = 1;
            buildingLimit = cityLevel * 100;
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    canBuild[i, j] = true;
                }
            }
        }
        public Building GetBuilding(Vector3Int position)
        {
            return Buildings[position.x, position.z];
        }

        public void Dismantle(Vector3Int position)
        {
            var building = buildings[position.x, position.z];
            position = building.Position;
            buildingList.Remove(building);
            for (var i = 0; i < building.Size[0]; i++)
            {
                for (var j = 0; j < building.Size[1]; j++)
                {
                    canBuild[position.x - i, position.z - j] = true;
                    buildings[position.x - i, position.z - j] = null;
                }
            }
            Debug.Log(building.BuildingName + " dismantled!");
            Destroy(building.gameObject);
        }
        public void RepairBuilding(Vector3Int getTilePosition)
        {
            var building = buildings[getTilePosition.x, getTilePosition.z];
            var scale = (1 - building.CurrentHealth / building.MaxHealth)/2 + 0.2f;
            var buildingInfo = BuildingLoader.Instance.BuildingsData[building.BuildingId];
            ConsumeResource(buildingInfo,scale);
            building.CurrentHealth = building.MaxHealth;
        }
        public void RepairBuilding(Building building)
        {
            var scale = (1 - building.CurrentHealth / building.MaxHealth)/2 + 0.2f;
            var buildingInfo = BuildingLoader.Instance.BuildingsData[building.BuildingId];
            ConsumeResource(buildingInfo,scale);
            building.CurrentHealth = building.MaxHealth;
        }

        public string GetBuildingInfo(Vector3Int getTilePosition)
        {
            return Buildings[getTilePosition.x, getTilePosition.z].GetBuildingInfo();
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
        public Dictionary<string,float> Resources
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

        public List<Building> BuildingList
        {
            get => buildingList;
            set => buildingList = value;
        }
        public int CityLevel
        {
            get => cityLevel;
            set => cityLevel = value;
        }

        public int BuildingLimit
        {
            get => buildingLimit;
            set => buildingLimit = value;
        }
    }
}