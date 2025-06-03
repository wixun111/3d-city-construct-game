using UnityEngine;
using System.Collections.Generic;
using Entity.Buildings;
using Manager;

public class Earthquake : MonoBehaviour
{
    [SerializeField] private float maxShakeIntensity = 0.5f;
    [SerializeField] private float shakeSpeed = 10f;
    [SerializeField] private float damageThreshold = 0.3f;
    
    private List<Building> affectedBuildings = new List<Building>();
    private float currentIntensity;
    private Vector3 originalPosition;
    private bool isActive;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        originalPosition = mainCamera.transform.position;
        DisasterManager.Instance.OnDisasterStart += HandleDisasterStart;
        DisasterManager.Instance.OnDisasterEnd += HandleDisasterEnd;
    }

    private void OnDestroy()
    {
        if (DisasterManager.Instance != null)
        {
            DisasterManager.Instance.OnDisasterStart -= HandleDisasterStart;
            DisasterManager.Instance.OnDisasterEnd -= HandleDisasterEnd;
        }
    }

    private void HandleDisasterStart(string disasterName)
    {
        if (disasterName == "Earthquake")
        {
            isActive = true;
            FindAffectedBuildings();
        }
    }

    private void HandleDisasterEnd(string disasterName)
    {
        if (disasterName == "Earthquake")
        {
            isActive = false;
            mainCamera.transform.position = originalPosition;
            affectedBuildings.Clear();
        }
    }

    private void Update()
    {
        if (!isActive) return;

        float progress = DisasterManager.Instance.GetDisasterProgress();
        currentIntensity = Mathf.Lerp(maxShakeIntensity, 0f, progress);

        ApplyCameraShake();
        CheckBuildingDamage();
    }

    private void ApplyCameraShake()
    {
        float x = Mathf.PerlinNoise(Time.time * shakeSpeed, 0) * 2 - 1;
        float y = Mathf.PerlinNoise(0, Time.time * shakeSpeed) * 2 - 1;
        float z = Mathf.PerlinNoise(Time.time * shakeSpeed, Time.time * shakeSpeed) * 2 - 1;

        Vector3 shakeOffset = new Vector3(x, y, z) * currentIntensity;
        mainCamera.transform.position = originalPosition + shakeOffset;
    }

    private void FindAffectedBuildings()
    {
        Building[] allBuildings = FindObjectsOfType<Building>();
        Debug.Log($"场景中找到 {allBuildings.Length} 个建筑物");
        
        var config = DisasterManager.Instance.GetDisasterConfig("Earthquake");
        if (config == null)
        {
            Debug.LogError("未找到地震配置！");
            return;
        }

        Debug.Log($"地震位置: {transform.position}, 影响半径: {config.damageRadius}");
        
        foreach (Building building in allBuildings)
        {
            float distance = Vector3.Distance(transform.position, building.transform.position);
            Debug.Log($"建筑物 {building.BuildingName} 位置: {building.transform.position}, 距离: {distance}");
            
            if (distance <= config.damageRadius)
            {
                affectedBuildings.Add(building);
                Debug.Log($"建筑物 {building.BuildingName} 在地震影响范围内");
            }
        }
        
        Debug.Log($"最终找到 {affectedBuildings.Count} 个受影响的建筑物");
    }

    private void CheckBuildingDamage()
    {
        var config = DisasterManager.Instance.GetDisasterConfig("Earthquake");
        if (config == null) return;

        Debug.Log($"地震检测到 {affectedBuildings.Count} 个建筑物");
        float maxDamageFactor = 0f;
        float totalDamage = 0f;

        foreach (Building building in affectedBuildings)
        {
            if (building != null)
            {
                var distance = Vector3.Distance(transform.position, building.transform.position);
                var damageFactor = 1f - (distance / config.damageRadius);
                var damage = currentIntensity * damageFactor * config.damageIntensity;

                Debug.Log($"建筑物 {building.BuildingName} 距离: {distance}, 伤害系数: {damageFactor}, 最终伤害: {damage}");

                maxDamageFactor = Mathf.Max(maxDamageFactor, damageFactor);
                totalDamage += damage;

                if (damage > damageThreshold)
                {
                    building.TakeDamage(damage);
                }
            }
        }

        // 计算地震影响度 (0-1之间)
        float earthquakeImpact = Mathf.Clamp01((maxDamageFactor + totalDamage / 1000f) / 2f);
        
        // 通过 UIManager 更新UI
        UIManager.Instance.UpdateDisasterImpact(0f, earthquakeImpact);
    }
} 