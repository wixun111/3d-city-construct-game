using UnityEngine;

namespace Entity.Buildings.Industrial
{
    public class LoggingCamp : IndustrialBuilding
    {
        private void Awake()
        {
            // 设定 LoggingCamp 的独有属性
            produceResourceType.Add("wood"); // 生产木材
            productionRate = 5;   // 每次生产 5 个木材
        }
    }   
}