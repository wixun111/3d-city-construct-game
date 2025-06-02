using System;
using Loader;
using Manager;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingPanel : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider; 
        [SerializeField] private Button aiSettingButton; 
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_Dropdown languageDropdown;
        private void Start()
        {
            // 给按钮添加点击事件
            confirmButton.onClick.AddListener(SaveSetting);
            aiSettingButton.onClick.AddListener(AiSetting);
            volumeSlider.value = SettingLoader.Instance.Setting.Volume;
            languageDropdown.value = LanguageToValue(TranslationManager.Instance.CurrentLanguage);
        }

        private void SaveSetting()
        {
            SaveManager.Instance.SaveSetting(ValueToLanguage(languageDropdown.value),volumeSlider.value/100f);
            TranslationManager.Instance.SetLanguage(ValueToLanguage(languageDropdown.value));
            SettingLoader.Instance.LoadSaveData();
            MusicManager.Instance.SetVolume(volumeSlider.value/100f);
        }

        private void AiSetting()
        {
            UIManager.Instance.ShowAISettingPanel();
        }

        private string ValueToLanguage(int input)
        {
            return input switch
            {
                0 => "English",
                1 => "Chinese",
                _ => "English"
            };
        }
        private int LanguageToValue(string input)
        {
            return input switch
            {
                "English" => 0,
                "Chinese" => 1,
                _ => 0
            };
        }
    }
}