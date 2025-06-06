﻿using Newtonsoft.Json;
using UnityEngine;

namespace Entity.Buildings
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class BuildingData
    {
        [SerializeField][JsonProperty] protected string uuid;
        [SerializeField][JsonProperty] protected string buildingName;
        [SerializeField][JsonProperty] protected int buildingId;
        [SerializeField][JsonProperty] protected int level;
        [SerializeField][JsonProperty] protected float currentHealth;
        [SerializeField][JsonProperty] protected Vector3Int position;
        [SerializeField][JsonProperty] protected float[] rotation;
        [SerializeField][JsonProperty] protected int style;
        public BuildingData(){}
        public BuildingData(Building building)
        {
            uuid = building.Uuid;
            buildingName = building.BuildingName;
            buildingId = building.BuildingId;
            level = building.Level;
            currentHealth = building.CurrentHealth;
            position = building.Position;
            rotation = new[] {
                building.Rotation.eulerAngles.x,
                building.Rotation.eulerAngles.y,
                building.Rotation.eulerAngles.z
            };
            style = building.Style;
        }
        public string Uuid
        {
            get => uuid;
            set => uuid = value;
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

        public float[] Rotation
        {
            get => rotation;
            set => rotation = value;
        }
        public float CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = value;
        }
    }
}