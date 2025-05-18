using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartMenu : MonoBehaviour
    {
        [SerializeField] private Button startButton; 
        [SerializeField] private Button loadButton; 
        [SerializeField] private Button quitButton;

        private void Start()
        {
            // 给按钮添加点击事件
            startButton.onClick.AddListener(StartGame);
            loadButton.onClick.AddListener(LoadGame);
            quitButton.onClick.AddListener(QuitGame);
        }
        
        public void StartGame()
        {
            gameObject.SetActive(false);
            GameManager.Instance.StartGame();
        }
        public void LoadGame()
        {
            gameObject.SetActive(false);
            UIManager.Instance.ShowSavePanel();
        }
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}