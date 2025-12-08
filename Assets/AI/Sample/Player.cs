using CarlosLab.UtilityIntelligence;
using UnityEngine;
using System.Collections.Generic;
using CarlosLab.UtilityIntelligence.Examples;

public enum RoleType
{
    Player,
    Enemy
}

public enum TargetSlotType
{
    Attack, // 内圈
    Keep,   // 中圈
    Shoot,   // 外圈
    None
}

[ExecuteAlways]
public class Player : UtilityAgentFacade
{
    public RoleType roleType = RoleType.Player;

    [Header("圈半径设置")]
    [Range(0.5f, 10f)] public float InnerRadius = 2f;
    [Range(0.5f, 20f)] public float MiddleRadius = 4f;
    [Range(0.5f, 30f)] public float OuterRadius = 8f;
    [Range(0.5f, 40f)] public float RangedRadius = 12f;

    [Header("点位数量")]
    [Range(1, 32)] public int InnerBandSlots = 6;
    [Range(1, 32)] public int OuterBandSlots = 10;
    [Range(1, 32)] public int RangedBandSlots = 8;

    [Header("视野设置")]
    [Range(1f, 180f)] public float ViewAngle = 90f;
    [Range(1f, 30f)] public float ViewDistance = 10f;

    [Header("颜色设置")]
    public Color InnerColor = Color.red;
    public Color MiddleColor = Color.yellow;
    public Color OuterColor = Color.green;
    public Color RangedColor = new Color(0.3f, 1f, 0.3f);
    public Color SlotColor = Color.cyan;
    public Color ViewColor = new Color(0f, 0.5f, 1f, 0.25f);

    [Header("Slot 实例化设置")]
    public GameObject slotPrefab;

    public UtilityEntityRegister entityRegister;

    // ✅ 用字典按类型管理 Slot 实例
    private readonly Dictionary<TargetSlotType, List<GameObject>> slotGroups = new();
    

    // 🕊️ 新增一个参数，控制过渡速度
    [Range(0.1f, 5f)] public float offsetLerpSpeed = 0.5f;
    
    
    
    [Header("随机偏移控制")]
    public float radiusOffsetRange = 2f;     // 最大半径漂移范围
    public float angleOffsetRange = 10f;     // 最大角度漂移范围（度）
    public Vector2 updateIntervalRange = new Vector2(1.5f, 3f); // 每次变更方向的间隔

    [Header("漂移速度控制")]
    [Range(0.01f, 1f)] public float radiusDriftSpeed = 0.3f;  // 半径变化速度
    [Range(0.1f, 5f)] public float angleDriftSpeed = 1.5f;    // 角度变化速度（度/秒）
    
    // 当前变化方向（+1 或 -1）
    private int radiusDirection = 1;
    private int angleDirection = 1;

    private float timeSinceLastPick = 0f;
    private float currentUpdateDuration = 2f;
    
    private float randomRadiusOffset = 0f;
    private float randomAngleOffset = 0f;
    
    private void Start()
    {
        PickNewDirections();
    }
    
    /// <summary>
    /// ✅ 随机选择新方向与持续时间
    /// </summary>
    private void PickNewDirections()
    {
        radiusDirection = Random.value > 0.5f ? 1 : -1;
        angleDirection = Random.value > 0.5f ? 1 : -1;

        currentUpdateDuration = Random.Range(updateIntervalRange.x, updateIntervalRange.y);
    }
    
    /// <summary>
    /// ✅ 连续平滑漂移逻辑
    /// </summary>
    private void UpdateRandomDrift()
    {
        timeSinceLastPick += Time.deltaTime;

        // 每帧轻微偏移（帧率无关）
        randomRadiusOffset += radiusDirection * radiusDriftSpeed * Time.deltaTime;
        randomAngleOffset += angleDirection * angleDriftSpeed * Time.deltaTime;

        // 限制在范围内来回摆动
        randomRadiusOffset = Mathf.Clamp(randomRadiusOffset, -radiusOffsetRange, radiusOffsetRange);
        randomAngleOffset = Mathf.Clamp(randomAngleOffset, -angleOffsetRange, angleOffsetRange);

        // 如果到达边界就反转方向
        if (Mathf.Abs(randomRadiusOffset) >= radiusOffsetRange * 0.95f)
            radiusDirection *= -1;

        if (Mathf.Abs(randomAngleOffset) >= angleOffsetRange * 0.95f)
            angleDirection *= -1;

        // 定时随机调整漂移方向
        if (timeSinceLastPick >= currentUpdateDuration)
        {
            PickNewDirections();
            timeSinceLastPick = 0f;
        }
    }


    private void Update()
    {
        if (!Application.isPlaying)
            return;

        UpdateRandomDrift();

        Vector3 pos = transform.position;

        float innerBandRadius = (InnerRadius + MiddleRadius) * 0.5f;
        float outerBandRadius = (MiddleRadius + OuterRadius) * 0.5f;
        float rangedBandRadius = (OuterRadius + RangedRadius) * 0.5f;

        UpdateSlotsRuntime(pos, innerBandRadius, InnerBandSlots, TargetSlotType.Attack);
        UpdateSlotsRuntime(pos, outerBandRadius, OuterBandSlots, TargetSlotType.Keep);
        UpdateSlotsRuntime(pos, rangedBandRadius, RangedBandSlots, TargetSlotType.Shoot);
    }
    
    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        pos.y += 0.05f;

        DrawCircle(pos, InnerRadius, InnerColor);
        DrawCircle(pos, MiddleRadius, MiddleColor);
        DrawCircle(pos, OuterRadius, OuterColor);
        DrawCircle(pos, RangedRadius, RangedColor);

        DrawViewCone(pos, transform.forward, ViewAngle, ViewDistance, ViewColor);

        float innerBandRadius = (InnerRadius + MiddleRadius) * 0.5f;
        float outerBandRadius = (MiddleRadius + OuterRadius) * 0.5f;
        float rangedBandRadius = (OuterRadius + RangedRadius) * 0.5f;

        DrawSlotsGizmo(pos, innerBandRadius, InnerBandSlots, Color.Lerp(InnerColor, MiddleColor, 0.5f));
        DrawSlotsGizmo(pos, outerBandRadius, OuterBandSlots, Color.Lerp(MiddleColor, OuterColor, 0.5f));
        DrawSlotsGizmo(pos, rangedBandRadius, RangedBandSlots, Color.Lerp(OuterColor, RangedColor, 0.5f));

        // ✅ 请求Scene视图持续刷新
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.SceneView.RepaintAll();
#endif
    }
    
    // ✅ Gizmo绘制用（也使用偏移）
    void DrawSlotsGizmo(Vector3 center, float radius, int count, Color color)
    {
        if (count <= 0) return;
        Gizmos.color = color;

        for (int i = 0; i < count; i++)
        {
            float baseAngle = (i / (float)count) * Mathf.PI * 2f;
            float angle = baseAngle + randomAngleOffset * Mathf.Deg2Rad;
            float finalRadius = radius + randomRadiusOffset;

            Vector3 slotPos = center + new Vector3(Mathf.Cos(angle) * finalRadius, 0, Mathf.Sin(angle) * finalRadius);
            Gizmos.DrawSphere(slotPos, 0.12f);
        }
    }

    // ✅ 运行时更新Slot Transform
    void UpdateSlotsRuntime(Vector3 center, float radius, int count, TargetSlotType slotType)
    {
        if (!slotGroups.ContainsKey(slotType))
            slotGroups[slotType] = new List<GameObject>();

        List<GameObject> slots = slotGroups[slotType];
        SyncSlotInstances(slots, count, slotType);

        for (int i = 0; i < count; i++)
        {
            float baseAngle = (i / (float)count) * Mathf.PI * 2f;
            float angle = baseAngle + randomAngleOffset * Mathf.Deg2Rad;
            float finalRadius = radius + randomRadiusOffset;

            Vector3 slotPos = center + new Vector3(Mathf.Cos(angle) * finalRadius, 0, Mathf.Sin(angle) * finalRadius);

            GameObject slot = slots[i];
            if (slot == null) continue;

            slot.transform.position = slotPos;
            slot.transform.rotation = Quaternion.LookRotation((slotPos - center).normalized, Vector3.up);
        }
    }

    void DrawCircle(Vector3 center, float radius, Color color, int segments = 64)
    {
        Gizmos.color = color;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    void DrawViewCone(Vector3 origin, Vector3 forward, float angle, float distance, Color color, int segments = 32)
    {
        Gizmos.color = color;
        Quaternion leftRot = Quaternion.Euler(0, -angle * 0.5f, 0);
        Quaternion rightRot = Quaternion.Euler(0, angle * 0.5f, 0);
        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.DrawLine(origin, origin + leftDir * distance);
        Gizmos.DrawLine(origin, origin + rightDir * distance);

        Vector3 prevPoint = origin + leftDir * distance;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float currAngle = -angle * 0.5f + t * angle;
            Vector3 dir = Quaternion.Euler(0, currAngle, 0) * forward;
            Vector3 nextPoint = origin + dir * distance;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

    void SyncSlotInstances(List<GameObject> slotList, int neededCount, TargetSlotType type)
    {
        if (!Application.isPlaying || slotPrefab == null)
            return;

        while (slotList.Count < neededCount)
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            slot.name = $"{type}_Slot_{slotList.Count}";

            var comp = slot.GetComponent<TargetSlot>();
            if (comp == null)
                comp = slot.AddComponent<TargetSlot>();

            comp.slotType = type;
            comp.slotIndex = slotList.Count;
            comp.usedEnemeyId = 0;
            comp.slotKey = $"{type}_{comp.slotIndex}";

            var entityController = slot.GetComponent<UtilityEntityController>();
            if (entityController != null && entityRegister != null)
                entityRegister.RegisterEntity(entityController);

            slotList.Add(slot);
        }

        while (slotList.Count > neededCount)
        {
            GameObject toRemove = slotList[^1];
            slotList.RemoveAt(slotList.Count - 1);

            var entityController = toRemove.GetComponent<UtilityEntityController>();
            if (entityController != null && entityRegister != null)
                entityRegister.UnregisterEntity(entityController);

            DestroyImmediate(toRemove);
        }
    }

    public List<GameObject> GetSlots(TargetSlotType type)
    {
        return slotGroups.ContainsKey(type) ? slotGroups[type] : null;
    }

    public bool HasSlotNotUsed()
    {
        if (slotGroups.Count == 0)
            return false;

        foreach (var group in slotGroups.Values)
        {
            foreach (var slotObj in group)
            {
                var slotComp = slotObj.GetComponent<TargetSlot>();
                if (slotComp != null && slotComp.usedEnemeyId == 0)
                    return true;
            }
        }

        return false;
    }
}
