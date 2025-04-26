using UnityEngine;

namespace Controller
{
    public class ViewController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float scrollSpeed;
        [SerializeField] private float rotationSpeed;

        private float yaw = 0f; // 水平旋转角度
        private float pitch = 0f; // 垂直旋转角度
        private bool isRotating = false;
    
        void Start()
        {
            pitch = 45f;
            yaw = 0f;
            moveSpeed = 10f;
            scrollSpeed = 100f;
            rotationSpeed = 1f;
            Cursor.visible = true;
        }

        void Update()
        {
            HandleKeyboardMovement();
            HandleMouseScroll();
            HandleMouseRotation();
        }

        private void HandleKeyboardMovement()
        {
            // 获取标准输入轴（已自动适配帧率）
            var horizontal = Input.GetAxis("Horizontal"); // A/D
            var vertical = Input.GetAxis("Vertical");     // W/S

            // 处理升降输入
            var lift = 0f;
            if (Input.GetKey(KeyCode.E)) lift = 1f;
            else if (Input.GetKey(KeyCode.Q)) lift = -1f;

            // 计算移动方向（基于摄像机坐标系）
            Vector3 move = transform.forward * vertical    // 前后：跟随摄像机面朝方向
                           + transform.right * horizontal    // 左右：跟随摄像机右侧方向
                           + transform.up * lift;            // 升降：垂直方向

            // 应用移动（统一使用世界坐标系）
            transform.position += move * moveSpeed * Time.deltaTime;
        }


        private void HandleMouseScroll()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            transform.position += transform.forward * scroll * scrollSpeed * Time.deltaTime;
        }

        private void HandleMouseRotation()
        {
            if (Input.GetMouseButtonDown(1)) // 右键按下
            {
                isRotating = true;
            }
            if (Input.GetMouseButtonUp(1)) // 右键释放
            {
                isRotating = false;
            }

            if (isRotating)
            {
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

                yaw += mouseX;
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, -80f, 80f); // 限制俯仰角范围，避免翻转

                transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            }
        }
    }
}
