using Entity;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CityPanel : MonoBehaviour
    {
        [SerializeField] private Text cityNameText;
        [SerializeField] private Text cityInfoText;

        public void ShowCity(City city)
        {
            cityNameText.text = city.CityName;
            cityInfoText.text = GenerateCityInfo(city);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private string GenerateCityInfo(City city)
        {
            System.Text.StringBuilder info = new System.Text.StringBuilder();

            info.AppendLine($"城市规模: {city.CityLevel}");
            info.AppendLine($"人口: {city.Population}");
            info.AppendLine($"经济: {city.Economy}");
            info.AppendLine($"城市大小: {city.Width} × {city.Length}");
            info.AppendLine($"建筑数量: {city.BuildingList.Count}/{city.BuildingLimit}");
            info.AppendLine("资源情况:");
            foreach (var resource in city.Resources)
            {
                info.AppendLine($"- {resource.Key}: {resource.Value:F1}");
            }
            return info.ToString();
        }
    }
}