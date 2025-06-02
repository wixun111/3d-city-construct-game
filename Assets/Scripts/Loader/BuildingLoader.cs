using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Loader
{
    public class BuildingLoader : Singleton<BuildingLoader>
    {
        private List<Dictionary<string, object>> _buildingsData = new List<Dictionary<string, object>>();
        private readonly Dictionary<int, Sprite> _buildingIcons = new Dictionary<int, Sprite>();

        public void Awake()
        {
            LoadBuildingData();
            
        }

        private void LoadBuildingData()
        {
            var jsonFile = Resources.Load<TextAsset>("Json/Buildings");
            if (jsonFile != null)
            {
                _buildingsData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonFile.text);
                Debug.Log("Building data loaded successfully.");
                LoadBuildingIcons(); // 加载图标
            }
            else
            {
                Debug.LogError("Failed to load Buildings.json");
            }
        }
        private void LoadBuildingIcons()
        {
            foreach (var buildingId in Enumerable.Range(1, _buildingsData.Count).ToList())
            {
                var iconPath = "Icons/" + (string)_buildingsData[buildingId]["buildingName"] + "Icon"; // 确保路径正确
                
                var icon = Resources.Load<Sprite>(iconPath);
                if (icon != null)
                {
                    _buildingIcons[buildingId] = icon;
                }
                else
                {
                    Debug.LogWarning($"Icon not found for {_buildingsData[buildingId]["buildingName"]}: {iconPath}");
                }
            }
        }

        public Sprite GetBuildingIcon(int buildingId)
        {
            return _buildingIcons.TryGetValue(buildingId, out var icon) ? icon : null;
        }
        public string GetBuildingCategory(int buildingId)
        {
            return _buildingsData[buildingId]["classType"].ToString().Split(".")[1];
        }
        public List<int> GetBuildingIds()
        {
            return Enumerable.Range(0, _buildingsData.Count-1).ToList();
        }
        public string GetBuildingName(int buildingId)
        {
            return _buildingsData[buildingId]["buildingName"].ToString();
        }
        public Dictionary<string, object> GetBuildingData(int buildingId)
        {
            return _buildingsData[buildingId];
        }
        public List<Dictionary<string, object>> GetBuildingData()
        {
            return _buildingsData;
        }
        public List<Dictionary<string, object>> BuildingsData
        {
            get => _buildingsData;
            set => _buildingsData = value;
        }

        public string GetBuildingType(int buildingId)
        {
            return _buildingsData[buildingId]["buildingType"].ToString();
        }
    }
}