using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Entity.Buildings;
using Manager;

public class DisasterUI : MonoBehaviour
{
    [Header("Warning Panel")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private Image warningProgressBar;
    [SerializeField] private Image warningBackground;

    [Header("Disaster Panel")]
    [SerializeField] private GameObject disasterPanel;
    [SerializeField] private TextMeshProUGUI disasterText;
    [SerializeField] private Image disasterProgressBar;
    [SerializeField] private Image disasterBackground;

    [Header("Building Info Panel")]
    [SerializeField] private GameObject buildingInfoPanel;
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI buildingHealthText;
    [SerializeField] private TextMeshProUGUI repairCostText;
    [SerializeField] private Button repairButton;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image buildingInfoBackground;

    private Building selectedBuilding;

    private void Start()
    {
        DisasterManager.Instance.OnDisasterWarning += HandleDisasterWarning;
        DisasterManager.Instance.OnDisasterStart += HandleDisasterStart;
        DisasterManager.Instance.OnDisasterEnd += HandleDisasterEnd;

        // 设置UI样式
        SetupUIStyle();

        warningPanel.SetActive(false);
        disasterPanel.SetActive(false);
        buildingInfoPanel.SetActive(false);

        repairButton.onClick.AddListener(OnRepairButtonClicked);
    }

    private void SetupUIStyle()
    {
        // 设置警告面板样式
        if (warningBackground != null)
        {
            warningBackground.color = new Color(1f, 0.5f, 0f, 0.8f); // 橙色半透明
        }
        if (warningProgressBar != null)
        {
            warningProgressBar.color = Color.red;
            warningProgressBar.type = Image.Type.Filled;
            warningProgressBar.fillMethod = Image.FillMethod.Horizontal;
        }
        if (warningText != null)
        {
            warningText.color = Color.white;
            warningText.fontSize = 24;
        }

        // 设置灾害面板样式
        if (disasterBackground != null)
        {
            disasterBackground.color = new Color(1f, 0f, 0f, 0.8f); // 红色半透明
        }
        if (disasterProgressBar != null)
        {
            disasterProgressBar.color = Color.yellow;
            disasterProgressBar.type = Image.Type.Filled;
            disasterProgressBar.fillMethod = Image.FillMethod.Horizontal;
        }
        if (disasterText != null)
        {
            disasterText.color = Color.white;
            disasterText.fontSize = 24;
        }

        // 设置建筑信息面板样式
        if (buildingInfoBackground != null)
        {
            buildingInfoBackground.color = new Color(0f, 0f, 0f, 0.8f); // 黑色半透明
        }
        if (healthBar != null)
        {
            healthBar.color = Color.green;
            healthBar.type = Image.Type.Filled;
            healthBar.fillMethod = Image.FillMethod.Horizontal;
        }
        if (buildingNameText != null)
        {
            buildingNameText.color = Color.white;
            buildingNameText.fontSize = 20;
        }
        if (buildingHealthText != null)
        {
            buildingHealthText.color = Color.white;
            buildingHealthText.fontSize = 18;
        }
        if (repairCostText != null)
        {
            repairCostText.color = Color.white;
            repairCostText.fontSize = 18;
        }
    }

    private void OnDestroy()
    {
        if (DisasterManager.Instance != null)
        {
            DisasterManager.Instance.OnDisasterWarning -= HandleDisasterWarning;
            DisasterManager.Instance.OnDisasterStart -= HandleDisasterStart;
            DisasterManager.Instance.OnDisasterEnd -= HandleDisasterEnd;
        }
    }

    private void Update()
    {
        if (DisasterManager.Instance.IsDisasterActive())
        {
            UpdateDisasterProgress();
        }

        if (selectedBuilding != null)
        {
            UpdateBuildingInfo();
        }
    }

    private void HandleDisasterWarning(string disasterName)
    {
        warningPanel.SetActive(true);
        warningText.text = $"警告：{disasterName}即将发生！";
        warningProgressBar.fillAmount = 0f;
    }

    private void HandleDisasterStart(string disasterName)
    {
        warningPanel.SetActive(false);
        disasterPanel.SetActive(true);
        disasterText.text = $"正在发生：{disasterName}";
        disasterProgressBar.fillAmount = 0f;
    }

    private void HandleDisasterEnd(string disasterName)
    {
        disasterPanel.SetActive(false);
    }

    private void UpdateDisasterProgress()
    {
        if (DisasterManager.Instance.IsDisasterActive())
        {
            disasterProgressBar.fillAmount = DisasterManager.Instance.GetDisasterProgress();
        }
        else
        {
            warningProgressBar.fillAmount = DisasterManager.Instance.GetWarningProgress();
        }
    }

    public void SelectBuilding(Building building)
    {
        selectedBuilding = building;
        buildingInfoPanel.SetActive(true);
        UpdateBuildingInfo();
    }

    private void UpdateBuildingInfo()
    {
        if (selectedBuilding == null)
        {
            buildingInfoPanel.SetActive(false);
            return;
        }

        buildingNameText.text = selectedBuilding.BuildingName;
        buildingHealthText.text = $"健康度：{selectedBuilding.CurrentHealth / selectedBuilding.MaxHealth:P0}";
        repairCostText.text = $"修复费用：${selectedBuilding.GetRepairCost():N0}";
        healthBar.fillAmount = selectedBuilding.CurrentHealth / selectedBuilding.MaxHealth;

        repairButton.interactable = !selectedBuilding.IsRepairing && 
                                  selectedBuilding.CurrentHealth < selectedBuilding.MaxHealth;
    }

    public void UpdateBuildingHealth(Building building)
    {
        if (!building) return;

        // 更新建筑物信息
        buildingNameText.text = building.BuildingName;
        buildingHealthText.text = $"耐久度: {building.CurrentHealth:F0}/{building.MaxHealth:F0}";
        repairCostText.text = $"修复费用: {building.GetRepairCost():F0}";

        // 更新耐久度条
        if (healthBar)
        {
            healthBar.fillAmount = building.CurrentHealth / building.MaxHealth;
        }

        // 更新修复按钮状态
        repairButton.interactable = !building.IsRepairing && building.CurrentHealth < building.MaxHealth;
    }

    private void OnRepairButtonClicked()
    {
        if (selectedBuilding != null)
        {
            selectedBuilding.StartRepair();
            UpdateBuildingHealth(selectedBuilding);
        }
    }
} 