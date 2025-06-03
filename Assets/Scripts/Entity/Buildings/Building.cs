using System;
using System.Collections.Generic;
using System.Reflection;
using Loader;
using Manager;
using Newtonsoft.Json;
using Unity.VisualScripting;
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
        [SerializeField] private int style;
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
            smokeEffect = ParticleManager.Instance.GetParticleSystem(Constant.SmokeEffect);
            fireEffect = ParticleManager.Instance.GetParticleSystem(Constant.FireEffect);
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

            var oldHealth = currentHealth;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
            Debug.Log($"{buildingName} 受到伤害: {damage}, 健康值变化: {oldHealth} -> {currentHealth}, 最大健康值: {maxHealth}");

            UpdateDamageState();

            // 如果损坏严重，可能引发火灾
            if (currentHealth < maxHealth * 0.3f && !isOnFire) {
                StartFire();
            }

            if (currentHealth <= 0) DestroyBuilding();

            // 通知 UI 更新
            var disasterUI = FindObjectOfType<DisasterUI>();
            if (disasterUI) {
                disasterUI.UpdateBuildingHealth(this);
            }
        }
        private void DestroyBuilding()
        {
            Debug.Log(buildingName + " 被摧毁！");
            Destroy(gameObject);
        }

        public void InitData(Dictionary<string, object> data,Vector3Int pos,int style)
        {
            LoadData(data);
            this.style = style;
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

        public int Style
        {
            get => style;
            set => style = value;
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
                   $"样式: {style}\n" +
                   $"状态: {(isOnFire ? "着火" : isRepairing ? "修复中" : "正常")}\n";
        }

        private void UpdateDamageState()
        {
            var healthPercentage = currentHealth / maxHealth;

            var newState = healthPercentage switch
            {
                < 0.25f => 3,
                < 0.5f => 2,
                < 0.75f => 1,
                _ => 0
            };

            if (newState == currentDamageState) return;
            currentDamageState = newState;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (damageStates == null || damageStates.Length == 0) return;

            // 隐藏所有损坏状态模型
            foreach (var state in damageStates)
            {
                if (state)
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
            if (fireEffect) {
                for (var i = 0; i < size[0]; i++)
                {
                    for (var j = 0; j < size[1]; j++)
                    {
                        var fireObj = new GameObject("FireEffect");
                        fireObj.transform.SetParent(transform);
                        fireObj.transform.localPosition = new Vector3(-i, 0f, -j);
                        var newFireEffect = Instantiate(fireEffect, fireObj.transform);
                        newFireEffect.transform.localPosition = Vector3.zero;
                        fireEffect.Play();
                    }
                }
            }
            if (smokeEffect) {
                for (var i = 0; i < size[0]; i++)
                {
                    for (var j = 0; j < size[1]; j++)
                    {
                        var smokeObj = new GameObject("SmokeEffect");
                        smokeObj.transform.SetParent(transform);
                        smokeObj.transform.localPosition = new Vector3(-i, 0.5f, -j);
                        var newSmokeEffect = Instantiate(smokeEffect, smokeObj.transform);
                        newSmokeEffect.transform.localPosition = Vector3.zero;
                        smokeEffect.Play();
                    }
                }
            }
            StartCoroutine(FireDamageRoutine());
        }

        private System.Collections.IEnumerator FireDamageRoutine()
        {
            while (isOnFire && currentHealth > 0)
            {
                TakeDamage(5f); // 每秒造成5点伤害
                CityManager.Instance.CurrentCity.FireSpread(position);
                yield return new WaitForSeconds(1f/TimeManager.Instance.TimeScale);
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
                if (fireEffect)
                    fireEffect.Stop();
                if (smokeEffect)
                    smokeEffect.Stop();
            }

            var repairStartTime = Time.time;
            var initialHealth = currentHealth;

            while (Time.time < repairStartTime + repairTime)
            {
                var progress = (Time.time - repairStartTime) / repairTime;
                currentHealth = Mathf.Lerp(initialHealth, maxHealth, progress);
                UpdateDamageState();
                yield return null;
            }

            currentHealth = maxHealth;
            UpdateDamageState();
            isRepairing = false;
        }
    }
}