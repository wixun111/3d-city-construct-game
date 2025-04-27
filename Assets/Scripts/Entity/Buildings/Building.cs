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
        [SerializeField] private string uuid;
        [SerializeField] protected string buildingName;
        [SerializeField] protected int buildingId;
        protected Dictionary<string,int> cost; 
        [SerializeField] protected int level;
        [SerializeField] protected int maxLevel;
        [SerializeField] protected int maxHealth;
        [SerializeField] protected int currentHealth;
        [SerializeField] protected Vector3Int position;
        [SerializeField] protected bool isProductive;
        [SerializeField] protected List<string> produceResourceType;
        [SerializeField] protected float productionRate; // 每次生产的资源数量
        [SerializeField] private int[] size;
        [SerializeField] private int existLimit;
        
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

        public int CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = value;
        }

        public int MaxHealth
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
                   $"等级: {level}\n" +
                   $"耐久值: {currentHealth}/{maxHealth}\n" +
                   $"位置: {position}\n";
            // $"可生产性: {isProductive}\n" +
            // $"生产速率: {productionRate}\n" +
            // $"生产类型: {string.Join(", ", produceResourceType)}\n" +
            // $"建筑消耗: {string.Join(", ", cost.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}";
        }
    }
}