using Entity;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CityUI : MonoBehaviour
    {
        [SerializeField] private Text cityNameText; 
        [SerializeField] private Text infoText;      
        [SerializeField] private Button cityPanelButton;      
        private void Start()
        {
            cityPanelButton.onClick.AddListener(ShowCityPanel);
        }

        private void ShowCityPanel()
        {
            UIManager.Instance.ShowCityPanel();
        }

        public void ShowCityUI(City city)
        {
            if (city == null) return;
            cityNameText.text = city.CityName;
            infoText.text = $"人口: {city.Population}\n 经济: {city.Economy}";
            foreach (var resourceName in city.Resources.Keys)
            {
                infoText.text += "\n  " +  resourceName + ": " + Mathf.RoundToInt(city.Resources[resourceName]);
            }
            gameObject.SetActive(true);
        }
        public void UpdateCityUI(City city)
        {
            infoText.text = $"人口: {city.Population}\n 经济: {city.Economy}";
            foreach (var resourceName in city.Resources.Keys)
            {
                infoText.text += "\n " +  resourceName + ": " + Mathf.RoundToInt(city.Resources[resourceName]);
            }
        }
    }
}