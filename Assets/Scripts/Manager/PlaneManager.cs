﻿using Entity.Buildings;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Manager
{
    public class PlaneManager : Singleton<PlaneManager>
    {
        // 用于表示平面
        [SerializeField] private GameObject groundPlane;

        // 选中的 Tile 信息
        [SerializeField] private Vector3Int selectedTilePosition;
        [SerializeField] private bool isTileSelected;

        // UI 面板
        [SerializeField] private GameObject buildPanel;
        [SerializeField] private Grid grid;

        // 空白 Tile 用于地面初始化
        [SerializeField] private GameObject emptyTilePrefab;
        private void Start()
        {

        }
        // 生成一个简单的平面网格
        private Mesh GeneratePlaneMesh(int cityLength,int cityWidth)
        {
            var mesh = new Mesh();
            var vertices = new Vector3[4]
            {
                new Vector3(0, 0, 0),
                new Vector3(cityLength, 0, 0),
                new Vector3(cityLength, 0, cityWidth),
                new Vector3(0, 0, cityWidth)
            };

            var triangles = new int[6]
            {
                0, 2, 1, // 第一三角形
                0, 3, 2  // 第二三角形
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            return mesh;
        }

        // 生成城市地图
        public void GenerateCity()
        {
            if (groundPlane)
            {
                Destroy(groundPlane);
            }
            var planeObject = new GameObject(name);
            groundPlane = planeObject;
            planeObject.transform.parent = grid.transform;

            // 添加一个平面渲染器，便于后续处理
            var meshFilter = planeObject.AddComponent<MeshFilter>();
            var meshRenderer = planeObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
            var cityLength = CityManager.Instance.CurrentCity.Length;
            var cityWidth = CityManager.Instance.CurrentCity.Width;
            meshFilter.mesh = GeneratePlaneMesh(cityLength, cityWidth);
            // 设置碰撞体以便于射线检测
            var boxCollider = planeObject.AddComponent<BoxCollider>();
            boxCollider.name = "PlaneBox";
            boxCollider.center = new Vector3(cityLength/2f, -0.5f, cityWidth/2f);
            boxCollider.size = new Vector3(cityLength, 1f, cityWidth); // 假设平面为较大区域
        }

        // 设置平面对象
        public void SetPlane(GameObject plane)
        {
            groundPlane = plane;
        }

        public GameObject GetPlane()
        {
            return groundPlane;
        }

        // Tile 选择和处理逻辑
        private void Update()
        {
        }

        public void HandleClick()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 生成射线
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit)) return; // 进行射线检测

            Debug.Log("点击到了物体: " + hit.collider.gameObject.name);
            var hitPosition = hit.point;
            // 计算点击位置的平面坐标（可以转换为相应的 Tile 位置）
            var tilePosition = new Vector3Int(Mathf.FloorToInt(hitPosition.x), 0, Mathf.FloorToInt(hitPosition.z));
            Debug.Log("点击 Tile 位置: " + tilePosition);

            // 判断是否点击到合适的物体
            if (BuildManager.Instance.IsBuildMode() && Input.mousePosition.y < 300) return;
            if (EventSystem.current.IsPointerOverGameObject()) return;
            // 记录选择的 Tile
            selectedTilePosition = tilePosition;
            isTileSelected = true;
            // 如果点击到建筑物，处理建筑物选择
            if (hit.collider.gameObject.name == "BuildingCollider"||hit.collider.gameObject.name.Contains("Street"))
            {
                UIManager.Instance.ShowBuildingPanel();
            }
            // 调用建筑管理器进行建造判断
            BuildManager.Instance.BuildJudge();
        }

        private void OnDrawGizmos()
        {
            if (!isTileSelected) return;

            Gizmos.color = Color.green;
            var worldPos = new Vector3(selectedTilePosition.x, 0, selectedTilePosition.z);
            Gizmos.DrawWireCube(worldPos + grid.cellSize/2, grid.cellSize); // 绘制一个简单的立方体
        }

        // 获取选中的 Tile 位置
        public Vector3Int GetTilePosition()
        {
            return selectedTilePosition;
        }

        public GameObject GroundPlane
        {
            get => groundPlane;
            set => groundPlane = value;
        }
    }
}