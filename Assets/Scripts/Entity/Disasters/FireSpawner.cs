using UnityEngine;
using Entity.Buildings;
using UI;
using System.Collections.Generic;
using System.Linq;
using Manager;

namespace Entity.Disasters
{
    public class FireSpawner : Singleton<FireSpawner>
    {
        [SerializeField] private GameObject fireSystemPrefab; // 火灾持续时间
        [SerializeField] private LayerMask groundLayer; // 地面层
        [SerializeField] private float fireRadius = 2f; // 火灾影响范围
        [SerializeField] private Color fireRangeColor = new(1f, 0.3f, 0.3f, 0.2f); // 火灾范围显示颜色
        [SerializeField] private float buildingDamageInterval = 1f; // 建筑物伤害间隔
        [SerializeField] private float buildingDamageAmount = 10f; // 每次伤害量
        [SerializeField] private float fireDuration = 10f; // 火灾持续时间
        private Camera mainCamera;
        private GameObject fireRangeIndicator; // 用于显示火灾范围的指示器
        private float currentFireImpact; // 当前火灾影响度
        private readonly List<GameObject> activeFires = new(); // 当前活动的火灾列表

        private void Start()
        {
            mainCamera = Camera.main;
            CreateFireRangeIndicator();
            Debug.Log("FireSpawner 初始化完成");
        }

        public void SpawnFire()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer))
            {
                Debug.Log($"在位置 {hit.point} 生成火灾");
                // 在点击位置创建火灾
                
                // 添加火灾系统组件
                var fireSystem = Instantiate(fireSystemPrefab,hit.point,Quaternion.identity, transform);
                var fireSystemScript = fireSystem.GetComponent<FireSystem>();
                fireSystemScript.TriggerFire();

                // 添加到活动火灾列表
                activeFires.Add(fireSystem);

                // 显示火灾范围
                ShowFireRange(hit.point);

                // 开始检测并伤害建筑物
                StartCoroutine(DamageBuildingsInRange(fireSystem));

                // fireSystem
                StartCoroutine(AutoExtinguishFire(fireSystem));
            }
            else
            {
                Debug.LogWarning("未能检测到地面，无法生成火灾");
            }
        }

        private System.Collections.IEnumerator AutoExtinguishFire(GameObject fireObj)
        {
            yield return new WaitForSeconds(fireDuration);
            if (!fireObj) yield break;
            var fireSystem = fireObj.GetComponent<FireSystem>();
            if (fireSystem) {
                fireSystem.StopFire();
            }
            activeFires.Remove(fireObj);
            Destroy(fireObj);
        }

        private System.Collections.IEnumerator DamageBuildingsInRange(GameObject fireObj)
        {
            Debug.Log($"开始检测火灾 {fireObj.name} 范围内的建筑物");
            while (fireObj != null)
            {
                // 检测火灾范围内的建筑物
                var colliders = Physics.OverlapSphere(fireObj.transform.position, fireRadius);
                Debug.Log($"火灾范围内检测到 {colliders.Length} 个碰撞体");

                var affectedBuildings = 0;
                var totalDamage = 0f;

                foreach (var collider in colliders)
                {
                    // 检查是否是建筑物
                    var building = collider.GetComponentInParent<Building>();
                    if (!building) continue;
                    affectedBuildings++;
                    // 对建筑物造成伤害
                    building.TakeDamage(buildingDamageAmount);
                    if (!building.IsOnFire) building.TriggerFire();
                    totalDamage += buildingDamageAmount;
                        
                    // 如果建筑物健康度低于阈值，销毁建筑物
                    if (building.CurrentHealth > 0) continue;
                    
                    Debug.Log($"建筑物 {building.name} 被摧毁");
                            
                    // 销毁建筑物
                    Destroy(building.gameObject);
                }

                // 更新火灾影响度
                UpdateFireImpact(affectedBuildings, totalDamage);

                yield return new WaitForSeconds(buildingDamageInterval/TimeManager.Instance.TimeScale);
            }
        }

        private void UpdateFireImpact(int affectedBuildings, float totalDamage)
        {
            // 直接使用伤害系数和最终伤害来更新影响度
            var damageCoefficient = affectedBuildings > 0 ? totalDamage / (affectedBuildings * buildingDamageAmount) : 0f;

            Debug.Log($"火灾影响度 - 伤害系数: {damageCoefficient:F2}, 最终伤害: {totalDamage:F2}");
            
            // 计算综合影响度 (0-1之间)
            currentFireImpact = Mathf.Clamp01((damageCoefficient + totalDamage / 1000f) / 2f);
            
            // 通过 UIManager 更新UI
            UIManager.Instance.UpdateDisasterImpact(currentFireImpact, 0f);
        }

        public void ExtinguishFire()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer)) return;
            Debug.Log($"尝试熄灭位置 {hit.point} 附近的火灾");
            // 检查点击位置附近的火灾
            var colliders = Physics.OverlapSphere(hit.point, fireRadius);
            var extinguishedCount = 0;
            foreach (var collider in colliders)
            {
                var fireSystem = collider.GetComponent<FireSystem>();
                if (!fireSystem) continue;
                fireSystem.StopFire();
                activeFires.Remove(collider.gameObject);
                Destroy(collider.gameObject);
                extinguishedCount++;
            }
            Debug.Log($"熄灭了 {extinguishedCount} 个火灾");

            // 如果熄灭了火灾，重置影响度
            if (extinguishedCount > 0)
            {
                currentFireImpact = 0f;
                UIManager.Instance.UpdateDisasterImpact(currentFireImpact, 0f);
            }
        }

        private void CreateFireRangeIndicator()
        {
            fireRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fireRangeIndicator.transform.localScale = new Vector3(fireRadius * 2, fireRadius * 2, fireRadius * 2);
            
            // 设置材质
            var renderer = fireRangeIndicator.GetComponent<Renderer>();
            var material = new Material(Shader.Find($"Transparent/Diffuse"));
            material.color = fireRangeColor;
            renderer.material = material;

            // 禁用碰撞
            Destroy(fireRangeIndicator.GetComponent<Collider>());
            
            // 默认隐藏
            fireRangeIndicator.SetActive(false);
            Debug.Log("火灾范围指示器创建完成");
        }

        private void ShowFireRange(Vector3 position)
        {
            fireRangeIndicator.transform.position = position;
            fireRangeIndicator.SetActive(true);
            Debug.Log($"显示火灾范围指示器在位置 {position}");
            
            // 2秒后隐藏范围指示器
            Invoke("HideFireRange", 2f);
        }

        private void HideFireRange()
        {
            fireRangeIndicator.SetActive(false);
            Debug.Log("隐藏火灾范围指示器");
        }

        private void OnDestroy()
        {
            if (fireRangeIndicator != null)
            {
                Destroy(fireRangeIndicator);
            }
            // 清理所有活动的火灾
            foreach (var fire in activeFires.Where(fire => fire != null))
            {
                Destroy(fire);
            }
            activeFires.Clear();
        }
    }
} 