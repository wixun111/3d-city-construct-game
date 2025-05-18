using System.Collections.Generic;
using Manager;
using UnityEngine;

namespace Entity
{
    public class Car : MonoBehaviour
    {
        public float moveSpeed;
        public float turnSpeed;
        public float reachThreshold;
        private bool isWaiting = false;
        [SerializeField] private List<Vector3> pathPoints;
        [SerializeField] private int currentIndex;
        [SerializeField] private bool isMoving;

        public void Awake()
        {
            moveSpeed = 1f;
            turnSpeed = 6f;
            reachThreshold = 0.13f;
            pathPoints = new List<Vector3>();
        }
        
        public void Init(List<Vector3> path)
        {
            pathPoints = path;
            currentIndex = 0;
            transform.position = pathPoints[0];
            isMoving = true;
        }

        void Update()
        {
            if (!isMoving || pathPoints == null || currentIndex >= pathPoints.Count) return;
            var angle = Mathf.Round(transform.eulerAngles.y / 90f) * 90f;
            var forward = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, -Mathf.Sin(angle * Mathf.Deg2Rad));
            var offset = forward * 0.1f;
            if (Mathf.Approximately(angle % 180f, 0))
            {
                offset.z = 0;
            }
            else
            {
                offset.x = 0;
            }
            var target = pathPoints[currentIndex];
            var direction = (target  + offset - transform.position).normalized;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                var targetYAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                var currentYAngle = transform.eulerAngles.y;
                var yAngle = Mathf.LerpAngle(currentYAngle, targetYAngle, turnSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(-90f, yAngle, 0f);
            }
            transform.position = Vector3.MoveTowards(transform.position, target + offset, moveSpeed * Time.deltaTime);

            if (!(Vector3.Distance(transform.position, target) < reachThreshold)) return;
            currentIndex++;
            if (currentIndex < pathPoints.Count) return;
            isMoving = false;
            TrafficManager.Instance.CurrentCarCount -= 1;
            Destroy(gameObject); // 到终点后销毁 
        }
    }
}