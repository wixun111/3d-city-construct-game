using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    public class TimeManager : Singleton<TimeManager>
    {
        [SerializeField] private float currentTime = 0f; // 游戏当前时间
        [SerializeField] private float timeScale = 1f; // 时间流速，1为正常，<1为慢速，>1为加速
        [SerializeField] private bool isPaused = true; // 是否暂停游戏时间
        [SerializeField] private Text gameTimeText;
        [SerializeField] private int lastYear, lastMonth;
        // 实例初始化
        private void Start()
        {
            InitTime();
            PauseGame();
        }
        // 每秒更新时间
        private void Update()
        {
            if (!isPaused)
            {
                // 增加时间，使用 Time.deltaTime 与 timeScale 控制时间流逝
                currentTime += Time.deltaTime * timeScale;
            }
            // 假设游戏时间 currentTime 以秒为单位
            // 计算年月日
            var totalMonths = Mathf.FloorToInt(currentTime / 60); // 计算总月份数
            var year = 2000 + (totalMonths / 12); // 从 2000 年开始
            var month = (totalMonths % 12) + 1; // 计算当前月份（1-12）

            // 仅当日期发生变化时更新 UI
            if (year == lastYear && month == lastMonth) return;
            gameTimeText.text = $"{year}-{month:D2}";
            lastYear = year;
            lastMonth = month;
        }

        public void ShowTime()
        {
            gameTimeText.gameObject.SetActive(true);
        }
        public void HideTime()
        {
            gameTimeText.gameObject.SetActive(false);
        }
        // 时间初始化
        public void InitTime()
        {
            currentTime = 0f;
            timeScale = 1f;
            isPaused = false;
        }
        // 暂停游戏时间
        public void PauseGame()
        {
            isPaused = true;
        }
        public void TogglePause()
        {
            isPaused = !isPaused;
        }
        // 恢复游戏时间
        public void ResumeGame()
        {
            isPaused = false;
        }

        // 加速时间流逝
        public void FastForward()
        {
            timeScale = 2f; // 加速2倍
        }

        // 恢复正常时间流逝
        public void NormalSpeed()
        {
            timeScale = 1f; // 恢复正常
        }

        // 设置倒计时
        public void SetCountdown(float countdownTime, System.Action onCountdownEnd)
        {
            StartCoroutine(CountdownTimer(countdownTime, onCountdownEnd));
        }

        // 倒计时函数
        private IEnumerator CountdownTimer(float countdownTime, System.Action onCountdownEnd)
        {
            var countdownStartTime = currentTime + countdownTime;

            while (currentTime < countdownStartTime)
            {
                yield return null;
            }

            // 倒计时结束后执行
            onCountdownEnd?.Invoke();
        }

        public float CurrentTime
        {
            get => currentTime;
            set => currentTime = value;
        }
        public float TimeScale
        {
            get => timeScale;
            set => timeScale = value;
        }
        public bool IsPaused
        {
            get => isPaused;
            set => isPaused = value;
        }

        public void Load(float currentTime, float timeScale)
        {
            gameTimeText.gameObject.SetActive(true);
            this.currentTime = currentTime;
            this.timeScale = timeScale;
        }
    }
}