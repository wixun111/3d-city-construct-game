using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Entity
{
    [Serializable]
    public class Setting
    {
        [SerializeField] private string language;   // 语言设置
        [SerializeField] private float volume;      // 音量（0.0 - 1.0）
        [SerializeField] private string controlMode; // 控制方式（键盘、手柄等）
        [SerializeField] private bool aiControl; // 控制方式（键盘、手柄等）
        public Setting()
        {
            language = "English";
            volume = 1.0f;
            controlMode = "Keyboard";
            aiControl = false;
        }
        public Setting(string language,float volume)
        {
            this.language = language;
            this.volume = volume;
            this.controlMode = "Keyboard";
            this.aiControl = false;
        }
        public Setting(Dictionary<string, object> settingData)
        {
            language = (string) settingData["language"];
            volume = JsonConvert.DeserializeObject<float>(settingData["volume"].ToString());
            controlMode = (string) settingData["controlMode"];
            // aiControl = (bool) settingData["aiControl"];
        }
    
        public void PrintSettings()
        {
            Debug.Log($"Language: {language}, Volume: {volume}, Control: {controlMode}");
        }

        public string Language
        {
            get => language;
            set => language = value;
        }

        public float Volume
        {
            get => volume;
            set => volume = value;
        }

        public string ControlMode
        {
            get => controlMode;
            set => controlMode = value;
        }
    }
}