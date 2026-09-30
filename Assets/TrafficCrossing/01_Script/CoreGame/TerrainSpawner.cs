using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;

namespace TrafficCrossing.CoreGame
{
    /// <summary>
    /// Configuration data for a terrain chunk type in the Unity Inspector.
    /// </summary>
    [System.Serializable]
    public class TerrainChunkConfig
    {
        [Tooltip("Pool ID asset registered in PoolManager for this terrain prefab.")]
        [SerializeField] private PoolIdSO _poolId;

        [Tooltip("Jarak Y yang dibutuhkan platform ini sebelum menempatkan platform berikutnya. Sesuaikan jika platform memiliki MovePlatform atau beberapa jalur.")]
        [SerializeField] private float _stepOffset = 1f;

        [Tooltip("Centang jika platform ini aman untuk area awal (misal: Grass/Jalan polos tanpa obstacle).")]
        [SerializeField] private bool _isSafeStartingTerrain;

        public PoolIdSO PoolId => _poolId;
        public float StepOffset => _stepOffset;
        public bool IsSafeStartingTerrain => _isSafeStartingTerrain;
    }

    /// <summary>
    /// Tracks active pooled chunks to manage despawning.
    /// </summary>
    public struct ActiveChunkData
    {
        public GameObject Instance;
        public PoolIdSO PoolId;
        public float YPosition;

        public ActiveChunkData(GameObject instance, PoolIdSO poolId, float yPosition)
        {
            Instance = instance;
            PoolId = poolId;
            YPosition = yPosition;
        }
    }

    /// <summary>
    /// Generates endless terrain along the Y-axis using PoolManager and customizable spacing offsets.
    /// </summary>
    public class TerrainSpawner : MonoBehaviour
    {
        [Header("Pool & Player References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private Transform _playerTransform;

        [Header("Terrain Configurations")]
        [Tooltip("Daftar konfigurasi jenis platform yang akan di-spawn acak.")]
        [SerializeField] private List<TerrainChunkConfig> _terrainConfigs = new List<TerrainChunkConfig>();

        [Header("Spawn Settings")]
        [SerializeField] private int _initialSafeCount = 4;
        [SerializeField] private float _aheadSpawnDistance = 15f;
        [SerializeField] private float _behindDespawnDistance = 8f;

        private float _currentSpawnY = 0f;
        private List<TerrainChunkConfig> _safeConfigs = new List<TerrainChunkConfig>();
        private Queue<ActiveChunkData> _activeChunks = new Queue<ActiveChunkData>();

        private void Awake()
        {
            FindReferencesIfMissing();
            FilterSafeConfigs();
        }

        private void Start()
        {
            InitializePoolManager();
            SpawnInitialTerrains();
        }

        private void Update()
        {
            if (_playerTransform == null || _poolManager == null)
            {
                return;
            }

            HandleEndlessSpawning();
            HandleChunkDespawning();
        }

        /// <summary>
        /// Finds missing scene references automatically.
        /// </summary>
        private void FindReferencesIfMissing()
        {
            if (_poolManager == null)
            {
                _poolManager = FindFirstObjectByType<PoolManager>();
            }

            if (_playerTransform == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    _playerTransform = playerObj.transform;
                }
            }
        }

        /// <summary>
        /// Ensures PoolManager is initialized before spawning.
        /// </summary>
        private void InitializePoolManager()
        {
            if (_poolManager != null && !_poolManager.IsInitialized)
            {
                _poolManager.Initialize();
            }
        }

        /// <summary>
        /// Collects terrain configurations marked as safe for starting area.
        /// </summary>
        private void FilterSafeConfigs()
        {
            _safeConfigs.Clear();
            foreach (TerrainChunkConfig config in _terrainConfigs)
            {
                if (config.IsSafeStartingTerrain)
                {
                    _safeConfigs.Add(config);
                }
            }
        }

        /// <summary>
        /// Spawns safe initial platform chunks when starting the game.
        /// </summary>
        private void SpawnInitialTerrains()
        {
            // Spawn platform aman di awal
            for (int i = 0; i < _initialSafeCount; i++)
            {
                if (_safeConfigs.Count > 0)
                {
                    TerrainChunkConfig safeConfig = _safeConfigs[Random.Range(0, _safeConfigs.Count)];
                    SpawnChunk(safeConfig);
                }
                else if (_terrainConfigs.Count > 0)
                {
                    SpawnChunk(_terrainConfigs[0]);
                }
            }
        }

        /// <summary>
        /// Spawns new chunks ahead of the player as they move forward on the Y-axis.
        /// </summary>
        private void HandleEndlessSpawning()
        {
            while (_playerTransform.position.y + _aheadSpawnDistance > _currentSpawnY)
            {
                if (_terrainConfigs.Count == 0)
                {
                    break;
                }

                TerrainChunkConfig randomConfig = _terrainConfigs[Random.Range(0, _terrainConfigs.Count)];
                SpawnChunk(randomConfig);
            }
        }

        /// <summary>
        /// Spawns a single chunk using PoolManager and advances the Y-axis spawn position by StepOffset.
        /// </summary>
        private void SpawnChunk(TerrainChunkConfig config)
        {
            if (config == null || config.PoolId == null || _poolManager == null)
            {
                return;
            }

            Vector3 spawnPosition = new Vector3(0f, _currentSpawnY, 0f);
            GameObject chunkInstance = _poolManager.Spawn(config.PoolId, spawnPosition, Quaternion.identity);

            if (chunkInstance != null)
            {
                _activeChunks.Enqueue(new ActiveChunkData(chunkInstance, config.PoolId, _currentSpawnY));
            }

            // Menambahkan jarak Y sesuai batas Step Offset di Inspector
            _currentSpawnY += config.StepOffset;
        }

        /// <summary>
        /// Returns chunks that fell behind the player back to PoolManager.
        /// </summary>
        private void HandleChunkDespawning()
        {
            while (_activeChunks.Count > 0)
            {
                ActiveChunkData oldestChunk = _activeChunks.Peek();

                if (_playerTransform.position.y - _behindDespawnDistance > oldestChunk.YPosition)
                {
                    _activeChunks.Dequeue();
                    _poolManager.Despawn(oldestChunk.PoolId, oldestChunk.Instance);
                }
                else
                {
                    break;
                }
            }
        }
    }
}