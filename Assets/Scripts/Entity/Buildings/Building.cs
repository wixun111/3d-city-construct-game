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

        private void Start() { currentHealth = maxHealth; }
        public virtual void Construct()
        {
            Debug.Log(buildingName + " is constructed!");
        }
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0) DestroyBuilding();
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
                    }else
                    {
                        convertedValue = JsonConvert.DeserializeObject(value.ToString(), field.FieldType);
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
                   $"位置: {position}\n";
            // $"可生产性: {isProductive}\n" +
            // $"生产速率: {productionRate}\n" +
            // $"生产类型: {string.Join(", ", produceResourceType)}\n" +
            // $"建筑消耗: {string.Join(", ", cost.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}";
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