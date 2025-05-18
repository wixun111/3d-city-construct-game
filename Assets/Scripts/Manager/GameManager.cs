using System;
using Entity;
using Loader;

namespace Manager
{
    public class GameManager : Singleton<GameManager>
    {
        private Player player;
        private Setting setting;

        public void Awake()
        {
            LoadSetting();
        }

        public void LoadSetting()
        {
            setting = new Setting(SettingLoader.Instance.SettingData);
        }

        public void StartGame()
        {
            TimeManager.Instance.InitTime();
            TimeManager.Instance.ShowTime();
            CityManager.Instance.CreateCity();
            PlaneManager.Instance.GenerateCity();
            WeatherManager.Instance.Init();
            TrafficManager.Instance.InitRoad();
            UIManager.Instance.ShowTimeControlPanel();
        }
        public void HandleClick()
        {
            if (!UIManager.Instance.IsUIOn())
            {
                PlaneManager.Instance.HandleClick();
            }
            else
            {
                UIManager.Instance.HandleClick();
            }
        }
    }
}