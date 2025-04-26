using Entity.Buildings;

namespace Entity.Buildings.Commercial
{
    public class CommercialBuilding:Building
    {
        private int _employeeNumber;
        private int _employeeCapacity;

        public void AddEmployee(int newEmployees)
        {
            if (_employeeCapacity >= _employeeNumber + newEmployees)
            {
                _employeeNumber += newEmployees;
            }
        }
    }
}