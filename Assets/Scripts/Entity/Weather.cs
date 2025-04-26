using UnityEngine;

namespace Entity
{
    public class Weather
    {
        [SerializeField] private string weatherName;
        [SerializeField] private float duration;     // 天气持续时间
        public Weather(string name, float duration)
        {
            this.weatherName = name;
            this.duration = duration;
        }
        public void ApplyWeatherEffects()
        {
            switch (weatherName.ToLower())  // 转换为小写，以确保字符串比较不区分大小写
            {
                case "sunny":
                    Debug.Log("晴天：提高资源生产！");
                    break;
                case "rainy":
                    Debug.Log("雨天：提升农作物生长！");
                    break;
                case "snowy":
                    Debug.Log("雪天：减少建筑生产速度！");
                    break;
                case "stormy":
                    Debug.Log("暴风雪：减少交通和建筑速度！");
                    break;
                default:
                    Debug.Log("未知天气！");
                    break;
            }
        }

        public float Duration
        {
            get => duration;
            set => duration = value;
        }

        public string WeatherName
        {
            get => weatherName;
            set => weatherName = value;
        }
    }
}