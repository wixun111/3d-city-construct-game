using UnityEngine;
using System.Collections;
using Manager;

namespace Entity.Disasters
{
    public class FireSystem : MonoBehaviour
    {
        [SerializeField] private ParticleSystem fireEffect;
        [SerializeField] private ParticleSystem smokeEffect;
        [SerializeField] private float fireDuration = 8f; // 火灾持续时间
        [SerializeField] private float fadeOutDuration = 2f; // 淡出时间
        private bool isOnFire;
        private float fireTimer;
        private bool isFadingOut;

        [ContextMenu("触发火灾")]
        public void TriggerFire()
        {
            if (!isOnFire) {
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
            if (!isOnFire || isFadingOut) return;
            fireTimer += Time.deltaTime * TimeManager.Instance.TimeScale;
            if (fireTimer >= fireDuration) {
                StartFadeOut();
            }
        }

        private void StartFire()
        {
            isOnFire = true;
            isFadingOut = false;
            fireTimer = 0f;

            if (fireEffect) {
                fireEffect.Play();
                var emission = fireEffect.emission;
                emission.rateOverTime = 100;
            }
            if (smokeEffect) {
                smokeEffect.Play();
                var emission = smokeEffect.emission;
                emission.rateOverTime = 50;
            }
        }

        private void StartFadeOut()
        {
            isFadingOut = true;
            StartCoroutine(FadeOutFire());
        }

        private IEnumerator FadeOutFire()
        {
            var elapsedTime = 0f;
            const float startRate = 100f; // 初始发射率
            const float startSmokeRate = 50f;
            while (elapsedTime < fadeOutDuration) {
                elapsedTime += Time.deltaTime * TimeManager.Instance.TimeScale;
                var t = elapsedTime / fadeOutDuration ;
                // 逐渐减少火焰和烟雾的发射率
                if (fireEffect) {
                    var emission = fireEffect.emission;
                    emission.rateOverTime = Mathf.Lerp(startRate, 0f, t);
                }
                if (smokeEffect) {
                    var emission = smokeEffect.emission;
                    emission.rateOverTime = Mathf.Lerp(startSmokeRate, 0f, t);
                }
                yield return null;
            }

            // 完全停止粒子效果
            if (fireEffect)
                fireEffect.Stop();
            if (smokeEffect)
                smokeEffect.Stop();

            isOnFire = false;
            isFadingOut = false;

            // 等待一段时间后销毁物体
            yield return new WaitForSeconds(2f/TimeManager.Instance.TimeScale);
            Destroy(gameObject);
        }
    }
} 