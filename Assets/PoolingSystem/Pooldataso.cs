using UnityEngine;

namespace Assets.PoolingSystem
{
    /// <summary>
    /// Configuration data for a single pool: which prefab it spawns,
    /// how many instances to pre-warm, and whether it can grow at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPoolData", menuName = "Pooling/Pool Data")]
    public class PoolDataSO : ScriptableObject
    {
        [SerializeField] private PoolIdSO _poolId;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _initialSize = 5;
        [SerializeField] private bool _isExpandable = true;

        /// <summary>
        /// Gets the unique identifier used to spawn/despawn from this pool.
        /// </summary>
        public PoolIdSO PoolId => _poolId;

        /// <summary>
        /// Gets the prefab this pool creates instances from.
        /// </summary>
        public GameObject Prefab => _prefab;

        /// <summary>
        /// Gets how many instances are pre-created when the pool is built.
        /// </summary>
        public int InitialSize => _initialSize;

        /// <summary>
        /// Gets whether the pool is allowed to create extra instances when it runs out.
        /// </summary>
        public bool IsExpandable => _isExpandable;
    }
}
