using System.Collections.Generic;
using Entity;
using UnityEngine;
using UnityEngine.UIElements;

namespace Manager
{
    public class TrafficManager : Singleton<TrafficManager>
    {
        public GameObject carsContainer;
        public GameObject[] carPrefabs;
        public int maxCarCount = 40;
        public float spawnInterval = 2f;
        private bool[,] road;
        private float timer;
        [SerializeField] private int currentCarCount;
        public void InitRoad()
        {
            var city = CityManager.Instance.CurrentCity;
            road = new bool[city.Length, city.Width];
            for (var i = 0; i < city.Length; i++)
            {
                for (var j = 0; j < city.Width; j++)
                {
                    if(!city.Buildings[i, j]) continue;
                    road[i, j] = city.Buildings[i, j].BuildingId == 1;
                }
            }
            for (var i = 0; i < 40; i++)
            {
                TrySpawnCar();
            }
        }
        void Update()
        {
            if (currentCarCount >= maxCarCount) return;
            timer += Time.deltaTime;
            if (!(timer >= spawnInterval)) return;
            TrySpawnCar();
            timer = 0f;
        }
        void TrySpawnCar()
        {
            if (road == null) return;
            var width = road.GetLength(0);
            var height = road.GetLength(1);

            var roadPoints = new List<Vector2Int>();
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (road[x, y])
                        roadPoints.Add(new Vector2Int(x, y));
                }
            }
            if (roadPoints.Count < 2) return;
            Vector2Int startIndex, endIndex;
            do
            {
                startIndex = roadPoints[Random.Range(0, roadPoints.Count)];
                endIndex = roadPoints[Random.Range(0, roadPoints.Count)];
            } while (startIndex == endIndex);

            var path = FindPath(startIndex, endIndex);
            if (path == null || path.Count < 2) return;

            // 随机选一个车辆预制体生成
            var prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
            var car = Instantiate(prefab, carsContainer.transform, true);
            car.GetComponent<Car>().Init(path);
            currentCarCount++;
        }
        List<Vector3> FindPath(Vector2Int start, Vector2Int end)
        {
            var width = road.GetLength(0);
            var height = road.GetLength(1);

            var visited = new bool[width][];
            for (int index = 0; index < width; index++)
            {
                visited[index] = new bool[height];
            }

            Dictionary<Vector2Int, Vector2Int> cameFrom = new();
            Queue<Vector2Int> queue = new();

            queue.Enqueue(start);
            visited[start.x][start.y] = true;

            var directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == end) break;

                foreach (var dir in directions)
                {
                    var next = current + dir;

                    // 越界检查
                    if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                        continue;

                    // 不可通行或访问过
                    if (!road[next.x, next.y] || visited[next.x][next.y])
                        continue;

                    visited[next.x][next.y] = true;
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }

            // 若未到达终点
            if (!cameFrom.ContainsKey(end)) return null;

            // 回溯路径
            List<Vector3> path = new();
            var node = end;

            while (node != start)
            {
                path.Add(GridToWorld(node));
                node = cameFrom[node];
            }
            path.Add(GridToWorld(start));
            path.Reverse();
            return path;
        }
        Vector3 GridToWorld(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x + 0.5f, 0.06f, gridPos.y + 0.5f);
        }

        public int CurrentCarCount
        {
            get => currentCarCount;
            set => currentCarCount = value;
        }
    }
}