using System;
using System.Collections.Generic;
using System.Reflection;
using Loader;
using Newtonsoft.Json;
using UnityEngine;

namespace Entity.Buildings
{
    public class Building : MonoBehaviour
    {
        //基本属性
        [SerializeField] private string uuid; 
        [SerializeField] protected string buildingName;
        [SerializeField] protected int buildingId;
        [SerializeField] protected string buildingType;
        protected Dictionary<string,int> cost; 
        [SerializeField] protected int level;
        [SerializeField] protected int maxLevel;
        [SerializeField] protected float maxHealth;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected Vector3Int position;
        [SerializeField] protected Quaternion rotation;
        [SerializeField] protected bool buildable;             
        [SerializeField] private int[] size;
        [SerializeField] private int existLimit;
        //建筑属性
        [SerializeField] protected bool isProductive;
        [SerializeField] protected List<string> produceResourceType;
        [SerializeField] protected float productionRate;        // 每次生产的资源数量
        [SerializeField] protected float maintenanceCost;       // 每回合/时间单位维护消耗资源
        [SerializeField] protected float upkeepPower;           // 需要消耗的电力
        [SerializeField] protected float upkeepWater;           // 需要消耗的水
        [SerializeField] protected float pollution;             // 该建筑造成的污染值
        [SerializeField] protected bool needsRoadAccess;

        // 灾难系统属性
        [SerializeField] private GameObject[] damageStates;     // 不同损坏程度的模型
        [SerializeField] private ParticleSystem smokeEffect;    // 烟雾效果
        [SerializeField] private ParticleSystem fireEffect;     // 火灾效果
        [SerializeField] private float repairCost = 1000f;      // 基础修复费用
        [SerializeField] private float repairTime = 30f;        // 修复时间
        private bool isRepairing;
        private int currentDamageState;
        private bool isOnFire;

        private void Awake()
        {
            if (string.IsNullOrEmpty(uuid)) // 只在第一次初始化时生成
            {
                uuid = Guid.NewGuid().ToString();
            }
        }
        public void Load(BuildingData buildingData)
        {
            buildingId = buildingData.BuildingId;
            buildingName = buildingData.BuildingName;
            level = buildingData.Level;
            currentHealth = buildingData.CurrentHealth;
            position = buildingData.Position;
            rotation = Quaternion.Euler(buildingData.Rotation[0], buildingData.Rotation[1], buildingData.Rotation[2]);
            LoadData(BuildingLoader.Instance.GetBuildingData(buildingId));
        }

        private void Start() { currentHealth = maxHealth; UpdateDamageState(); }
        public virtual void Construct()
        {
            Debug.Log(buildingName + " is constructed!");
        }
        public void TakeDamage(float damage)
        {
            if (isRepairing) return;

            float oldHealth = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            Debug.Log($"{buildingName} 受到伤害: {damage}, 健康值变化: {oldHealth} -> {currentHealth}, 最大健康值: {maxHealth}");

            UpdateDamageState();

            // 如果损坏严重，可能引发火灾
            if (currentHealth < maxHealth * 0.3f && !isOnFire)
            {
                StartFire();
            }

            if (currentHealth <= 0) DestroyBuilding();

            // 通知 UI 更新
            var disasterUI = FindObjectOfType<DisasterUI>();
            if (disasterUI != null)
            {
                disasterUI.UpdateBuildingHealth(this);
            }
        }
        private void DestroyBuilding()
        {
            Debug.Log(buildingName + " 被摧毁！");
            Destroy(gameObject);
        }

        public void InitData(Dictionary<string, object> data,Vector3Int pos)
        {
            LoadData(data);
            currentHealth = maxHealth;
            if (existLimit != 0)
            {
                existLimit = 999;
            }
            level = 1;
            position = pos;
            rotation = gameObject.transform.rotation;
        }

        public void LoadData(Dictionary<string, object> data)
        {
            var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (!data.TryGetValue(field.Name, out var value)) continue;

                try
                {
                    object convertedValue;
                    if (field.FieldType == typeof(string))
                    {
                        convertedValue = Convert.ToString(value);
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        convertedValue = Convert.ToBoolean(value);
                    }
                    else if (field.FieldType == typeof(int[]))
                    {
                        var jsonStr = JsonConvert.SerializeObject(value);
                        convertedValue = JsonConvert.DeserializeObject<int[]>(jsonStr);
                    }
                    else if (field.FieldType == typeof(float[]))
                    {
                        var jsonStr = JsonConvert.SerializeObject(value);
                        convertedValue = JsonConvert.DeserializeObject<float[]>(jsonStr);
                    }
                    else
                    {
                        var jsonStr = JsonConvert.SerializeObject(value);
                        convertedValue = JsonConvert.DeserializeObject(jsonStr, field.FieldType);
                    }
                    field.SetValue(this, convertedValue);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error converting field {field.Name}: {ex.Message}");
                }
            }
            buildingType = data["classType"].ToString().Split(".")[1];
        }
        public bool IsProductive
        {
            get => isProductive;
            set => isProductive = value;
        }
        public List<string> ProduceResourceType
        {
            get => produceResourceType;
            set => produceResourceType = value;
        }

        public float ProductionRate
        {
            get => productionRate;
            set => productionRate = value;
        }

        public string BuildingName
        {
            get => buildingName;
            set => buildingName = value;
        }

        public int BuildingId
        {
            get => buildingId;
            set => buildingId = value;
        }

        public int Level
        {
            get => level;
            set => level = value;
        }

        public Vector3Int Position
        {
            get => position;
            set => position = value;
        }

        public Quaternion Rotation
        {
            get => rotation;
            set => rotation = value;
        }
        public float CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = value;
        }

        public float MaxHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }
        public int[] Size
        {
            get => size;
            set => size = value;
        }
        public string Uuid => uuid;

        public bool IsOnFire => isOnFire;
        public bool IsRepairing => isRepairing;

        public int MaxLevel
        {
            get => maxLevel;
            set => maxLevel = value;
        }

        public float GetRepairCost()
        {
            return repairCost * (1f - (currentHealth / maxHealth));
        }

        public virtual void Upgrade()
        {
            if(level >= maxLevel) return;
            level++;
            maxHealth += 10;
            currentHealth += 10;
        }

        public virtual string GetBuildingInfo()
        {
            return $"建筑名: {buildingName}\n" +
                   $"建筑类别: {buildingType}\n" +
                   $"等级: {level}\n" +
                   $"耐久值: {currentHealth}/{maxHealth}\n" +
                   $"位置: {position}\n" +
                   $"状态: {(isOnFire ? "着火" : isRepairing ? "修复中" : "正常")}\n";
        }

        private void UpdateDamageState()
        {
            float healthPercentage = currentHealth / maxHealth;
            int newState = 0;

            if (healthPercentage < 0.25f)
                newState = 3;  // 严重损坏
            else if (healthPercentage < 0.5f)
                newState = 2;  // 中度损坏
            else if (healthPercentage < 0.75f)
                newState = 1;  // 轻微损坏

            if (newState != currentDamageState)
            {
                currentDamageState = newState;
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (damageStates == null || damageStates.Length == 0) return;

            // 隐藏所有损坏状态模型
            foreach (var state in damageStates)
            {
                if (state != null)
                    state.SetActive(false);
            }

            // 显示当前损坏状态模型
            if (currentDamageState < damageStates.Length && damageStates[currentDamageState] != null)
            {
                damageStates[currentDamageState].SetActive(true);
            }
        }

        [ContextMenu("触发火灾")]
        public void TriggerFire()
        {
            if (!isOnFire)
            {
                StartFire();
            }
        }

        private void StartFire()
        {
            isOnFire = true;
            
            // 如果没有设置粒子效果，创建默认的
            if (fireEffect == null)
            {
                CreateDefaultFireEffect();
            }
            if (smokeEffect == null)
            {
                CreateDefaultSmokeEffect();
            }

            if (fireEffect != null)
                fireEffect.Play();
            if (smokeEffect != null)
                smokeEffect.Play();

            StartCoroutine(FireDamageRoutine());
        }

        private void CreateDefaultFireEffect()
        {
            // 创建火焰粒子系统
            GameObject fireObj = new GameObject("FireEffect");
            fireObj.transform.SetParent(transform);
            fireObj.transform.localPosition = new Vector3(0, 1, 0); // 在建筑顶部

            fireEffect = fireObj.AddComponent<ParticleSystem>();
            var main = fireEffect.main;
            main.startColor = new Color(1f, 0.5f, 0f); // 橙色火焰
            main.startSize = 0.5f;
            main.startSpeed = 2f;
            main.startLifetime = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = fireEffect.emission;
            emission.rateOverTime = 20;

            var shape = fireEffect.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            // 添加颜色渐变
            var colorOverLifetime = fireEffect.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.yellow, 0.0f),
                    new GradientColorKey(Color.red, 0.5f),
                    new GradientColorKey(Color.red, 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 0.5f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = gradient;
        }

        private void CreateDefaultSmokeEffect()
        {
            // 创建烟雾粒子系统
            GameObject smokeObj = new GameObject("SmokeEffect");
            smokeObj.transform.SetParent(transform);
            smokeObj.transform.localPosition = new Vector3(0, 2, 0); // 在火焰上方

            smokeEffect = smokeObj.AddComponent<ParticleSystem>();
            var main = smokeEffect.main;
            main.startColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 灰色烟雾
            main.startSize = 1f;
            main.startSpeed = 1f;
            main.startLifetime = 2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = smokeEffect.emission;
            emission.rateOverTime = 10;

            var shape = smokeEffect.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            // 添加颜色渐变
            var colorOverLifetime = smokeEffect.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(0.5f, 0.5f, 0.5f, 0.5f), 0.0f),
                    new GradientColorKey(new Color(0.5f, 0.5f, 0.5f, 0.0f), 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.5f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = gradient;
        }

        private System.Collections.IEnumerator FireDamageRoutine()
        {
            while (isOnFire && currentHealth > 0)
            {
                TakeDamage(5f); // 每秒造成5点伤害
                yield return new WaitForSeconds(1f);
            }
        }

        public void StartRepair()
        {
            if (isRepairing || currentHealth >= maxHealth) return;

            isRepairing = true;
            StartCoroutine(RepairRoutine());
        }

        private System.Collections.IEnumerator RepairRoutine()
        {
            if (isOnFire)
            {
                isOnFire = false;
                if (fireEffect != null)
                    fireEffect.Stop();
                if (smokeEffect != null)
                    smokeEffect.Stop();
            }

            float repairStartTime = Time.time;
            float initialHealth = currentHealth;

            while (Time.time < repairStartTime + repairTime)
            {
                float progress = (Time.time - repairStartTime) / repairTime;
                currentHealth = Mathf.Lerp(initialHealth, maxHealth, progress);
                UpdateDamageState();
                yield return null;
            }

            currentHealth = maxHealth;
            UpdateDamageState();
            isRepairing = false;
        }
    }
    public enum BuildingCategory
    {
        Residential,
        Cultural,
        Commercial,
        Industrial,
        Governmental,
        Medical,
        Natural,
        Entertainment,
        Transport,
        Special,
        Other
    }
}