using System.Collections.Generic;
using UnityEngine;

namespace Assets.PoolingSystem
{    

    /// <summary>
    /// Manages object pools defined by PoolCatalogSO assets.
    /// Reuses inactive GameObjects instead of repeatedly instantiating and destroying them,
    /// which keeps endless spawners (obstacles, platforms, etc.) lightweight.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        [SerializeField] private List<PoolCatalogSO> _catalogs;

        private Dictionary<PoolIdSO, Queue<GameObject>> _poolDictionary;
        private Dictionary<PoolIdSO, PoolDataSO> _poolDataDictionary;
        private Transform _poolContainer;

        /// <summary>
        /// Gets whether the pool manager has finished building its pools.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Creates the pool container and pre-warms every pool listed in the assigned catalogs.
        /// Must be called once, before any Spawn/Despawn calls — normally by GameManager.
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            _poolContainer = new GameObject("PoolContainer").transform;
            _poolContainer.SetParent(transform);

            BuildPoolsFromCatalogs();
            IsInitialized = true;
        }

        /// <summary>
        /// Reads every assigned catalog and registers each valid pool entry inside it.
        /// </summary>
        private void BuildPoolsFromCatalogs()
        {
            _poolDictionary = new Dictionary<PoolIdSO, Queue<GameObject>>();
            _poolDataDictionary = new Dictionary<PoolIdSO, PoolDataSO>();

            if (_catalogs == null || _catalogs.Count == 0)
            {
                Debug.LogWarning("[PoolManager] No catalogs assigned.");
                return;
            }

            foreach (PoolCatalogSO catalog in _catalogs)
            {
                if (catalog == null)
                {
                    continue;
                }

                foreach (PoolDataSO data in catalog.Pools)
                {
                    RegisterPool(data);
                }
            }
        }

        /// <summary>
        /// Validates a single pool entry, then pre-warms its starting instances.
        /// </summary>
        private void RegisterPool(PoolDataSO data)
        {
            if (data == null || data.PoolId == null || _poolDictionary.ContainsKey(data.PoolId))
            {
                return;
            }

            if (data.Prefab == null)
            {
                Debug.LogError($"[PoolManager] Pool data '{data.name}' has no prefab assigned. Pool was not registered.");
                return;
            }

            _poolDataDictionary[data.PoolId] = data;

            GameObject folder = new GameObject($"Pool_{data.PoolId.name}");
            folder.transform.SetParent(_poolContainer);

            Queue<GameObject> newPool = new Queue<GameObject>();

            for (int i = 0; i < data.InitialSize; i++)
            {
                newPool.Enqueue(CreatePooledInstance(data, folder.transform));
            }

            _poolDictionary.Add(data.PoolId, newPool);
        }

        /// <summary>
        /// Instantiates a fresh, inactive instance and parents it under its pool's folder.
        /// </summary>
        private GameObject CreatePooledInstance(PoolDataSO data, Transform parentFolder)
        {
            GameObject instance = Instantiate(data.Prefab, parentFolder);
            instance.SetActive(false);
            return instance;
        }

        /// <summary>
        /// Activates and positions a pooled object. Returns null if the pool doesn't exist
        /// or has no object available and isn't allowed to expand.
        /// </summary>
        public GameObject Spawn(PoolIdSO poolId, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[PoolManager] Spawn called before Initialize(). Make sure GameManager calls PoolManager.Initialize() first.");
                return null;
            }

            if (poolId == null || !_poolDictionary.ContainsKey(poolId))
            {
                Debug.LogWarning("[PoolManager] Tried to spawn from an unregistered pool.");
                return null;
            }

            GameObject instance = GetOrCreateInstance(poolId);

            if (instance == null)
            {
                return null;
            }

            ActivateInstance(instance, position, rotation, parent);
            return instance;
        }

        /// <summary>
        /// Gets an available instance from the queue, or creates a new one if the pool is expandable.
        /// </summary>
        private GameObject GetOrCreateInstance(PoolIdSO poolId)
        {
            Queue<GameObject> pool = _poolDictionary[poolId];

            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            PoolDataSO data = _poolDataDictionary[poolId];

            if (!data.IsExpandable)
            {
                return null;
            }

            Transform folder = _poolContainer.Find($"Pool_{poolId.name}");
            return CreatePooledInstance(data, folder);
        }

        /// <summary>
        /// Moves a pooled instance into position, re-parents it if requested, and activates it.
        /// </summary>
        private void ActivateInstance(GameObject instance, Vector3 position, Quaternion rotation, Transform parent)
        {
            instance.transform.position = position;
            instance.transform.rotation = rotation;

            if (parent != null)
            {
                instance.transform.SetParent(parent);
            }

            instance.SetActive(true);
        }

        /// <summary>
        /// Deactivates an object and returns it to its pool for reuse.
        /// Destroys the object instead if its pool isn't registered.
        /// </summary>
        public void Despawn(PoolIdSO poolId, GameObject instance)
        {
            if (poolId == null || !_poolDictionary.ContainsKey(poolId))
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);

            Transform folder = _poolContainer.Find($"Pool_{poolId.name}");
            instance.transform.SetParent(folder);

            _poolDictionary[poolId].Enqueue(instance);
        }
    }
}
