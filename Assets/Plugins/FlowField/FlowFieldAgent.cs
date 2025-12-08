using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlowFieldAgent : MonoBehaviour
{
    public float moveSpeed = 8.0f;
    public float rotationSpeed = 10.0f; // 转向速度

    private FlowFieldController controller;
    private Rigidbody rb;

    void Start()
    {
        // 自动查找场控制器
        controller = FindObjectOfType<FlowFieldController>();
        rb = GetComponent<Rigidbody>();
        
        // 确保 Agent 不会因为物理而翻倒或停止
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }

    void FixedUpdate()
    {
        // 确保控制器和网格都已准备就绪
        if (controller == null || controller.grid == null || !controller.HasTarget())
        {
            rb.linearVelocity = Vector3.zero; // 如果没有目标，就停下
            return;
        }

        // --- 步骤 3: 向量插值 ---
        // 从控制器获取平滑的插值向量
        Vector2 flowDir = controller.GetFlowVector(transform.position);

        if (flowDir == Vector2.zero)
        {
            rb.linearVelocity = Vector3.zero; // 到达目标或无路径
            return;
        }

        // 将 2D 向量 (XZ) 转换为 3D 移动方向
        Vector3 moveDirection = new Vector3(flowDir.x, 0, flowDir.y);

        // 1. 移动: 使用物理引擎施加力
        // 我们使用 AddForce 来获得更自然的群体移动，而不是直接设置 position
        // rb.velocity = moveDirection * moveSpeed; 
        rb.AddForce(moveDirection * moveSpeed, ForceMode.Acceleration);

        // 限制最大速度
        if (rb.linearVelocity.sqrMagnitude > moveSpeed * moveSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        // 2. 转向: 平滑地转向移动方向
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}