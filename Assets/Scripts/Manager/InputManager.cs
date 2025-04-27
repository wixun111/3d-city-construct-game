using UnityEngine;
namespace Manager
{
    public class InputManager : Singleton<InputManager>
    {
        private void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu();
            }
            // 未来可以扩展更多键盘输入
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameManager.Instance.HandleClick();
            }
            // 未来可以扩展鼠标右键等输入
        }

        private void ToggleMenu()
        {
            var isActive = UIManager.Instance.GetGameMenuActive();
            if (isActive)
            {
                TimeManager.Instance.ResumeGame();
                UIManager.Instance.HideGameMenu();
                // UIManager.Instance.ShowTimeControlPanel();
            }
            else
            {
                TimeManager.Instance.PauseGame();
                // UIManager.Instance.HideTimeControlPanel();
                UIManager.Instance.ShowGameMenu();
            }
        }
    }
}