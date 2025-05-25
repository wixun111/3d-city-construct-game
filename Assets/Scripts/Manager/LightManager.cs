using UnityEngine;

namespace Manager
{
    public class LightManager : Singleton<LightManager>
    {
        [SerializeField] private Material daySkybox; // 白天天空盒
        [SerializeField] private Material nightSkybox; // 夜晚天空盒
        [SerializeField] private Light sunLight; // 太阳光（Directional Light）
        [SerializeField] private Light moonLight; // 月光（Directional Light）
        [SerializeField] private float dayIntensity; // 白天光照强度
        [SerializeField] private float nightIntensity; // 夜晚光照强度
        [SerializeField] private float dayDuration ; // // 一天的持续时间（秒）
        private void Awake()
        {
            dayIntensity = 1.0f;
            nightIntensity = 0.5f;
            dayDuration = 360f;
        }
        private void Update()
        {
            UpdateDayNightCycle();
        }
        public void SetSunlightIntensity(float intensity)
        {
            if (sunLight != null)
            {
                sunLight.intensity = intensity;
            }
        }
        private void UpdateDayNightCycle()
        {
            float normalizedTime = (TimeManager.Instance.CurrentTime % dayDuration) / dayDuration;
        
            // 太阳光旋转
            var rotationAngle = normalizedTime * 360f + 30f; // -90° 让太阳从东方升起
            sunLight.transform.rotation = Quaternion.Euler(rotationAngle, 0f, 0f);

            // 调整光照强度（白天亮、夜晚暗）
            var intensityFactor = Mathf.Clamp01(Mathf.Sin(normalizedTime * Mathf.PI)); // 正弦曲线模拟光照变化
            sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, intensityFactor);

            // 月亮光照强度，反向操作
            moonLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, intensityFactor);
            
            // 调整月亮的旋转与太阳相反，保证白天和夜晚交替
            moonLight.transform.rotation = Quaternion.Euler(rotationAngle + 180f, 0f, 0f); // 让月亮与太阳相对
            // 根据时间切换天空盒
            if (normalizedTime < 0.5f | normalizedTime>0.8f) // 白天
            {
                RenderSettings.skybox = daySkybox;
                sunLight.enabled = true;
                moonLight.enabled = false;
            }
            else // 夜晚
            {
                RenderSettings.skybox = nightSkybox;
                sunLight.enabled = false;
                moonLight.enabled = true;
            };
        }
    }
}