using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个竞拍者信息
/// </summary>
public class AuctionBidder
{
    public EnemyType EnemyType;
    public Func<float> GetBidValue;           // 实时出价   只用于排序
    public float MaxBidValue;                 // 最大出价   用于扣除，确保总量不超出权限池
    public IAuctionListener Listener;         // 回调接口
    public Func<float> GetAngleToPlayer;      // 实时角度
    public Func<float> GetDisToPlayer;        // 实时距离
    public Func<TargetSlotType> GetSlotType;  // 槽位类型
    public Func<bool> IsAbnormal;             // 是否异常状态（动态）

    public AuctionBidder(
        EnemyType enemyType,
        float maxBidValue,
        IAuctionListener listener,
        Func<float> getAngleToPlayer,
        Func<float> getDisToPlayer,
        Func<TargetSlotType> getSlotType,
        Func<float> getBidValue,
        Func<bool> isAbnormal)
    {
        EnemyType = enemyType;
        Listener = listener;
        GetAngleToPlayer = getAngleToPlayer;
        GetDisToPlayer = getDisToPlayer;
        GetSlotType = getSlotType;
        GetBidValue = getBidValue;
        IsAbnormal = isAbnormal;
        MaxBidValue = maxBidValue;
    }

    public float CurrentAngleToPlayer => GetAngleToPlayer != null ? GetAngleToPlayer() : 0f;
    public float CurrentDistanceToPlayer => GetDisToPlayer != null ? GetDisToPlayer() : 0f;
}

/// <summary>
/// Token 封装类，AI 持有它，可主动释放
/// </summary>
public class AttackTokenHandle
{
    private AttackToken _token;
    private AttackAuctionSystem _system;
    private bool _released = false;

    public AttackTokenHandle(AttackToken token, AttackAuctionSystem system)
    {
        _token = token;
        _system = system;
    }

    /// <summary>
    /// 主动释放 token
    /// </summary>
    public void Release()
    {
        if (_released) return;
        _system.RecycleToken(_token);
        _released = true;
    }

    public bool IsValid => !_released;
}

/// <summary>
/// 攻击许可 Token（内部使用）
/// </summary>
public class AttackToken
{
    public IAuctionListener Owner;
    public float DesireValue;
    public Func<bool> IsAbnormal;
    public float AcquireTime;  // 获取时间
    public float Duration;     // 最大持有时间

    public AttackToken(IAuctionListener owner, float desireValue, Func<bool> isAbnormal, float duration)
    {
        Owner = owner;
        DesireValue = desireValue;
        IsAbnormal = isAbnormal;
        AcquireTime = Time.time;
        Duration = duration;
    }

    public bool IsExpired => Time.time - AcquireTime >= Duration;
}

/// <summary>
/// 回调接口
/// </summary>
public interface IAuctionListener
{
    void OnAuctionWon(AttackTokenHandle token);
    void OnAuctionLost();
    void OnTokenRevoked();
}

/// <summary>
/// 攻击许可池：动态增减资源
/// </summary>
public class AttackPermissionPool
{
    public float TotalPermission { get; private set; }
    private readonly float _maxPermission;

    public AttackPermissionPool(float total)
    {
        TotalPermission = total;
        _maxPermission = total;
    }

    public bool TryConsume(float value)
    {
        if (TotalPermission >= value)
        {
            TotalPermission -= value;
            return true;
        }
        return false;
    }

    public void Refund(float value)
    {
        TotalPermission = Mathf.Min(_maxPermission, TotalPermission + value);
    }
}

/// <summary>
/// 实时拍卖系统：随时分配与回收攻击权限
/// </summary>
public class AttackAuctionSystem : MonoBehaviour
{
    [Header("Auction Settings")]
    public float maxPermission = 30f;       // 最大攻击权限
    public float tokenDuration = 6f;        // token 最大持有时间（秒）
    public float angleWeight = 0.4f;
    public float distanceWeight = 0.3f;
    public float bidWeight = 0.3f;

    private AttackPermissionPool permissionPool;
    private readonly List<AuctionBidder> bidders = new List<AuctionBidder>();
    private readonly List<AttackToken> activeTokens = new List<AttackToken>();

    private void Start()
    {
        permissionPool = new AttackPermissionPool(maxPermission);
    }

    private void Update()
    {
        CheckAndRecycleTokens();
    }

    public void RegisterBidder(AuctionBidder bidder)
    {
        if (!bidders.Contains(bidder))
        {
            bidders.Add(bidder);
            TryDistributePermission();
        }
    }

    public void UnregisterBidder(AuctionBidder bidder)
    {
        bidders.Remove(bidder);

        var token = activeTokens.Find(t => t.Owner == bidder.Listener);
        if (token != null)
        {
            RecycleToken(token);
        }
    }

    /// <summary>
    /// 检查 token 是否过期或异常，回收
    /// </summary>
    private void CheckAndRecycleTokens()
    {
        for (int i = activeTokens.Count - 1; i >= 0; i--)
        {
            var token = activeTokens[i];
            if ((token.IsAbnormal != null && token.IsAbnormal()) || token.IsExpired)
            {
                RecycleToken(token);
            }
        }
    }

    /// <summary>
    /// 回收 token 并尝试补位
    /// </summary>
    public void RecycleToken(AttackToken token)
    {
        token.Owner?.OnTokenRevoked();
        permissionPool.Refund(token.DesireValue);
        activeTokens.Remove(token);

        TryDistributePermission();
    }

    /// <summary>
    /// 尝试从空闲的 bidder 中分配剩余资源
    /// </summary>
    private void TryDistributePermission()
    {
        var availableBidders = new List<AuctionBidder>();
        foreach (var bidder in bidders)
        {
            bool alreadyHasToken = activeTokens.Exists(t => t.Owner == bidder.Listener);
            if (!alreadyHasToken)
                availableBidders.Add(bidder);
        }

        if (availableBidders.Count == 0) return;

        availableBidders.Sort(CompareBidders);

        foreach (var bidder in availableBidders)
        {
            //为了确保场上总量不超出权限池，出价直接使用最大值
            float bidValue = bidder.MaxBidValue;
            if (permissionPool.TryConsume(bidValue))
            {
                var token = new AttackToken(bidder.Listener, bidValue, bidder.IsAbnormal, tokenDuration);
                activeTokens.Add(token);

                bidder.Listener?.OnAuctionWon(new AttackTokenHandle(token, this));
            }
            else
            {
                bidder.Listener?.OnAuctionLost();
            }

            if (permissionPool.TotalPermission <= 0)
                break;
        }
    }

    /// <summary>
    /// 综合排序
    /// </summary>
    public int CompareBidders(AuctionBidder a, AuctionBidder b)
    {
        int GetSlotPriority(TargetSlotType slotType, EnemyType enemyType)
        {
            if (enemyType == EnemyType.Near)
                return slotType == TargetSlotType.Attack ? 1 : 0;
            if (enemyType == EnemyType.Far)
                return 1;
            return 0;
        }

        // ===== ✅ 槽位匹配优先级 =====
        int slotPriorityA = GetSlotPriority(a.GetSlotType(), a.EnemyType);
        int slotPriorityB = GetSlotPriority(b.GetSlotType(), b.EnemyType);
        int slotCompare = slotPriorityB.CompareTo(slotPriorityA);
        if (slotCompare != 0) return slotCompare;

        // ===== ✅ 综合角度、距离、出价 =====
        // 数值含义：
        //  - 角度越小越好（目标正前方）
        //  - 距离越小越好（靠近玩家）
        //  - 出价越高越好
        // 所以角度与距离是“惩罚项”，出价是“奖励项”

        // 为避免角度和距离越大分越高，取负号
        float scoreA =
                -angleWeight * a.CurrentAngleToPlayer
                -distanceWeight * a.CurrentDistanceToPlayer
                +bidWeight * a.GetBidValue();

        float scoreB =
                -angleWeight * b.CurrentAngleToPlayer
                -distanceWeight * b.CurrentDistanceToPlayer
                +bidWeight * b.GetBidValue();

        return scoreB.CompareTo(scoreA); // 分高者优先
    }

}
