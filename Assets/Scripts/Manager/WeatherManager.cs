using System.Collections.Generic;
using Entity;
using UnityEngine;

namespace Manager
{
    public class WeatherManager: Singleton<WeatherManager>
    {
        [SerializeField] private Weather currentWeather;
        [SerializeField] private float weatherTimer; // 当前天气的持续计时器
        [SerializeField] private float minWeatherDuration = 10f;
        [SerializeField] private float maxWeatherDuration = 30f;
        private List<Weather> weatherOptions;

        private void Awake()
        {
            minWeatherDuration = 10f;
            maxWeatherDuration = 30f;
        }

        private void Start()
        {
            InitializeWeatherOptions();
            GenerateRandomWeather();
        }
        private void Update()
        {
            weatherTimer -= Time.deltaTime;
            if (!(weatherTimer <= 0)) return;
            GenerateRandomWeather();
            UIManager.Instance.UpdateWeatherPanel(currentWeather.WeatherName);
        }
        private void InitializeWeatherOptions()
        {
            weatherOptions = new List<Weather>
            {
                new Weather("sunny", Random.Range(minWeatherDuration, maxWeatherDuration)),
                new Weather("rainy", Random.Range(minWeatherDuration, maxWeatherDuration)),
                new Weather("snowy", Random.Range(minWeatherDuration, maxWeatherDuration)),
                new Weather("stormy", Random.Range(minWeatherDuration, maxWeatherDuration))
            };
        }

        private void GenerateRandomWeather()
        {
            var index = Random.Range(0, weatherOptions.Count-1);
            currentWeather = weatherOptions[index];
            weatherTimer = currentWeather.Duration;
            currentWeather.ApplyWeatherEffects();
            Debug.Log($"当前天气: {currentWeather.WeatherName}, 持续时间: {weatherTimer} 秒");
        }
    }
}