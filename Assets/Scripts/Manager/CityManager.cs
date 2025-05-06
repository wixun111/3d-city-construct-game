using System;
using System.Collections;
using System.Collections.Generic;
using Controller;
using Entity;
using Entity.Buildings;
using Loader;
using UnityEngine;

namespace Manager
{
    public class CityManager : Singleton<CityManager>
    {
        [SerializeField] private GameObject cityPrefab;
        [SerializeField] private City currentCity;
        [SerializeField] private List<City> cities;
        [SerializeField] private float nextUpdateTime;
        [SerializeField] private float updateInterval;
        private void Start()
        {
            nextUpdateTime = 1f;
            updateInterval = 1f;
            cities = new List<City>();
        }
        private void Update()
        {
            if (!currentCity) return;
            if (TimeManager.Instance.IsPaused) return;
            var currentTime = TimeManager.Instance.CurrentTime;
            if (currentTime >= nextUpdateTime)
            {
                foreach (var city in cities)
                {
                    city.UpdateResources();
                }
                // 计算下一次更新时间（基于游戏内时间）
                nextUpdateTime = currentTime + updateInterval;
                // Debug.Log(currentTime + " : " + nextUpdateTime);
            }
            UIManager.Instance.UpdateCityUI(currentCity);
        }

        // 创建城市
        public void CreateCity()
        {
            Clear();
            var cityData = CityLoader.Instance.CityData;
            for (var i = 0; i < cityData.Count; i++)
            {
                // 实例化城市对象
                var cityObj = Instantiate(cityPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                // 获取 `City.cs` 组件并设置属性
                var cityScript = cityObj.GetComponent<City>();
                if (cityScript == null) continue;
                cityScript.InitData(cityData[i],i);
                cities.Add(cityScript);
            }
            currentCity = cities[0];
            UIManager.Instance.UpdateCityUI(currentCity);
        }

        public City CurrentCity
        {
            get => currentCity;
            set => currentCity = value;
        }
        
        public List<City> Cities
        {
            get => cities;
            set => cities = value;
        }

        public void Load(List<CityData> cityData, int currentCityId)
        {
            Clear();
            foreach (var city in cityData)
            {
                // 实例化城市对象
                var cityObj = Instantiate(cityPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                cityObj.name = "City_" + city.CityId;
                // 获取 `City.cs` 组件并设置属性
                var cityScript = cityObj.GetComponent<City>();
                if (cityScript == null) continue;
                cityScript.Load(city);
                cities.Add(cityScript);
                if (cityScript.CityId == currentCityId)
                {
                    currentCity = cityScript;
                }
            }
            PlaneManager.Instance.GenerateCity();
            currentCity.UpdateBuildingTile();
            UIManager.Instance.ShowCityUI(currentCity);
        }

        public void Clear()
        {
            foreach (var city in cities)
            {
                Destroy(city.gameObject);
            }
            cities.Clear();
        }

        public Building GetBuilding(Vector3Int position)
        {
            return CurrentCity.GetBuilding(position);
        }

        public void RepairBuilding()
        {
            currentCity.RepairBuilding(PlaneManager.Instance.GetTilePosition());
        }
    }
}