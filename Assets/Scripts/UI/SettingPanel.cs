using Loader;
using Manager;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingPanel : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider; 
        [SerializeField] private Button aiSettingButton; 
        [SerializeField] private Button confirmButton;
        private void Start()
        {
            // 给按钮添加点击事件
            confirmButton.onClick.AddListener(SaveSetting);
            aiSettingButton.onClick.AddListener(AiSetting);
            volumeSlider.value = SettingLoader.Instance.Setting.Volume;
        }

        private void SaveSetting()
        {
            SaveManager.Instance.SaveSetting(volumeSlider.value/100f);
            SettingLoader.Instance.LoadSaveData();
            MusicManager.Instance.SetVolume(volumeSlider.value/100f);
        }

        private void AiSetting()
        {
            UIManager.Instance.ShowAISettingPanel();
        }
    }
}