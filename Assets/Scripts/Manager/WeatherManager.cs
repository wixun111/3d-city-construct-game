using System;
using System.Collections.Generic;
using Entity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Manager
{
    public class WeatherManager: Singleton<WeatherManager>
    {
        [SerializeField] private Weather currentWeather;
        [SerializeField] private float weatherTimer; // 当前天气的持续计时器
        [SerializeField] private float minWeatherDuration;
        [SerializeField] private float maxWeatherDuration;
        private List<Weather> weatherOptions;

        private void Awake()
        {
            weatherTimer = float.PositiveInfinity;
            minWeatherDuration = 10f;
            maxWeatherDuration = 30f;
        }

        public void Init()
        {
            InitializeWeatherOptions();
            GenerateRandomWeather();
        }
        private void Update()
        {
            weatherTimer -= Time.deltaTime;
            if (weatherTimer > 0) return;
            GenerateRandomWeather();
            UIManager.Instance.UpdateWeatherPanel(currentWeather.WeatherName);
        }
        private void InitializeWeatherOptions()
        {
            weatherOptions = new List<Weather>
            {
                // CreateWeather("sunny"),
                // CreateWeather("rainy"),
                // CreateWeather("snowy"),
                CreateWeather("stormy")
            };
        }
        private Weather CreateWeather(string type)
        {
            var duration = Random.Range(minWeatherDuration, maxWeatherDuration);
            var center = new Vector3Int(
                Random.Range(0, CityManager.Instance.CurrentCity.Length),
                0,
                Random.Range(0, CityManager.Instance.CurrentCity.Width)
            );
            float radius = Random.Range(50, 100);
            return new Weather(type, duration, center, radius);
        }

        private void GenerateRandomWeather()
        {
            var index = Random.Range(0, weatherOptions.Count-1);
            currentWeather = weatherOptions[index];
            weatherTimer = currentWeather.Duration;
            currentWeather.ApplyWeatherEffects();
            Debug.Log($"当前天气: {currentWeather.WeatherName}, 持续时间: {weatherTimer} 秒");
        }

        public Weather CurrentWeather
        {
            get => currentWeather;
            set => currentWeather = value;
        }
    }
}