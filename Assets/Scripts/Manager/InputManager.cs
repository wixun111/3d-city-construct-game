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
            if (Input.GetKeyDown(KeyCode.R))
            {
                BuildManager.Instance.RotatingBuilding();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                BuildManager.Instance.ChangeBuildingStyle();
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameManager.Instance.HandleMouseClick();
            }

            if (BuildManager.Instance.IsBuildMode())
            {   
                GameManager.Instance.HandleMouseMove();
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