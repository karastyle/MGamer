using System.Collections.Generic;
using System.Linq;
using CarlosLab.UtilityIntelligence;
using CarlosLab.UtilityIntelligence.Examples;
using UnityEngine;
using UnityEngine.AI;

[ExecuteAlways]
public class Enemy : UtilityAgentFacade, IAuctionListener
{
    public RoleType roleType = RoleType.Enemy;
    public EnemyType enemyType = EnemyType.Near;

    public int enemyId;  // ✅ 唯一编号
    private NavMeshAgent agent;

    // ✅ 当前正在使用的 Slot
    private TargetSlot currentSlot;

    [Header("移动参数")]
    public float moveSpeed = 3.5f;
    public float patrolSpeed = 1.5f;
    public float switchRadius = 1.5f;

    [Header("加速度参数")]
    public float moveAcceleration = 30f;
    public float patrolAcceleration = 8f;
    public float transitionSmooth = 3f;

    [Header("是否达到的距离")]
    public float arrivedRadio = 0.5f;

    [Header("梯度下降参数")]
    public float frontCheckDistance = 1.0f;  //前后距离
    public float sideCheckDistance = 1.0f;   //左右距离

    private float[,] sdfGrid;
    private Vector3[,] gridWorldPositions;
    
    private bool isGradientRunning = false;
    
    // ✅ 梯度下降运行时数据
    private Vector3 bestGradientPos;
    private readonly List<Vector3> gradientCandidates = new();
    
    // 攻击权限参数
    [Header("拍卖参数")]
    public float desireValue = 0;
    public float maxDesireValue = 0;
    public float desireRecoverRate = 10f; // 每秒恢复速率
    public float angleToPlayer = 0;
    public float disToPlayer = 0;
    
    private AttackTokenHandle tokenHandle;
    
    private void Start()
    {
        if (Application.isPlaying)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
                agent = gameObject.AddComponent<NavMeshAgent>();

            agent.stoppingDistance = 0.1f;
            agent.autoBraking = true;

            agent.speed = moveSpeed;
            agent.acceleration = moveAcceleration;

            // 初始参数
            switch (enemyType)
            {
                case EnemyType.Far:
                    desireValue = 10;
                    maxDesireValue = 10;
                    break;
                case EnemyType.Near:
                    desireValue = 30;
                    maxDesireValue = 30;
                    break;
            }

            var auctionSystem = AIGameManager.AttackAuction;
            
            AuctionBidder bidder = new AuctionBidder(
                maxBidValue: this.maxDesireValue,
                enemyType: this.enemyType,
                getSlotType: () => currentSlot?currentSlot.slotType:TargetSlotType.None,
                getBidValue: () => desireValue,
                listener: this,
                getAngleToPlayer: () => angleToPlayer,   // ✅ 使用委托，实时读取当前角度
                getDisToPlayer: () => disToPlayer,       // ✅ 使用委托，实时读取当前距离
                isAbnormal: IsAbnormal
            );
            
            auctionSystem.RegisterBidder(bidder);
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        UpdateMoveSpeedSmooth();

        UpdateAngleToPlayer();

        UpdateDisToPlayer();
        
        RecoverDesire();
    }
    
    private void UpdateDisToPlayer()
    {
        var player = AIGameManager.Player.transform;
        disToPlayer = Vector3.Distance(transform.position, player.position);
    }
    
    private void UpdateAngleToPlayer()
    {
        // 玩家面朝方向（忽略Y）
        var player = AIGameManager.Player.transform;
        Vector3 playerForward = player.forward;
        playerForward.y = 0f;

        // 从玩家指向敌人的方向
        Vector3 toEnemy = (transform.position - player.position);
        toEnemy.y = 0f;

        if (toEnemy.sqrMagnitude < 0.001f)
            return;

        // 计算夹角   45度内视为正面，应该有相同的优先级
        angleToPlayer = Vector3.Angle(playerForward, toEnemy);
        if (angleToPlayer < 45)
        {
            angleToPlayer = 0;
        }
    }

    #region === 原逻辑不变 ===

    private void UpdateMoveSpeedSmooth()
    {
        if (currentSlot != null && agent != null && agent.isOnNavMesh)
        {
            float dist = Vector3.Distance(transform.position, currentSlot.transform.position);
            float targetSpeed = dist <= switchRadius ? patrolSpeed : moveSpeed;
            float targetAccel = dist <= switchRadius ? patrolAcceleration : moveAcceleration;

            agent.speed = Mathf.Lerp(agent.speed, targetSpeed, Time.deltaTime * transitionSmooth);
            agent.acceleration = Mathf.Lerp(agent.acceleration, targetAccel, Time.deltaTime * transitionSmooth);
        }
    }

    public bool MoveToSlot(TargetSlot newSlot)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ MoveToSlot 仅在运行时有效。");
            return false;
        }

        if (newSlot == null)
        {
            Debug.LogWarning($"{name}: MoveToSlot 被调用但传入的 slot 为 null。");
            return false;
        }

        if (newSlot.usedEnemeyId != 0 && newSlot.usedEnemeyId != this.enemyId)
        {
            Debug.LogWarning("⚠️ 目标 Slot 已被其他敌人占用，无法移动到该 Slot。");
            return false;
        }

        StopAllMovement();
        
        ReleaseSlot();

        currentSlot = newSlot;
        currentSlot.usedEnemeyId = this.enemyId;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(currentSlot.transform.position);
            return true;
        }
        else
        {
            Debug.LogWarning($"{name}: NavMeshAgent 不在 NavMesh 上，无法移动到 Slot。");
            return false;
        }
    }

    public void StopMoving()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public bool IsAgentStopped() =>
        agent == null || agent.isStopped || !agent.hasPath;

    public TargetSlot GetCurrentSlot() => currentSlot;
    public bool IsSlotUsedByMe() => this.currentSlot && this.currentSlot.usedEnemeyId == this.enemyId;
    public bool IsTargetSlotUsedByMe(TargetSlot targetSlot) => targetSlot && targetSlot.usedEnemeyId == this.enemyId;

    public void ReleaseSlot()
    {
        if (currentSlot != null && this.IsSlotUsedByMe())
        {
            currentSlot.usedEnemeyId = 0;
            currentSlot = null;
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            ReleaseSlot();
        }
    }

    public float GetScoreBySlotType(TargetSlotType slotType)
    {
        switch (enemyType)
        {
            case EnemyType.Far:
                return slotType switch
                {
                    TargetSlotType.Attack => 0.5f,
                    TargetSlotType.Keep => 0.6f,
                    TargetSlotType.Shoot => 1.0f,
                    _ => 0f
                };
            case EnemyType.Near:
                return slotType switch
                {
                    TargetSlotType.Attack => 1.0f,
                    TargetSlotType.Keep => 0.9f,
                    TargetSlotType.Shoot => 0.5f,
                    _ => 0f
                };
            default:
                return 0f;
        }
    }

    public bool IsPositionWalkable(Vector3 position, float maxDistance = 0.1f)
    {
        return NavMesh.SamplePosition(position, out _, maxDistance, NavMesh.AllAreas);
    }

    public bool IsArrived()
    {
        if (currentSlot == null)
            return false;
        var pos = currentSlot.transform.position;
        var dis = Vector3.Distance(pos, transform.position);
        return dis <= arrivedRadio;
    }

    #endregion


    #region === 梯度下降逻辑 ===

    public List<Vector3> GenerateCandidatePoints()
    {
        var player = AIGameManager.Player.transform;
        Vector3 playerPos = player.position;
        Vector3 enemyPos = transform.position;
        Vector3 forward = (playerPos - enemyPos).normalized;

        var innerRadius = AIGameManager.Player.InnerRadius;
        var finalFrontDis = frontCheckDistance + innerRadius;

        Vector3 frontPoint = playerPos + forward * finalFrontDis;
        Vector3 backPoint = playerPos - forward * finalFrontDis;

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        var finalSideDis = sideCheckDistance + innerRadius;
        Vector3 leftPoint = playerPos - right * finalSideDis;
        Vector3 rightPoint = playerPos + right * finalSideDis;

        gradientCandidates.Clear();
        gradientCandidates.AddRange(new List<Vector3> { frontPoint, backPoint, leftPoint, rightPoint, playerPos });

        return gradientCandidates;
    }
    
    private void ComputeGradientPosition()
    {
        var gradientManager = AIGameManager.GradientManager;

        gradientManager.StartGradientDescent(enemyId, GenerateCandidatePoints, (id, bestPos) =>
        {
            if (id != enemyId) return;

            bestGradientPos = bestPos;
            MoveToGradientPosition();
        });
    }

    private void MoveToGradientPosition()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(bestGradientPos);
        }
    }
    
    public void StartGradientDescent()
    {
        if (isGradientRunning) return;
        StopAllMovement();
        
        isGradientRunning = true;
        ComputeGradientPosition();
    }
    
    public void StopGradientDescent()
    {
        isGradientRunning = false;
        var gradientManager = AIGameManager.GradientManager;
        gradientManager.StopGradientDescent(this.enemyId);
    }
    
    public void StopAllMovement()
    {
        StopGradientDescent();
        StopMoving();
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        if (!this.isGradientRunning)
            return;

        if (gradientCandidates != null && gradientCandidates.Count > 0)
        {
            Gizmos.color = Color.blue;
            foreach (var p in gradientCandidates)
                Gizmos.DrawSphere(p, 0.1f);
        }
    }
#endif


    #endregion

    public void OnAuctionWon(AttackTokenHandle handle)
    {
        Debug.Log($"{this.enemyId} 赢得拍卖！（消耗 {this.desireValue}）");
        desireValue = 0;
        this.tokenHandle = handle;
    }
    

    public void OnAuctionLost()
    {
    }

    //token失效
    public void OnTokenRevoked()
    {
        Debug.Log($"{this.enemyId} token 失效！");
        tokenHandle = null;
    }

    private void RecoverDesire()
    {
        // 若当前正在攻击，也允许慢慢回升（可按需要禁用）
        if (desireValue < maxDesireValue)
        {
            desireValue += desireRecoverRate * Time.deltaTime;
            desireValue = Mathf.Min(desireValue, maxDesireValue);
        }
    }
    
    //是否处于异常， 用于回收token
    private bool IsAbnormal()
    {
        return false;
    }
    
    public bool HasAttackToken()
    {
        return tokenHandle != null && tokenHandle.IsValid;
    }
    
    //发起攻击
    public void DoAttack()
    {
        StopAllMovement();
        
        //回收token
        tokenHandle?.Release();
        tokenHandle = null;
    }


}
