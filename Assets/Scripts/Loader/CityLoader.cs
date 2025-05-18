using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Loader
{
    public class CityLoader : Singleton<CityLoader>
    {
        private Dictionary<int, Dictionary<string, object>> _cityData = new Dictionary<int, Dictionary<string, object>>();
        private List<Dictionary<string, int>> _cityBuildingData = new List<Dictionary<string, int>>();
        private readonly Dictionary<int, Sprite> _cityIcons = new Dictionary<int, Sprite>();
        
        public void Awake()
        {
            LoadCityData();
            LoadCityBuildingData();
        }
        private void LoadCityData()
        {
            var jsonFile = Resources.Load<TextAsset>("Json/Cities");
            if (jsonFile != null)
            {
                _cityData = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, object>>>(jsonFile.text);
                Debug.Log("City data loaded successfully.");
                LoadCityIcons(); // 加载图标
            }
            else
            {
                Debug.LogError("Failed to load Cities.json");
            }
        }
        private void LoadCityBuildingData()
        {
            var jsonFile = Resources.Load<TextAsset>("Json/CityBuildings");
            if (jsonFile != null)
            {
                _cityBuildingData = JsonConvert.DeserializeObject<List<Dictionary<string, int>>>(jsonFile.text);
                Debug.Log("City Building data loaded successfully.");
                LoadCityIcons(); // 加载图标
            }
            else
            {
                Debug.LogError("Failed to load CityBuildings.json");
            }
        }
        private void LoadCityIcons()
        {
            foreach (var cityId in _cityData.Keys)
            {
                var iconPath = "Icons/" + _cityData[cityId]["cityName"] + "Icon"; // 确保路径正确
                var icon = Resources.Load<Sprite>(iconPath);
                if (icon != null)
                {
                    _cityIcons[cityId] = icon;
                }
                else
                {
                    Debug.LogWarning($"Icon not found for {_cityData[cityId]["cityName"]}: {iconPath}");
                }
            }
        }

        public Dictionary<int, Sprite> CityIcons => _cityIcons;
        public Dictionary<int, Dictionary<string, object>> CityData
        {
            get => _cityData;
            set => _cityData = value;
        }

        public List<Dictionary<string, int>> CityBuildingData
        {
            get => _cityBuildingData;
            set => _cityBuildingData = value;
        }
    }
}