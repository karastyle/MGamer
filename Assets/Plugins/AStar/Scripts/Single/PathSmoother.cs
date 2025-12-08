using UnityEngine;
using System.Collections.Generic;

public static class PathSmoother
{
    /// <summary>
    /// 拐点优化（远离墙壁，避免卡角）
    /// </summary>
    public static List<Vector3> OptimizeCorners(List<Vector3> path, AStarGrid grid, float cornerOffset = 0.5f, float minAngle = 30f)
    {
        if (path == null || path.Count <= 2)
        {
            return path;
        }

        List<Vector3> optimizedPath = new List<Vector3>();
        optimizedPath.Add(path[0]);

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 prev = path[i - 1];
            Vector3 current = path[i];
            Vector3 next = path[i + 1];

            Vector3 dirFromPrev = (current - prev);
            dirFromPrev.y = 0;
            dirFromPrev.Normalize();

            Vector3 dirToNext = (next - current);
            dirToNext.y = 0;
            dirToNext.Normalize();

            float angle = Vector3.Angle(dirFromPrev, dirToNext);

            // 如果是拐点
            if (angle > minAngle && angle < 180f - minAngle)
            {
                // 🔑 关键：检测周围障碍物，计算"远离障碍"的方向
                Vector3 awayFromObstacles = CalculateAwayFromObstaclesDirection(current, grid);

                if (awayFromObstacles != Vector3.zero)
                {
                    // 向远离障碍的方向偏移
                    Vector3 offsetPoint = current + awayFromObstacles * cornerOffset;

                    // 检查偏移点是否可行走
                    AStarNode offsetNode = grid.NodeFromWorldPoint(offsetPoint);
                    if (offsetNode != null && offsetNode.walkable)
                    {
                        optimizedPath.Add(offsetPoint);
                    }
                    else
                    {
                        // 尝试减小偏移距离
                        float reducedOffset = cornerOffset * 0.5f;
                        offsetPoint = current + awayFromObstacles * reducedOffset;
                        offsetNode = grid.NodeFromWorldPoint(offsetPoint);

                        if (offsetNode != null && offsetNode.walkable)
                        {
                            optimizedPath.Add(offsetPoint);
                        }
                        else
                        {
                            optimizedPath.Add(current);
                        }
                    }
                }
                else
                {
                    optimizedPath.Add(current);
                }
            }
            else
            {
                optimizedPath.Add(current);
            }
        }

        optimizedPath.Add(path[path.Count - 1]);

        return optimizedPath;
    }

    /// <summary>
    /// 计算"远离障碍物"的方向
    /// </summary>
    private static Vector3 CalculateAwayFromObstaclesDirection(Vector3 position, AStarGrid grid)
    {
        AStarNode centerNode = grid.NodeFromWorldPoint(position);
        if (centerNode == null) return Vector3.zero;

        // 8 个方向的向量
        Vector3[] directions = new Vector3[]
        {
            new Vector3(1, 0, 0), // 右
            new Vector3(-1, 0, 0), // 左
            new Vector3(0, 0, 1), // 前
            new Vector3(0, 0, -1), // 后
            new Vector3(1, 0, 1).normalized, // 右前
            new Vector3(-1, 0, 1).normalized, // 左前
            new Vector3(1, 0, -1).normalized, // 右后
            new Vector3(-1, 0, -1).normalized // 左后
        };

        Vector3 totalAwayDirection = Vector3.zero;
        int obstacleCount = 0;

        // 检测周围的障碍物
        float checkRadius = grid.nodeRadius * 3; // 检测半径（3格）

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 checkPos = position + directions[i] * checkRadius;
            AStarNode checkNode = grid.NodeFromWorldPoint(checkPos);

            // 如果这个方向有障碍物
            if (checkNode == null || !checkNode.walkable)
            {
                // 累加"远离障碍"的反方向
                totalAwayDirection -= directions[i];
                obstacleCount++;
            }
        }

        // 如果周围有障碍物，返回远离障碍的平均方向
        if (obstacleCount > 0)
        {
            totalAwayDirection.Normalize();
            return totalAwayDirection;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 射线检测平滑（支持手动障碍物检测）
    /// </summary>
    public static List<Vector3> SmoothPathRaycast(List<AStarNode> path, AStarGrid grid)
    {
        if (path == null || path.Count <= 2)
        {
            return NodeListToVector3List(path);
        }

        List<Vector3> smoothPath = new List<Vector3>();
        smoothPath.Add(path[0].worldPosition);

        int currentIndex = 0;

        while (currentIndex < path.Count - 1)
        {
            int farthestIndex = currentIndex + 1;

            for (int i = currentIndex + 2; i < path.Count; i++)
            {
                if (IsPathClear(path[currentIndex], path[i], grid))
                {
                    farthestIndex = i;
                }
                else
                {
                    break;
                }
            }

            currentIndex = farthestIndex;
            smoothPath.Add(path[currentIndex].worldPosition);
        }

        return smoothPath;
    }

    private static bool IsPathClear(AStarNode from, AStarNode to, AStarGrid grid)
    {
        Vector3 start = from.worldPosition;
        Vector3 end = to.worldPosition;

        float distance = Vector3.Distance(start, end);
        float nodeSize = grid.nodeRadius * 2;
        int sampleCount = Mathf.CeilToInt(distance / (nodeSize * 0.5f));

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            Vector3 samplePoint = Vector3.Lerp(start, end, t);

            AStarNode node = grid.NodeFromWorldPoint(samplePoint);
            if (node == null || !node.walkable)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Catmull-Rom 样条平滑（更光滑）
    /// </summary>
    public static List<Vector3> SmoothPathCatmullRom(List<AStarNode> path, AStarGrid grid, int pointsPerSegment = 5)
    {
        if (path == null || path.Count <= 2)
        {
            return NodeListToVector3List(path);
        }

        List<Vector3> keyPoints = SmoothPathRaycast(path, grid);

        if (keyPoints.Count <= 2)
        {
            return keyPoints;
        }

        List<Vector3> smoothPath = new List<Vector3>();

        for (int i = 0; i < keyPoints.Count - 1; i++)
        {
            Vector3 p0 = (i == 0) ? keyPoints[i] : keyPoints[i - 1];
            Vector3 p1 = keyPoints[i];
            Vector3 p2 = keyPoints[i + 1];
            Vector3 p3 = (i + 2 < keyPoints.Count) ? keyPoints[i + 2] : keyPoints[i + 1];

            for (int j = 0; j < pointsPerSegment; j++)
            {
                float t = j / (float)pointsPerSegment;
                Vector3 point = CalculateCatmullRomPoint(t, p0, p1, p2, p3);

                AStarNode node = grid.NodeFromWorldPoint(point);
                if (node != null && node.walkable)
                {
                    smoothPath.Add(point);
                }
                else
                {
                    smoothPath.Add(p1);
                    break;
                }
            }
        }

        smoothPath.Add(keyPoints[keyPoints.Count - 1]);

        return smoothPath;
    }

    /// <summary>
    /// 简化路径（移除共线点）
    /// </summary>
    public static List<Vector3> SimplifyPath(List<AStarNode> path, float angleThreshold = 5f)
    {
        if (path == null || path.Count <= 2)
        {
            return NodeListToVector3List(path);
        }

        List<Vector3> simplified = new List<Vector3>();
        simplified.Add(path[0].worldPosition);

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector3 dirToPrev = (path[i].worldPosition - path[i - 1].worldPosition).normalized;
            Vector3 dirToNext = (path[i + 1].worldPosition - path[i].worldPosition).normalized;

            float angle = Vector3.Angle(dirToPrev, dirToNext);

            if (angle > angleThreshold)
            {
                simplified.Add(path[i].worldPosition);
            }
        }

        simplified.Add(path[path.Count - 1].worldPosition);

        return simplified;
    }

    private static Vector3 CalculateCatmullRomPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 result = 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );

        return result;
    }

    private static List<Vector3> NodeListToVector3List(List<AStarNode> nodes)
    {
        if (nodes == null) return new List<Vector3>();

        List<Vector3> result = new List<Vector3>();
        foreach (var node in nodes)
        {
            result.Add(node.worldPosition);
        }

        return result;
    }
}