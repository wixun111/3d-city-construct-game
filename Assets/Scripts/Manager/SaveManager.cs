using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entity;
using Loader;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Manager
{
    public class SaveManager : Singleton<SaveManager>
    {
        private const string SavePath = "Saves";

        public void SaveGame(int saveId, string saveName)
        {
            var data = new SaveData
            {
                SaveID = saveId,
                SaveName = saveName,
                CurrentCityId = CityManager.Instance.CurrentCity.CityId,
                CityData = CityManager.Instance.Cities.Select(city => new CityData(city)).ToList(),
                CurrentTime = TimeManager.Instance.CurrentTime,
                TimeScale = TimeManager.Instance.TimeScale,
            };
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },
                Formatting = Formatting.Indented
            };
            var json = JsonConvert.SerializeObject(data,settings);  // true: 格式化 JSON
            var path = ConvertPath(saveId);
            File.WriteAllText(path, json);
            Debug.Log("Game Saved: " + path);
        }

        public void SaveBuilding()
        {
            var currentCity = CityManager.Instance.CurrentCity;
            var buildingList = currentCity.BuildingList;
            var buildingInfoList = buildingList.Select(building => new BuildingInfo
            {
                position = new int[] { building.Position.x, building.Position.z },
                rotation = new float[] { building.Rotation.eulerAngles.x,
                    building.Rotation.eulerAngles.y,
                    building.Rotation.eulerAngles.z },
                BuildingId = building.BuildingId
            }).ToList();
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },
                Formatting = Formatting.Indented
            };
            var json = JsonConvert.SerializeObject(buildingInfoList, settings);
            File.WriteAllText("Assets/Resources/Json/CityBuildings.json", json);
            CityLoader.Instance.LoadCityBuildingData();
        }

        public void LoadGame(int saveId)
        {
            var saveData = SaveLoader.Instance.SaveDataDict[saveId];
            var cityData = JsonConvert.DeserializeObject<List<CityData>>(saveData["cityData"].ToString());
            CityManager.Instance.Load(cityData,JsonConvert.DeserializeObject<int>(saveData["currentCityId"].ToString()));
            TrafficManager.Instance.Init();
            BuildManager.Instance.FixRoads(1);
            TimeManager.Instance.Load( JsonConvert.DeserializeObject<float>(saveData["currentTime"].ToString()),JsonConvert.DeserializeObject<float>(saveData["timeScale"].ToString()));
        }

        private string ConvertPath(int saveId)
        {
            return SavePath + "/save"+ saveId + ".json";
        }

        public void DeleteSave(int saveId)
        {
            var path = ConvertPath(saveId);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("File deleted successfully.");
            }
            else
            {
                Debug.LogWarning("File does not exist: " + path);
            }
        }

        public void SaveSetting(string language,float volumeSliderValue)
        {
            var data = new Setting(language,volumeSliderValue);
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy(),
                },
                Formatting = Formatting.Indented
            };
            var json = JsonConvert.SerializeObject(data,settings);
            File.WriteAllText("Assets/Resources/Json/Setting.json", json);
            Debug.Log("Game Setting Saved: Json/Setting.json");
        }
    }
    public struct BuildingInfo
    {
        public int[] position;
        public float[] rotation;
        public int BuildingId;
    }
}