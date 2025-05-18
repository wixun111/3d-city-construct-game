using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] private Button closeButton; 
        [SerializeField] private Button backGameButton; 
        [SerializeField] private Button quitMenuButton; 
        [SerializeField] private Button saveGameButton; 
        [SerializeField] private Button saveBuildingButton; 
        [SerializeField] private Button settingButton; 
        [SerializeField] private Button quitGameButton; 

        private void Start()
        {
            gameObject.SetActive(false);
            // 给按钮添加点击事件
            closeButton.onClick.AddListener(HidePanel);
            backGameButton.onClick.AddListener(BackGame);
            quitMenuButton.onClick.AddListener(QuitMenu);
            settingButton.onClick.AddListener(Setting);
            quitGameButton.onClick.AddListener(QuitGame);
            saveGameButton.onClick.AddListener(SaveGame);
            saveBuildingButton.onClick.AddListener(SaveBuilding);
        }

        private void SaveBuilding()
        {
            SaveManager.Instance.SaveBuilding();
        }

        public void BackGame()
        {
            gameObject.SetActive(false);
            TimeManager.Instance.ResumeGame();
        }

        public void SaveGame()
        {
            gameObject.SetActive(false);
            UIManager.Instance.ShowSavePanel();
        }
        public void QuitMenu()
        {
            gameObject.SetActive(false);
            UIManager.Instance.ShowStartMenu();
            UIManager.Instance.HideTimeControlPanel();
            TimeManager.Instance.HideTime();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void Setting()
        {
            UIManager.Instance.ShowSettingPanel();
            UIManager.Instance.HideGameMenu();
        }
        private void HidePanel()
        {
            TimeManager.Instance.ResumeGame();
            gameObject.SetActive(false);
        }
    }
}