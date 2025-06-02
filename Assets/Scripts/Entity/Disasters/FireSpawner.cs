using UnityEngine;
using Entity.Buildings;
using UI;
using System.Collections.Generic;
using Manager;

namespace Entity.Disasters
{
    public class FireSpawner : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayer; // 地面层
        [SerializeField] private float fireRadius = 2f; // 火灾影响范围
        [SerializeField] private Color fireRangeColor = new Color(1f, 0.3f, 0.3f, 0.3f); // 火灾范围显示颜色
        [SerializeField] private float buildingDamageInterval = 1f; // 建筑物伤害间隔
        [SerializeField] private float buildingDamageAmount = 20f; // 每次伤害量
        [SerializeField] private float fireDuration = 30f; // 火灾持续时间
        private Camera mainCamera;
        private GameObject fireRangeIndicator; // 用于显示火灾范围的指示器
        private float currentFireImpact = 0f; // 当前火灾影响度
        private List<GameObject> activeFires = new List<GameObject>(); // 当前活动的火灾列表

        private void Start()
        {
            mainCamera = Camera.main;
            CreateFireRangeIndicator();
            Debug.Log("FireSpawner 初始化完成");
        }

        private void Update()
        {
            // 按G键生成火灾
            if (Input.GetKeyDown(KeyCode.G))
            {
                SpawnFire();
            }
            // 右键点击熄灭火灾
            else if (Input.GetMouseButtonDown(1))
            {
                ExtinguishFire();
            }

            // 更新火灾范围指示器位置
            UpdateFireRangeIndicator();
        }

        private void SpawnFire()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                Debug.Log($"在位置 {hit.point} 生成火灾");
                // 在点击位置创建火灾
                GameObject fireObj = new GameObject("Fire");
                fireObj.transform.position = hit.point;
                
                // 添加火灾系统组件
                var fireSystem = fireObj.AddComponent<FireSystem>();
                fireSystem.TriggerFire();

                // 添加到活动火灾列表
                activeFires.Add(fireObj);

                // 显示火灾范围
                ShowFireRange(hit.point);

                // 开始检测并伤害建筑物
                StartCoroutine(DamageBuildingsInRange(fireObj));

                // 设置火灾自动熄灭
                StartCoroutine(AutoExtinguishFire(fireObj));
            }
            else
            {
                Debug.LogWarning("未能检测到地面，无法生成火灾");
            }
        }

        private System.Collections.IEnumerator AutoExtinguishFire(GameObject fireObj)
        {
            yield return new WaitForSeconds(fireDuration);
            if (fireObj != null)
            {
                var fireSystem = fireObj.GetComponent<FireSystem>();
                if (fireSystem != null)
                {
                    fireSystem.StopFire();
                }
                activeFires.Remove(fireObj);
                Destroy(fireObj);
            }
        }

        private System.Collections.IEnumerator DamageBuildingsInRange(GameObject fireObj)
        {
            Debug.Log($"开始检测火灾 {fireObj.name} 范围内的建筑物");
            while (fireObj != null)
            {
                // 检测火灾范围内的建筑物
                Collider[] colliders = Physics.OverlapSphere(fireObj.transform.position, fireRadius);
                Debug.Log($"火灾范围内检测到 {colliders.Length} 个碰撞体");

                int affectedBuildings = 0;
                float totalDamage = 0f;

                foreach (Collider collider in colliders)
                {
                    // 检查是否是建筑物
                    var building = collider.GetComponentInParent<Entity.Buildings.Building>();
                    if (building != null)
                    {
                        affectedBuildings++;
                        // 对建筑物造成伤害
                        building.TakeDamage(buildingDamageAmount);
                        totalDamage += buildingDamageAmount;
                        
                        // 如果建筑物健康度低于阈值，销毁建筑物
                        if (building.CurrentHealth <= 0)
                        {
                            Debug.Log($"建筑物 {building.name} 被摧毁");
                            // 获取建筑物所在的位置
                            Vector3 buildingPos = building.transform.position;
                            
                            // 销毁建筑物
                            Destroy(building.gameObject);
                            
                            // 在建筑物位置生成新的火灾（但不会递归检测）
                            GameObject newFire = new GameObject("Fire");
                            newFire.transform.position = buildingPos;
                            var newFireSystem = newFire.AddComponent<FireSystem>();
                            newFireSystem.TriggerFire();
                            
                            // 添加到活动火灾列表
                            activeFires.Add(newFire);
                            
                            // 设置新火灾自动熄灭
                            StartCoroutine(AutoExtinguishFire(newFire));
                        }
                    }
                }

                // 更新火灾影响度
                UpdateFireImpact(affectedBuildings, totalDamage);

                yield return new WaitForSeconds(buildingDamageInterval);
            }
        }

        private void UpdateFireImpact(int affectedBuildings, float totalDamage)
        {
            // 直接使用伤害系数和最终伤害来更新影响度
            float damageCoefficient = affectedBuildings > 0 ? totalDamage / (affectedBuildings * buildingDamageAmount) : 0f;
            float finalDamage = totalDamage;
            
            Debug.Log($"火灾影响度 - 伤害系数: {damageCoefficient:F2}, 最终伤害: {finalDamage:F2}");
            
            // 计算综合影响度 (0-1之间)
            currentFireImpact = Mathf.Clamp01((damageCoefficient + finalDamage / 1000f) / 2f);
            
            // 通过 UIManager 更新UI
            UIManager.Instance.UpdateDisasterImpact(currentFireImpact, 0f);
        }

        private void ExtinguishFire()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                Debug.Log($"尝试熄灭位置 {hit.point} 附近的火灾");
                // 检查点击位置附近的火灾
                Collider[] colliders = Physics.OverlapSphere(hit.point, fireRadius);
                int extinguishedCount = 0;
                foreach (Collider collider in colliders)
                {
                    var fireSystem = collider.GetComponent<FireSystem>();
                    if (fireSystem != null)
                    {
                        fireSystem.StopFire();
                        activeFires.Remove(collider.gameObject);
                        Destroy(collider.gameObject);
                        extinguishedCount++;
                    }
                }
                Debug.Log($"熄灭了 {extinguishedCount} 个火灾");

                // 如果熄灭了火灾，重置影响度
                if (extinguishedCount > 0)
                {
                    currentFireImpact = 0f;
                    UIManager.Instance.UpdateDisasterImpact(currentFireImpact, 0f);
                }
            }
        }

        private void CreateFireRangeIndicator()
        {
            fireRangeIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fireRangeIndicator.transform.localScale = new Vector3(fireRadius * 2, 0.1f, fireRadius * 2);
            
            // 设置材质
            var renderer = fireRangeIndicator.GetComponent<Renderer>();
            var material = new Material(Shader.Find("Transparent/Diffuse"));
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

        private void UpdateFireRangeIndicator()
        {
            if (fireRangeIndicator.activeSelf)
            {
                // 让范围指示器始终面向摄像机
                fireRangeIndicator.transform.forward = Vector3.up;
            }
        }

        private void OnDestroy()
        {
            if (fireRangeIndicator != null)
            {
                Destroy(fireRangeIndicator);
            }
            // 清理所有活动的火灾
            foreach (var fire in activeFires)
            {
                if (fire != null)
                {
                    Destroy(fire);
                }
            }
            activeFires.Clear();
        }
    }
} 