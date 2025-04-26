using Entity.Buildings;
using UnityEngine;

namespace Entity.Buildings.Residential
{
    public class ResidentialBuilding:Building
    {
        [SerializeField] protected int residentsNumber;
        [SerializeField] protected int residentsCapacity;

        public void AddResidents(int newResidents)
        {
            if (residentsCapacity >= residentsNumber + newResidents)
            {
                residentsNumber += newResidents;   
            }
        }
    }
}