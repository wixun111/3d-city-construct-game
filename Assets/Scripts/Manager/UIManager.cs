﻿using System;
using System.Collections.Generic;
using Entity;
using UI;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Manager
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private GameObject cityUI;
        [SerializeField] private GameObject cityPanel;
        [SerializeField] private GameObject savePanel;
        [SerializeField] private GameObject startMenu;
        [SerializeField] private GameObject timeControlPanel;
        [SerializeField] private GameObject gameMenu;
        [SerializeField] private GameObject buildingPanel;
        [SerializeField] private GameObject weatherPanel;
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private GameObject aiSettingPanel;
        [SerializeField] private GameObject disasterImpactPanel;
        [SerializeField] private List<GameObject> displayUI;

        private DisasterImpactPanel impactPanel;

        private void Start()
        {
            if (disasterImpactPanel != null)
            {
                impactPanel = disasterImpactPanel.GetComponent<DisasterImpactPanel>();
                if (impactPanel != null)
                {
                    disasterImpactPanel.SetActive(true);
                    ShowDisasterImpactPanel();
                }
                else
                {
                    Debug.LogError("DisasterImpactPanel component not found on disasterImpactPanel GameObject");
                }
            }
            else
            {
                Debug.LogError("disasterImpactPanel reference is missing in UIManager");
            }
        }

        public void UpdateBuildingPanel()
        {
            var buildingPanelScript = buildingPanel.GetComponentInChildren<BuildingPanel>();
            buildingPanelScript.SetBuilding(CityManager.Instance.CurrentCity.GetBuilding(PlaneManager.Instance.GetTilePosition()));
        }
        public void UpdateCityUI(City currentCity)
        {
            var ui = cityUI.GetComponentInChildren<CityUI>();
            ui.UpdateCityUI(currentCity);
        }
        public void UpdateCityPanel()
        {
            var currentCity = CityManager.Instance.CurrentCity;
            var ui = cityPanel.GetComponentInChildren<CityPanel>();
            ui.ShowCity(currentCity);
        }
        public void ShowCityUI(City currentCity)
        {
            var ui = cityUI.GetComponentInChildren<CityUI>();
            ui.ShowCityUI(currentCity);
        }
        public void UpdateWeatherPanel(string weatherName)
        {
            // var weatherImage = Resources.Load<Sprite>("Icons/Weather/stormy");
            var weatherImage = Resources.Load<Sprite>("Icons/Weather/"+ weatherName);
            weatherPanel.GetComponentInChildren<Image>().sprite = weatherImage;
            weatherPanel.GetComponentInChildren<Text>().text = weatherName;
        }
        public void ShowSavePanel()
        {
            displayUI.Add(savePanel);
            savePanel.SetActive(true);
        }
        public void HideSavePanel()
        {
            displayUI.Remove(savePanel);
            savePanel.SetActive(false);
        }

        public void ShowCityPanel()
        {
            displayUI.Add(cityPanel);
            UpdateCityPanel();
            cityPanel.SetActive(true);
        }

        public void HideCityPanel()
        {
            displayUI.Remove(cityPanel);
            cityPanel.SetActive(false);
        }

        public void ShowCityUI()
        {
            cityUI.SetActive(true);
        }

        public void HideCityUI()
        {
            cityUI.SetActive(false);
        }

        public void ShowStartMenu()
        {
            startMenu.SetActive(true);
        }
        public void HideStartMenu()
        {
            startMenu.SetActive(false);
        }
        public void ShowTimeControlPanel()
        {
            timeControlPanel.SetActive(true);
        }

        public void HideTimeControlPanel()
        {
            timeControlPanel.SetActive(false);
        }
        public void ShowGameMenu()
        {
            displayUI.Add(gameMenu);
            gameMenu.SetActive(true);
        }
        public void HideGameMenu()
        {
            gameMenu.SetActive(false);
            displayUI.Remove(gameMenu);
        }

        public void ShowSettingPanel()
        {
            displayUI.Add(settingPanel);
            settingPanel.SetActive(true);
        }

        public void HideSettingPanel()
        {
            settingPanel.SetActive(false);
        }

        public void ShowAISettingPanel()
        {
            displayUI.Add(aiSettingPanel);
            aiSettingPanel.SetActive(true);
        }

        public void HideAISettingPanel()
        {
            aiSettingPanel.SetActive(false);
        }

        public void ShowWeatherPanel()
        {
        }

        public void HideWeatherPanel()
        {
        }

        public void ShowBuildingPanel()
        {
            displayUI.Add(buildingPanel);
            UpdateBuildingPanel();
            buildingPanel.SetActive(true);
        }
        public void HideBuildingPanel()
        {
            buildingPanel.SetActive(false);
            displayUI.Remove(buildingPanel);
        }
        public bool GetGameMenuActive()
        {
            return gameMenu.activeInHierarchy;
        }
        public void ShowDisplayUI()
        {
            foreach (var ui in displayUI)
            {
                ui.SetActive(true);
            }
        }
        public void HideDisplayUI()
        {
            foreach (var ui in displayUI)
            {
                ui.SetActive(false);
            }
            displayUI.Clear();
        }
        public void HandleClick()
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                HideDisplayUI();
            }
        }

        public bool IsUIOn()
        {
            return displayUI.Count != 0;
        }

        public void UpdateDisasterImpact(float fireImpact, float earthquakeImpact)
        {
            if (impactPanel != null)
            {
                impactPanel.UpdateFireImpact(fireImpact);
                impactPanel.UpdateEarthquakeImpact(earthquakeImpact);
            }
        }

        public void ShowDisasterImpactPanel()
        {
            if (disasterImpactPanel != null)
            {
                displayUI.Add(disasterImpactPanel);
                disasterImpactPanel.SetActive(true);
            }
        }

        public void HideDisasterImpactPanel()
        {
            if (disasterImpactPanel != null)
            {
                displayUI.Remove(disasterImpactPanel);
                disasterImpactPanel.SetActive(false);
            }
        }
    }
}