using System.Collections.Generic;
using UnityEngine;

namespace Entity
{
    public class Car : MonoBehaviour
    {
        public float moveSpeed = 8f;
        public float turnSpeed = 5f;
        public float reachThreshold = 0.2f;
        private bool isWaiting = false;
        private List<Vector3> pathPoints;
        private int currentIndex;
        private bool isMoving;

        public void Start()
        {
            pathPoints = new List<Vector3>();
        }
        
        public void Init(List<Vector3> path)
        {
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("Empty path given to car.");
                return;
            }
            pathPoints = path;
            currentIndex = 0;
            transform.position = pathPoints[0];
            isMoving = true;
        }

        void Update()
        {
            if (!isMoving || pathPoints == null || currentIndex >= pathPoints.Count) return;

            var target = pathPoints[currentIndex];
            var direction = (target - transform.position).normalized;
            var targetRot = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (!(Vector3.Distance(transform.position, target) < reachThreshold)) return;
            currentIndex++;
            if (currentIndex >= pathPoints.Count)
            {
                isMoving = false;
                Destroy(gameObject); // 到终点后销毁
            }
        }
    }
}