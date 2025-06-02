using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Manager
{
    public class DisasterManager : Singleton<DisasterManager>
    {
        [Serializable]
        public class DisasterConfig
        {
            public string disasterName;
            public float minInterval = 300f;
            public float maxInterval = 900f;
            public float warningTime = 30f;
            public float duration = 60f;
            public float damageRadius = 50f;
            public float damageIntensity = 0.5f;
        }

        [SerializeField] private List<DisasterConfig> disasterConfigs = new List<DisasterConfig>
        {
            new DisasterConfig
            {
                disasterName = "Earthquake",
                minInterval = 300f,
                maxInterval = 900f,
                warningTime = 30f,
                duration = 60f,
                damageRadius = 50f,
                damageIntensity = 0.5f
            }
        };
        private Dictionary<string, DisasterConfig> disasterConfigDict;
        [SerializeField] private float time;
        [SerializeField] private float nextDisasterTime;
        [SerializeField] private string currentDisaster;
        [SerializeField] private float disasterStartTime;
        [SerializeField] private bool isDisasterActive;
        [SerializeField] private float warningStartTime;

        public event Action<string> OnDisasterWarning;
        public event Action<string> OnDisasterStart;
        public event Action<string> OnDisasterEnd;

        private void Start()
        {
            InitializeDisasterConfigs();
            // TriggerRandomDisaster();
        }

        private void InitializeDisasterConfigs()
        {
            disasterConfigDict = new Dictionary<string, DisasterConfig>();
            foreach (var config in disasterConfigs)
            {
                disasterConfigDict[config.disasterName] = config;
            }
        }

        private void Update()
        {
            time = TimeManager.Instance.CurrentTime;
            if (!isDisasterActive)
            {
                if (time >= nextDisasterTime)
                {
                    TriggerRandomDisaster();
                }
            }
            else
            {
                if (time >= disasterStartTime + GetCurrentDisasterConfig().duration)
                {
                    EndCurrentDisaster();
                }
            }
        }

        private void TriggerRandomDisaster()
        {
            if (disasterConfigs.Count == 0) return;

            int randomIndex = UnityEngine.Random.Range(0, disasterConfigs.Count);
            currentDisaster = disasterConfigs[randomIndex].disasterName;
            warningStartTime = time;
            isDisasterActive = true;

            OnDisasterWarning?.Invoke(currentDisaster);
            Invoke(nameof(StartDisaster), GetCurrentDisasterConfig().warningTime);
        }

        private void StartDisaster()
        {
            disasterStartTime = time;
            OnDisasterStart?.Invoke(currentDisaster);
            ApplyDisasterEffects();
        }

        private void EndCurrentDisaster()
        {
            isDisasterActive = false;
            OnDisasterEnd?.Invoke(currentDisaster);
            currentDisaster = null;
            ScheduleNextDisaster();
        }

        private void ScheduleNextDisaster()
        {
            if (disasterConfigs.Count == 0) return;

            var minInterval = disasterConfigs.Aggregate(float.MaxValue, (current, config) => Mathf.Min(current, current));

            nextDisasterTime = time + minInterval;
        }

        private void ApplyDisasterEffects()
        {
            // 具体灾难效果由各个灾难类实现
        }

        private DisasterConfig GetCurrentDisasterConfig()
        {
            return disasterConfigDict[currentDisaster];
        }

        public bool IsDisasterActive()
        {
            return isDisasterActive;
        }

        public string GetCurrentDisaster()
        {
            return currentDisaster;
        }

        public float GetDisasterProgress()
        {
            if (!isDisasterActive) return 0f;
            return (time - disasterStartTime) / GetCurrentDisasterConfig().duration;
        }

        public float GetWarningProgress()
        {
            if (!isDisasterActive || time >= disasterStartTime) return 1f;
            return (time - warningStartTime) / GetCurrentDisasterConfig().warningTime;
        }

        public DisasterConfig GetDisasterConfig(string disasterName)
        {
            return disasterConfigDict.TryGetValue(disasterName, out var config) ? config : null;
        }
    }
} 