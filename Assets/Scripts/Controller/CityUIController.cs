using System;
using Controller;
using Entity;
using UnityEngine;
using UnityEngine.UI;

namespace Controller
{
    public class CityUIController  : Singleton<CityUIController>
    {
        [SerializeField] private GameObject cityPanel;
        [SerializeField] private GameObject buildPanel; 
        [SerializeField] private Text cityNameText; 
        [SerializeField] private Text infoText;       
        
 

        void Start()
        {
            // cityPanel.SetActive(false);
        }

        public void ShowCityPanel(City city)
        {
            if (city == null) return;
            cityNameText.text = city.CityName;
            infoText.text = $"人口: {city.Population}\n 经济: {city.Economy}";
            foreach (var resourceName in city.Resources.Keys)
            {
                infoText.text += "\n  " +  resourceName + ": " + Mathf.RoundToInt(city.Resources[resourceName]);
            }
            cityPanel.SetActive(true);
        }
        public void UpdateCityInfo(City city)
        {
            infoText.text = $"人口: {city.Population}\n 经济: {city.Economy}";
            foreach (var resourceName in city.Resources.Keys)
            {
                infoText.text += "\n " +  resourceName + ": " + Mathf.RoundToInt(city.Resources[resourceName]);
            }
        }
        
        public void HideCityPanel()
        {
            cityPanel.SetActive(false);
        }
        void Update()
        {
            // if (Input.GetMouseButtonDown(0) && !RectTransformUtility.RectangleContainsScreenPoint(
            //         cityPanel.GetComponent<RectTransform>(), Input.mousePosition))
            // {
            //     HideCityPanel();
            // }
        }
    }
}