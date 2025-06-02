using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Entity.Disasters;

namespace UI
{
    public class DisasterImpactPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI fireImpactText;
        [SerializeField] private TextMeshProUGUI earthquakeImpactText;
        [SerializeField] private Image fireImpactBar;
        [SerializeField] private Image earthquakeImpactBar;

        private void Awake()
        {
            enabled = true;  // 确保组件被启用
        }

        private void Start()
        {
            // 初始化面板
            if (!panel)
            {
                panel = gameObject;
            }
            
            // 设置进度条样式
            SetupImpactBars();

            // 初始化文本
            if (fireImpactText)
            {
                fireImpactText.text = "火灾影响度: 0%";
            }
            if (earthquakeImpactText)
            {
                earthquakeImpactText.text = "地震影响度: 0%";
            }
        }

        private void SetupImpactBars()
        {
            if (fireImpactBar)
            {
                fireImpactBar.type = Image.Type.Filled;
                fireImpactBar.fillMethod = Image.FillMethod.Horizontal;
                fireImpactBar.color = new Color(1f, 0.3f, 0.3f, 1f); // 红色
                fireImpactBar.fillAmount = 0f;
            }

            if (earthquakeImpactBar)
            {
                earthquakeImpactBar.type = Image.Type.Filled;
                earthquakeImpactBar.fillMethod = Image.FillMethod.Horizontal;
                earthquakeImpactBar.color = new Color(0.8f, 0.4f, 0f, 1f); // 橙色
                earthquakeImpactBar.fillAmount = 0f;
            }
        }

        public void UpdateFireImpact(float impact)
        {
            if (fireImpactBar)
            {
                fireImpactBar.fillAmount = impact;
            }
            if (fireImpactText)
            {
                // 将影响度转换为百分比显示
                float percentage = impact * 100f;
                fireImpactText.text = $"火灾影响度: {percentage:F1}%";
            }
        }

        public void UpdateEarthquakeImpact(float impact)
        {
            if (earthquakeImpactBar)
            {
                earthquakeImpactBar.fillAmount = impact;
            }
            if (earthquakeImpactText)
            {
                // 将影响度转换为百分比显示
                float percentage = impact * 100f;
                earthquakeImpactText.text = $"地震影响度: {percentage:F1}%\n伤害系数: {impact:F2}\n最终伤害: {impact * 1000:F0}";
            }
        }

        public void ShowPanel()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        public void HidePanel()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
} 