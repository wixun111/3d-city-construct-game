using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Loader
{
    public class CityLoader : Singleton<CityLoader>
    {
        private Dictionary<int, Dictionary<string, object>> _cityData = new Dictionary<int, Dictionary<string, object>>();
        private readonly Dictionary<int, Sprite> _cityIcons = new Dictionary<int, Sprite>();
        
        public void Awake()
        {
            LoadCityData();
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
    }
}