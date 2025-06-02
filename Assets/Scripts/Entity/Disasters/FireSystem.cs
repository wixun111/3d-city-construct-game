using UnityEngine;
using System.Collections;

namespace Entity.Disasters
{
    public class FireSystem : MonoBehaviour
    {
        [SerializeField] private ParticleSystem fireEffect;
        [SerializeField] private ParticleSystem smokeEffect;
        [SerializeField] private float fireDuration = 10f; // 火灾持续时间
        [SerializeField] private float fadeOutDuration = 2f; // 淡出时间
        private bool isOnFire;
        private float fireTimer;
        private bool isFadingOut;

        [ContextMenu("触发火灾")]
        public void TriggerFire()
        {
            if (!isOnFire)
            {
                StartFire();
            }
        }

        [ContextMenu("熄灭火灾")]
        public void StopFire()
        {
            if (isOnFire)
            {
                StartFadeOut();
            }
        }

        private void Update()
        {
            if (isOnFire && !isFadingOut)
            {
                fireTimer += Time.deltaTime;
                if (fireTimer >= fireDuration)
                {
                    StartFadeOut();
                }
            }
        }

        private void StartFire()
        {
            isOnFire = true;
            isFadingOut = false;
            fireTimer = 0f;
            
            // 如果没有设置粒子效果，创建默认的
            if (fireEffect == null)
            {
                CreateDefaultFireEffect();
            }
            if (smokeEffect == null)
            {
                CreateDefaultSmokeEffect();
            }

            if (fireEffect != null)
            {
                fireEffect.Play();
                var emission = fireEffect.emission;
                emission.rateOverTime = 20;
            }
            if (smokeEffect != null)
            {
                smokeEffect.Play();
                var emission = smokeEffect.emission;
                emission.rateOverTime = 10;
            }
        }

        private void StartFadeOut()
        {
            isFadingOut = true;
            StartCoroutine(FadeOutFire());
        }

        private System.Collections.IEnumerator FadeOutFire()
        {
            float elapsedTime = 0f;
            float startRate = 20f; // 初始发射率
            float startSmokeRate = 10f;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;

                // 逐渐减少火焰和烟雾的发射率
                if (fireEffect != null)
                {
                    var emission = fireEffect.emission;
                    emission.rateOverTime = Mathf.Lerp(startRate, 0f, t);
                }
                if (smokeEffect != null)
                {
                    var emission = smokeEffect.emission;
                    emission.rateOverTime = Mathf.Lerp(startSmokeRate, 0f, t);
                }

                yield return null;
            }

            // 完全停止粒子效果
            if (fireEffect != null)
                fireEffect.Stop();
            if (smokeEffect != null)
                smokeEffect.Stop();

            isOnFire = false;
            isFadingOut = false;

            // 等待一段时间后销毁物体
            yield return new WaitForSeconds(2f);
            Destroy(gameObject);
        }

        private void CreateDefaultFireEffect()
        {
            // 创建火焰粒子系统
            GameObject fireObj = new GameObject("FireEffect");
            fireObj.transform.SetParent(transform);
            fireObj.transform.localPosition = new Vector3(0, 1, 0); // 在建筑顶部

            fireEffect = fireObj.AddComponent<ParticleSystem>();
            var main = fireEffect.main;
            main.startColor = new Color(1f, 0.5f, 0f); // 橙色火焰
            main.startSize = 0.5f;
            main.startSpeed = 2f;
            main.startLifetime = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = fireEffect.emission;
            emission.rateOverTime = 20;

            var shape = fireEffect.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            // 添加颜色渐变
            var colorOverLifetime = fireEffect.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.yellow, 0.0f),
                    new GradientColorKey(Color.red, 0.5f),
                    new GradientColorKey(Color.red, 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 0.5f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = gradient;
        }

        private void CreateDefaultSmokeEffect()
        {
            // 创建烟雾粒子系统
            GameObject smokeObj = new GameObject("SmokeEffect");
            smokeObj.transform.SetParent(transform);
            smokeObj.transform.localPosition = new Vector3(0, 2, 0); // 在火焰上方

            smokeEffect = smokeObj.AddComponent<ParticleSystem>();
            var main = smokeEffect.main;
            main.startColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 灰色烟雾
            main.startSize = 1f;
            main.startSpeed = 1f;
            main.startLifetime = 2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = smokeEffect.emission;
            emission.rateOverTime = 10;

            var shape = smokeEffect.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            // 添加颜色渐变
            var colorOverLifetime = smokeEffect.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(0.5f, 0.5f, 0.5f, 0.5f), 0.0f),
                    new GradientColorKey(new Color(0.5f, 0.5f, 0.5f, 0.0f), 1.0f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.5f, 0.0f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            colorOverLifetime.color = gradient;
        }
    }
} 