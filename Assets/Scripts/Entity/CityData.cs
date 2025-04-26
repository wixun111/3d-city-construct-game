using System.Collections.Generic;
using Entity.Buildings;
using Newtonsoft.Json;
using UnityEngine;

namespace Entity
{
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class CityData
    {
        [SerializeField][JsonProperty] private int cityId;
        [SerializeField][JsonProperty] private string cityName;
        [SerializeField][JsonProperty] private int population;
        [SerializeField][JsonProperty] private Dictionary<string,int> resources;
        [SerializeField][JsonProperty] private int economy;
        [SerializeField][JsonProperty] private int width;
        [SerializeField][JsonProperty] private int length;
        [SerializeField][JsonProperty] private int cityLevel;
        [SerializeField][JsonProperty] private List<BuildingData> buildingsData;
        

        public CityData()
        {
        }

        public CityData(City city)
        {
            cityId = city.CityId;
            cityName = city.CityName;
            population = city.Population;
            resources = city.Resources;
            economy = city.Economy;
            width = city.Width;
            length = city.Length;
            cityLevel = city.CityLevel;
            buildingsData = new List<BuildingData>();
            var buildings = city.Buildings;
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var building = buildings[i, j];
                    if (!building) continue;
                    buildingsData.Add(new BuildingData(building));
                }
            }
        }

        public string CityName
        {
            get => cityName;
            set => cityName = value;
        }

        public int CityId
        {
            get => cityId;
            set => cityId = value;
        }

        public int Population
        {
            get => population;
            set => population = value;
        }

        public int Economy
        {
            get => economy;
            set => economy = value;
        }
        public int Length
        {
            get => length;
            set => length = value;
        }
        public int Width
        {
            get => width;
            set => width = value;
        }
        public Dictionary<string, int> Resources
        {
            get => resources;
            set => resources = value;
        }

        public List<BuildingData> BuildingsData
        {
            get => buildingsData;
            set => buildingsData = value;
        }
    }
}