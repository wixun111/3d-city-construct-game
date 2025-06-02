using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Loader;
using UnityEngine;
using UnityEngine.Tilemaps;
using Entity.Buildings;

namespace Manager
{
    public class BuildManager : Singleton<BuildManager>
    {
        [SerializeField] private GameObject previewInstance;
        [SerializeField] private GameObject buildingPrefab;
        [SerializeField] private GameObject roadStraight, deadEnd, corner, threeWay, fourWay;
        [SerializeField] private Dictionary<int, Dictionary<string, object>> buildingData = new Dictionary<int, Dictionary<string, object>>();
        [SerializeField] private int selectedBuildingId;
        [SerializeField] private int styleIndex;
        [SerializeField] private int buildingStyleCount;
        [SerializeField] private Quaternion currentRotation = Quaternion.identity;
        private void Start()
        {
            // 获取建筑数据
            buildingData = BuildingLoader.Instance.GetBuildingData();
            selectedBuildingId = -1;
            buildingStyleCount = 1;
        }

        public void SetBuildingPrefab()
        {
            var buildingName = buildingData[selectedBuildingId]["buildingName"].ToString();
            styleIndex = 1;
            buildingStyleCount = GetBuildingStyleCount(buildingName);
            if (buildingStyleCount == 1) {
                buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/" + buildingName);
            }
            else {
                buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/" + buildingName + styleIndex);
            }
        }
        public int GetBuildingStyleCount(string buildingName)
        {
            var allPrefabs = Resources.LoadAll<GameObject>("Prefabs/Buildings");
            var count = allPrefabs.Count(go => go.name.StartsWith(buildingName));
            return count;
        }
        public void Build()
        {
            // 获取建筑的数据
            if (!buildingData.TryGetValue(selectedBuildingId, value: out var buildingInfo)) return;

            var currentCity = CityManager.Instance.CurrentCity;
            var position = PlaneManager.Instance.GetTilePosition();
            Debug.Log(position);
            // 判断是否可以建造（例如，检查是否有足够资源等）
            if (!currentCity.CanBuild(buildingInfo, position)) return;
            currentCity.Build(buildingInfo,position, SetBuilding(position,currentRotation, (string)buildingInfo["buildingName"]));
            UIManager.Instance.UpdateCityUI(currentCity);
        }

        public GameObject SetBuilding(Vector3 position,Quaternion rotation,string buildingName)
        {
            var buildingObject = InstantiatePrefab(position,rotation,buildingName);
            if (buildingObject != null)
            {
                var building = buildingObject.GetComponent<Entity.Buildings.Building>();
                if (building == null)
                {
                    building = buildingObject.AddComponent<Entity.Buildings.Building>();
                }
            }
            return buildingObject;
        }
        public GameObject SetBuilding(Vector3 position,Quaternion rotation,int buildingId)
        {
            if (buildingId != -1 && buildingData.ContainsKey(buildingId))
            {
                var buildingName = buildingData[buildingId]["buildingName"].ToString();
                var buildingObject = InstantiatePrefab(position,rotation,buildingName);
                if (buildingObject != null)
                {
                    var building = buildingObject.GetComponent<Entity.Buildings.Building>();
                    if (building == null)
                    {
                        building = buildingObject.AddComponent<Entity.Buildings.Building>();
                    }
                }
                return buildingObject;
            }
            else
            {
                // 使用默认建筑
                var buildingObject = InstantiatePrefab(position, rotation, "default");
                if (buildingObject != null)
                {
                    var building = buildingObject.GetComponent<Entity.Buildings.Building>();
                    if (building == null)
                    {
                        building = buildingObject.AddComponent<Entity.Buildings.Building>();
                    }
                }
                return buildingObject;
            }
        }

        private GameObject InstantiatePrefab(Vector3 position, Quaternion rotation, string buildingName) {
            if (buildingStyleCount == 1) {
                buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/" + buildingName);
            }
            else {
                buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/" + buildingName + styleIndex);
            }
            if (!buildingPrefab) {
                buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/default");
            }
            var newBuilding = Instantiate(buildingPrefab, position, rotation);
            newBuilding.transform.SetParent(CityManager.Instance.CurrentCity.gameObject.transform);
            
            // 检查是否已经有 Building 组件
            var building = newBuilding.GetComponent<Entity.Buildings.Building>();
            if (building == null)
            {
                building = newBuilding.AddComponent<Entity.Buildings.Building>();
            }
            
            // 初始化建筑数据
            if (selectedBuildingId != -1 && buildingData.ContainsKey(selectedBuildingId)) {
                building.InitData(buildingData[selectedBuildingId], new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z)));
            } else {
                // 使用默认数据初始化
                var defaultData = new Dictionary<string, object> {
                    { "buildingName", buildingName },
                    { "buildingId", -1 },
                    { "level", 1 },
                    { "maxHealth", 100f },
                    { "currentHealth", 100f },
                    { "buildable", true },
                    { "size", new int[] { 1, 1 } }
                };
                building.InitData(defaultData, new Vector3Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y), Mathf.RoundToInt(position.z)));
            }
            
            return newBuilding;
        }

        public void StartBuildingMode(int buildingId)
        {
            selectedBuildingId = buildingId;
            SetBuildingPrefab();
            previewInstance = Instantiate(buildingPrefab,PlaneManager.Instance.GetTilePosition(), currentRotation);
            DisableCollider();
            Debug.Log("进入建造模式: " + buildingId);
            // 这里可以显示建筑预览，或者改变鼠标光标等
        }
        public void ExitBuildingMode()
        {
            Destroy(previewInstance);
            selectedBuildingId = -1;
            TrafficManager.Instance.InitRoad();
            Debug.Log("退出建造模式");
            // 这里可以隐藏建筑预览等
        }
        public void BuildJudge()
        {
            if (selectedBuildingId == 1)
            {
                Build();
                FixRoads();
            }
            else if (selectedBuildingId != -1)
            {
                Build();
            }
        }

        public void FixRoads(int type = 0)
        {
            var pos = PlaneManager.Instance.GetTilePosition();
            var city = CityManager.Instance.CurrentCity;
            var buildings = city.Buildings;
            var directions = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0),   // 右
                new Vector3Int(0, 0, 1),   // 上
                new Vector3Int(-1, 0, 0),  // 左
                new Vector3Int(0, 0, -1),  // 下
            };
            var width = buildings.GetLength(0);
            var height = buildings.GetLength(1);
            List<Vector3Int> toUpdate;
            if (type == 1)
            {
                toUpdate = new List<Vector3Int>();
                for (var x = 0; x < width; x++)
                {
                    for (var z = 0; z < height; z++)
                    {
                        var building = buildings[x, z];
                        if (building && building.BuildingId == 1)
                        {
                            toUpdate.Add(new Vector3Int(x, 0, z));
                        }
                    }
                }
            }
            else
            {
                toUpdate = new List<Vector3Int>
                {
                    pos
                };
                toUpdate.AddRange(directions.Select(d => pos + d));
            }
            foreach (var position in toUpdate)
            {
                if (position.x < 0 || position.x >= width || position.z < 0 || position.z >= height)
                    continue;
                var building = buildings[position.x, position.z];
                if (!building) continue;
                if (building.BuildingId != 1) continue;
                var isRoad = new bool[4];
                for (var i = 0; i < 4; i++)
                {
                    var neighborPos = position + directions[i];
                    if (neighborPos.x < 0 || neighborPos.x >= width || neighborPos.z < 0 || neighborPos.z >= height)
                        continue;
                    var neighborBuilding = buildings[neighborPos.x, neighborPos.z];
                    if (neighborBuilding&&neighborBuilding.BuildingId == 1)
                    {
                        isRoad[i] = true;
                    }
                }
                var count = isRoad.Count(x => x);
                var prefab = roadStraight;
                var rot = Quaternion.identity;
                switch (count)
                {
                    case 0:
                        continue;
                    case 1:
                    {
                        prefab = deadEnd;
                        if (isRoad[1]) rot = Quaternion.Euler(0, 270, 0); // 上
                        else if (isRoad[0]) rot = Quaternion.Euler(0, 0, 0); // 右
                        else if (isRoad[3]) rot = Quaternion.Euler(0, 90, 0); // 下
                        else if (isRoad[2]) rot = Quaternion.Euler(0, 180, 0); // 左
                        break;
                    }
                    case 2 when isRoad[0] && isRoad[2]:
                        prefab = roadStraight;
                        rot = Quaternion.identity; // 左右
                        break;
                    case 2 when isRoad[1] && isRoad[3]:
                        prefab = roadStraight;
                        rot = Quaternion.Euler(0, 90, 0); // 上下
                        break;
                    case 2:
                    {
                        prefab = corner;
                        if (isRoad[1] && isRoad[0]) rot = Quaternion.Euler(0, 90, 0); // 上右
                        else if (isRoad[0] && isRoad[3]) rot = Quaternion.Euler(0, 180, 0); // 右下
                        else if (isRoad[3] && isRoad[2]) rot = Quaternion.Euler(0, 270, 0); // 下左
                        else if (isRoad[2] && isRoad[1]) rot = Quaternion.Euler(0, 0, 0); // 左上
                        break;
                    }
                    case 3:
                    {
                        prefab = threeWay;
                        if (!isRoad[1]) rot = Quaternion.Euler(0, 90, 0); // 缺上
                        else if (!isRoad[0]) rot = Quaternion.Euler(0, 180, 0); // 缺右
                        else if (!isRoad[3]) rot = Quaternion.Euler(0, 270, 0); // 缺下
                        else if (!isRoad[2]) rot = Quaternion.Euler(0, 0, 0); // 缺左
                        break;
                    }
                    case 4:
                        prefab = fourWay;
                        rot = Quaternion.identity;
                        break;
                }
                Destroy(building.transform.GetChild(0).gameObject);
                var newModel = Instantiate(prefab, building.transform.position, rot);
                var model = newModel.transform.GetChild(0);
                model.SetParent(building.transform);
                model.localPosition = new Vector3(0f, 0f, 0f);
                Destroy(newModel);
            }
        }
        public bool IsBuildMode()
        {
            return selectedBuildingId != -1;
        }

        private Type GetBuildingTypeByName(string typeName)
        {
            return Type.GetType(typeName);
        }

        public void Dismantle()
        {
            var position = PlaneManager.Instance.GetTilePosition();
            var currentCity = CityManager.Instance.CurrentCity;
            currentCity.Dismantle(position);
            FixRoads();
        }

        public void Upgrade()
        {
            var position = PlaneManager.Instance.GetTilePosition();
            CityManager.Instance.GetBuilding(position).Upgrade();
            UIManager.Instance.UpdateBuildingPanel();
        }

        public void RotatingBuilding()
        {
            currentRotation *= Quaternion.Euler(0, 90, 0);
            previewInstance.transform.rotation = currentRotation;
        }
        public void ChangeBuildingStyle()
        {
            styleIndex = styleIndex % buildingStyleCount + 1;
            var buildingName = buildingData[selectedBuildingId]["buildingName"].ToString();
            buildingPrefab = Resources.Load<GameObject>("Prefabs/Buildings/" + buildingName + styleIndex);
            var oldInstace = previewInstance;
            previewInstance = Instantiate(buildingPrefab, oldInstace.transform.position, oldInstace.transform.rotation);
            previewInstance.transform.rotation = currentRotation;
            DisableCollider();
            Destroy(oldInstace);
        }
        public void SetPreviewPosition(Vector3 position)
        {
            previewInstance.transform.position = position;
        }

        public void DisableCollider()
        {
            var boxCollider = previewInstance.GetComponentInChildren<BoxCollider>();
            if (boxCollider) {
                boxCollider.enabled = false;
            }
            else {
                previewInstance.GetComponentInChildren<MeshCollider>().enabled = false;
            }
        }
    }
}