using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Manager
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private GameObject savePanel;
        [SerializeField] private GameObject startMenu;
        [SerializeField] private GameObject timeControlPanel;
        [SerializeField] private GameObject gameMenu;
        [SerializeField] private GameObject buildingPanel;
        [SerializeField] private GameObject weatherPanel;
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private GameObject aiSettingPanel;
        [SerializeField] private List<GameObject> displayUI;


        public void UpdateBuildingPanel()
        {
            var buildingInfo = buildingPanel.GetComponentInChildren<Text>();
            var currentCity = CityManager.Instance.CurrentCity;
            Debug.Log(PlaneManager.Instance.GetTilePosition());
            buildingInfo.text = currentCity.GetBuildingInfo(PlaneManager.Instance.GetTilePosition());
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
            savePanel.SetActive(true);
        }
        public void HideSavePanel()
        {
            savePanel.SetActive(false);
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
    }
}