using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Entity
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class SaveData
    {
        [SerializeField][JsonProperty] private int saveID;
        [SerializeField][JsonProperty] private string saveName;
        [SerializeField][JsonProperty] private int currentCityId;
        [SerializeField][JsonProperty] private List<CityData> cityData;
        [SerializeField][JsonProperty] private string playerName;
        [SerializeField][JsonProperty] private float currentTime; // 游戏当前时间
        [SerializeField][JsonProperty] private float timeScale; // 时间流速，1为正常，<1为慢速，>1为加速
        public List<CityData> CityData
        {
            get => cityData; 
            set => cityData = value;
        }

        public string PlayerName
        {
            get => playerName; 
            set => playerName = value;
        }

        public float CurrentTime
        {
            get => currentTime; 
            set => currentTime = value;
        }

        public float TimeScale
        {
            get => timeScale; 
            set => timeScale = value;
        }
    

        public int SaveID
        {
            get => saveID; set => saveID = value; 
        }

        public int CurrentCityId
        {
            get => currentCityId;
            set => currentCityId = value;
        }
    
        public string SaveName
        {
            get => saveName;
            set => saveName = value;
        }
    }
}
