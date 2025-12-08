using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using CarlosLab.UtilityIntelligence.Examples;

public class GradientDescentManager : MonoBehaviour
{
    [Header("梯度下降参数")]
    public int gridResolution = 20;
    public float gridSpacing = 1.0f;
    public float repulsionRadius = 3.0f;
    public float playerRepulsionWeight = 2.0f;
    public int iterations = 10;
    public float stepSize = 1f;
    public float distanceWeight = 0.1f;
    public float recalcInterval = 0.5f;

    [Header("调试")]
    public bool drawGizmos = true;

    private Transform player;

    private float gradientTimer = 0f;

    // 🔹 每个敌人独立的 SDF 网格
    private Dictionary<int, float[,]> sdfPerEnemy = new Dictionary<int, float[,]>();
    private Dictionary<int, Vector3[,]> gridPosPerEnemy = new Dictionary<int, Vector3[,]>();

    // 每个敌人的数据
    private class EnemyData
    {
        public int enemyId;
        public Func<List<Vector3>> candidatePointFunc; // 动态生成候选点
        public Action<int, Vector3> callback;
        public Vector3 lastBestPoint;
    }
    private List<EnemyData> enemiesRunning = new List<EnemyData>();

    private void Start()
    {
        player = AIGameManager.Player.transform;
    }

    private void Update()
    {
        if (player == null || enemiesRunning.Count == 0) return;

        gradientTimer += Time.deltaTime;
        if (gradientTimer >= recalcInterval)
        {
            gradientTimer = 0f;

            foreach (var eData in enemiesRunning)
            {
                GenerateLocalSDF(eData);
                Vector3 bestPoint = ComputeBestGradient(eData);
                eData.lastBestPoint = bestPoint;
                eData.callback?.Invoke(eData.enemyId, bestPoint);
            }
        }
    }

    #region === 公共接口 ===

    public void StartGradientDescent(int enemyId, Func<List<Vector3>> candidatePointFunc, Action<int, Vector3> callback)
    {
        var existing = enemiesRunning.Find(e => e.enemyId == enemyId);
        if (existing != null)
        {
            existing.candidatePointFunc = candidatePointFunc;
            existing.callback = callback;
            return;
        }

        EnemyData data = new EnemyData
        {
            enemyId = enemyId,
            candidatePointFunc = candidatePointFunc,
            callback = callback,
            lastBestPoint = Vector3.zero
        };
        enemiesRunning.Add(data);

        GenerateLocalSDF(data);
        Vector3 best = ComputeBestGradient(data);
        data.lastBestPoint = best;
        data.callback?.Invoke(enemyId, best);
    }

    public void StopGradientDescent(int enemyId)
    {
        enemiesRunning.RemoveAll(e => e.enemyId == enemyId);
        sdfPerEnemy.Remove(enemyId);
        gridPosPerEnemy.Remove(enemyId);
    }

    #endregion

    #region === 核心梯度下降逻辑 ===

    private void GenerateLocalSDF(EnemyData data)
    {
        if (player == null) return;

        int id = data.enemyId;

        // 分配独立的 SDF 缓冲区
        if (!sdfPerEnemy.ContainsKey(id))
            sdfPerEnemy[id] = new float[gridResolution, gridResolution];
        if (!gridPosPerEnemy.ContainsKey(id))
            gridPosPerEnemy[id] = new Vector3[gridResolution, gridResolution];

        float[,] sdfGrid = sdfPerEnemy[id];
        Vector3[,] gridWorldPositions = gridPosPerEnemy[id];

        var allEnemies = AIGameManager.Spawner.GetEnemies();
        Vector3 center = player.position;
        float halfSize = gridResolution * gridSpacing * 0.5f;

        for (int x = 0; x < gridResolution; x++)
        {
            for (int z = 0; z < gridResolution; z++)
            {
                Vector3 worldPos = new Vector3(center.x - halfSize + x * gridSpacing,
                                               center.y,
                                               center.z - halfSize + z * gridSpacing);
                gridWorldPositions[x, z] = worldPos;

                float totalPotential = 0f;

                // 敌人排斥力（排除自己）
                foreach (var e in allEnemies)
                {
                    if (e == null) continue;
                    var enemyComp = e.GetComponent<Enemy>();
                    if (enemyComp.enemyId == data.enemyId) continue;

                    float dist = Vector3.Distance(worldPos, e.transform.position);
                    if (dist < repulsionRadius)
                        totalPotential += Mathf.Exp(-dist / repulsionRadius);
                }

                // 玩家排斥力
                float playerDist = Vector3.Distance(worldPos, player.position);
                if (playerDist < repulsionRadius)
                    totalPotential += Mathf.Exp(-playerDist / repulsionRadius) * playerRepulsionWeight;

                // 已运行敌人 bestPoint 排斥
                foreach (var eData in enemiesRunning)
                {
                    if (eData.enemyId == data.enemyId) continue;
                    if (eData.lastBestPoint == Vector3.zero) continue;

                    float dist = Vector3.Distance(worldPos, eData.lastBestPoint);
                    if (dist < repulsionRadius)
                        totalPotential += Mathf.Exp(-dist / repulsionRadius);
                }

                sdfGrid[x, z] = totalPotential;
            }
        }
    }

    private Vector3 ComputeBestGradient(EnemyData eData)
    {
        var sdfGrid = sdfPerEnemy[eData.enemyId];
        var gridWorldPositions = gridPosPerEnemy[eData.enemyId];

        List<Vector3> points = eData.candidatePointFunc.Invoke();
        List<Vector3> newPoints = new List<Vector3>(points.Count);

        foreach (var p0 in points)
        {
            Vector3 p = p0;
            for (int i = 0; i < iterations; i++)
                p = ComputeGradientDescentPoint(p, sdfGrid, gridWorldPositions);
            newPoints.Add(p);
        }

        Vector3 bestPoint = newPoints[0];
        float bestScore = float.MaxValue;

        foreach (var p in newPoints)
        {
            float sdfVal = GetSDFValueAtWorldPosition(p, sdfGrid, gridWorldPositions);
            var enemyObj = AIGameManager.Spawner.GetEnemyById(eData.enemyId);
            float dist = Vector3.Distance(enemyObj.transform.position, p);
            float score = sdfVal + distanceWeight * dist;
            if (score < bestScore)
            {
                bestScore = score;
                bestPoint = p;
            }
        }

        return bestPoint;
    }

    private Vector3 ComputeGradientDescentPoint(Vector3 point, float[,] sdfGrid, Vector3[,] gridWorldPositions)
    {
        int res = gridResolution;
        float spacing = gridSpacing;

        int closestX = 0, closestZ = 0;
        float minDist = float.MaxValue;

        for (int x = 0; x < res; x++)
        {
            for (int z = 0; z < res; z++)
            {
                float dist = Vector3.SqrMagnitude(point - gridWorldPositions[x, z]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestX = x;
                    closestZ = z;
                }
            }
        }

        int xL = Mathf.Clamp(closestX - 1, 0, res - 1);
        int xR = Mathf.Clamp(closestX + 1, 0, res - 1);
        int zD = Mathf.Clamp(closestZ - 1, 0, res - 1);
        int zU = Mathf.Clamp(closestZ + 1, 0, res - 1);

        float dUx = (sdfGrid[xR, closestZ] - sdfGrid[xL, closestZ]) / (2f * spacing);
        float dUz = (sdfGrid[closestX, zU] - sdfGrid[closestX, zD]) / (2f * spacing);

        Vector3 gradDir = new Vector3(-dUx, 0, -dUz).normalized;
        Vector3 targetPos = point + gradDir * stepSize;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, stepSize * 2, NavMesh.AllAreas))
            return hit.position;

        return point;
    }

    private float GetSDFValueAtWorldPosition(Vector3 pos, float[,] sdfGrid, Vector3[,] gridWorldPositions)
    {
        int res = gridResolution;
        int closestX = 0, closestZ = 0;
        float minDist = float.MaxValue;

        for (int x = 0; x < res; x++)
        {
            for (int z = 0; z < res; z++)
            {
                float dist = Vector3.SqrMagnitude(pos - gridWorldPositions[x, z]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestX = x;
                    closestZ = z;
                }
            }
        }

        return sdfGrid[closestX, closestZ];
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos || sdfPerEnemy.Count == 0) return;

        var selected = UnityEditor.Selection.activeGameObject;
        if (selected == null) return;

        var enemy = selected.GetComponent<Enemy>();
        if (enemy == null) return;

        int id = enemy.enemyId;
        if (!sdfPerEnemy.TryGetValue(id, out var sdfGrid) ||
            !gridPosPerEnemy.TryGetValue(id, out var gridWorldPositions)) return;

        for (int x = 0; x < gridResolution; x++)
        {
            for (int z = 0; z < gridResolution; z++)
            {
                float val = sdfGrid[x, z];
                if (val <= 0.01f) continue;
                Gizmos.color = Color.Lerp(Color.green, Color.red, Mathf.Clamp01(val));
                Gizmos.DrawSphere(gridWorldPositions[x, z], 0.1f);
            }
        }
    }
#endif
}
