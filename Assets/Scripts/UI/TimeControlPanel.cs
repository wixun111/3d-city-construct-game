using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TimeControlPanel : MonoBehaviour
    {
        [SerializeField] private Button fastButton; 
        [SerializeField] private Button pauseButton; 
        [SerializeField] private Button slowButton; 
        [SerializeField] private Sprite playSprite;   // 播放状态图片
        [SerializeField] private Sprite pauseSprite;  // 暂停状态图片
        private void Start()
        {
            // 给按钮添加点击事件
            fastButton.onClick.AddListener(FastTime);
            pauseButton.onClick.AddListener(TogglePause);
            slowButton.onClick.AddListener(SlowTime);
        }

        private void SlowTime()
        {
            var timeScale = TimeManager.Instance.TimeScale;
            if (timeScale <= 1)
            {
                timeScale /= 2;
            }
            else
            {
                timeScale -= 0.5f;
            }
            TimeManager.Instance.TimeScale = timeScale;
        }

        private void TogglePause()
        {
            pauseButton.GetComponentsInChildren<Image>()[1].sprite = TimeManager.Instance.IsPaused ?  pauseSprite : playSprite;
            TimeManager.Instance.TogglePause();
        }

        private void FastTime()
        {
            var timeScale = TimeManager.Instance.TimeScale;
            if (timeScale < 1)
            {
                timeScale *= 2;
            }
            else
            {
                timeScale += 0.5f;
            }
            TimeManager.Instance.TimeScale = timeScale;
        }
    }
}