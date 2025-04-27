using System.Collections.Generic;
using System.IO;
using Entity;
using Newtonsoft.Json;
using UnityEngine;

namespace Loader
{
    public class SettingLoader : Singleton<SettingLoader>
    {
        private Dictionary<string,object> settingData = new Dictionary<string,object>();
        private Setting setting;
        public void Awake()
        {
            LoadSaveData();
        }
        public void LoadSaveData()
        {
            var jsonFile = Resources.Load<TextAsset>("Json/Setting");
            if (jsonFile != null)
            {
                settingData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonFile.text);
                Debug.Log("Setting data loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load Setting.json");
            }
            setting = new Setting(settingData);
        }

        public Dictionary<string, object> SettingData
        {
            get => settingData;
            set => settingData = value;
        }

        public Setting Setting
        {
            get => setting;
            set => setting = value;
        }
    }
}