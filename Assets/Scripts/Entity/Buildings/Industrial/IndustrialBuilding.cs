using System;
using System.Collections.Generic;
using Entity.Buildings;
using Manager;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity.Buildings.Industrial
{
    public class IndustrialBuilding : Building
    {
        [SerializeField] protected int productionInterval; // 每次生产的间隔
        [SerializeField] protected float pollutionLevel;// 污染值
        private void Start()
        {
        }
    }
}