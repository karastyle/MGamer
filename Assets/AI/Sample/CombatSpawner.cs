using System.Threading;
using System.Collections.Generic;
using CarlosLab.UtilityIntelligence;
using CarlosLab.UtilityIntelligence.Examples;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EnemyType
{
    Near,
    Far
}

public class CombatSpawner : MonoBehaviour
{
    [Header("Prefab 设置")]
    public GameObject nearEnemyPrefab;
    public GameObject farEnemyPrefab;
    public GameObject playerPrefab;

    [Header("数量设置")]
    [Range(0, 50)] public int nearEnemyCount = 4;
    [Range(0, 50)] public int farEnemyCount = 4;

    [Header("生成参数")]
    [Range(1f, 30f)] public float spawnRadius = 8f;
    public bool autoSpawnOnStart = true;

    public UtilityEntityRegister entityRegister;

    private readonly List<GameObject> enemies = new();

    // ✅ 静态ID计数器
    private static int globalIdCounter = 0;

    void Start()
    {
        if (autoSpawnOnStart)
        {
            SpawnEnemies();
        }
    }

    /// <summary>
    /// ✅ 生成全局唯一ID
    /// </summary>
    public static int GenId()
    {
        return Interlocked.Increment(ref globalIdCounter);
    }

    /// <summary>
    /// ✅ 生成敌人（支持近战/远程两种类型）
    /// </summary>
    public void SpawnEnemies()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ 必须在运行时点击按钮生成敌人！");
            return;
        }

        ClearEnemies();

        Vector3 center = playerPrefab != null ? playerPrefab.transform.position : Vector3.zero;

        // 生成近战敌人
        SpawnEnemyGroup(EnemyType.Near, nearEnemyPrefab, nearEnemyCount, center);
        // 生成远程敌人
        SpawnEnemyGroup(EnemyType.Far, farEnemyPrefab, farEnemyCount, center);

        Debug.Log($"✅ 已生成 {nearEnemyCount} 个近战敌人 + {farEnemyCount} 个远程敌人。");
    }

    /// <summary>
    /// ✅ 按类型生成敌人组
    /// </summary>
    private void SpawnEnemyGroup(EnemyType type, GameObject prefab, int count, Vector3 center)
    {
        if (prefab == null || count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(spawnRadius * 0.7f, spawnRadius);

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 spawnPos = center + offset;
            spawnPos.y = center.y;

            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.LookRotation(center - spawnPos));
            enemy.name = $"{type}_Enemy_{i + 1}";
            enemies.Add(enemy);

            // ✅ 分配唯一ID + 类型
            var enemyComp = enemy.GetComponent<Enemy>();
            if (enemyComp != null)
            {
                enemyComp.enemyId = GenId();
                enemyComp.enemyType = type;
            }

            // 注册到 Utility 系统
            var agentController = enemy.GetComponent<UtilityAgentController>();
            if (agentController != null && entityRegister != null)
            {
                entityRegister.RegisterAgent(agentController);
            }
        }
    }

    /// <summary>
    /// 清空敌人
    /// </summary>
    public void ClearEnemies()
    {
        foreach (var e in enemies)
        {
            if (e == null) continue;

            var agentController = e.GetComponent<UtilityAgentController>();
            if (agentController != null && entityRegister != null)
                entityRegister.UnregisterAgent(agentController);

            Destroy(e);
        }

        enemies.Clear();
    }
    
    public List<GameObject> GetEnemies()
    {
        return enemies;
    }
    
    public GameObject GetEnemyById(int id)
    {
        foreach (var e in enemies)
        {
            if (e == null) continue;

            var enemyComp = e.GetComponent<Enemy>();
            if (enemyComp != null && enemyComp.enemyId == id)
                return e;
        }

        return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CombatSpawner))]
public class CombatSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CombatSpawner spawner = (CombatSpawner)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🧩 控制按钮", EditorStyles.boldLabel);

        if (GUILayout.Button("生成 Enemies"))
        {
            spawner.SpawnEnemies();
        }

        if (GUILayout.Button("清空 Enemies"))
        {
            if (Application.isPlaying)
                spawner.ClearEnemies();
            else
                Debug.LogWarning("⚠️ 请在运行时清空！");
        }
    }
}
#endif
