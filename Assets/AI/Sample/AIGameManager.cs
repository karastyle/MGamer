using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    /// <summary>
    /// 全局游戏管理器（单例）
    /// 管理场景中关键对象，如 Player、EntityRegister、CombatSpawner 等。
    /// </summary>
    public class AIGameManager : MonoBehaviour
    {
        private static AIGameManager instance;
        public static bool IsInitialized => instance != null;

        public static AIGameManager Instance
        {
            get
            {
                // 避免域重载后引用失效
                if (instance == null)
                {
                    instance = FindObjectOfType<AIGameManager>();

                    if (instance == null)
                    {
                        Debug.LogWarning("[GameManager] No instance found in scene, creating one dynamically.");
                        var go = new GameObject("GameManager (AutoCreated)");
                        instance = go.AddComponent<AIGameManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return instance;
            }
        }

        [Header("References")]
        public Player player;
        public UtilityEntityRegister entityRegister;
        public CombatSpawner combatSpawner;
        public GradientDescentManager gradientManager;
        public AttackAuctionSystem attackAuctionSystem;
        
        public static Player Player => IsInitialized ? Instance.player : null;
        public static UtilityEntityRegister EntityRegister => IsInitialized ? Instance.entityRegister : null;
        public static CombatSpawner Spawner => IsInitialized ? Instance.combatSpawner : null;
        public static GradientDescentManager GradientManager => IsInitialized ? Instance.gradientManager : null;
        public static AttackAuctionSystem AttackAuction => IsInitialized ? Instance.attackAuctionSystem : null;

        private void Awake()
        {
            // 防止重复实例
            if (instance != null && instance != this)
            {
                Debug.LogWarning("[GameManager] Duplicate found, destroying: " + gameObject.name);
                DestroyImmediate(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 确保编辑器中引用不为空（方便调试）
            if (!player)
                player = FindObjectOfType<Player>();
            if (!combatSpawner)
                combatSpawner = FindObjectOfType<CombatSpawner>();
            if (!entityRegister)
                entityRegister = FindObjectOfType<UtilityEntityRegister>();
            if (!gradientManager)
                gradientManager = FindObjectOfType<GradientDescentManager>();
            if (!attackAuctionSystem)
                attackAuctionSystem = FindObjectOfType<AttackAuctionSystem>();
        }
#endif
    }
}
