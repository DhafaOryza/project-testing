using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;
using TrafficCrossing.CoreGame.Serializeble;

namespace TrafficCrossing.CoreGame
{
    public class TerrainSpawner : MonoBehaviour
    {
        public struct ActiveChunkData
        {
            public GameObject Instance;
            public PoolIdSO PoolId;
            public float YPosition;
            public float TotalLength;
            public List<SpawnedObstacle> Obstacles;

            public ActiveChunkData(GameObject instance, PoolIdSO poolId, float yPosition, float totalLength, List<SpawnedObstacle> obstacles)
            {
                Instance = instance;
                PoolId = poolId;
                YPosition = yPosition;
                Obstacles = obstacles;
                TotalLength = totalLength;
            }
        }

        public struct SpawnedObstacle
        {
            public GameObject Instance;
            public PoolIdSO PoolId;
        }

        [Header("Pool & Player References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private Transform _playerTransform;

        [Header("Terrain Configurations")]
        [SerializeField] private List<TerrainChunkConfig> _terrainConfigs = new List<TerrainChunkConfig>();

        [Header("Spawn Settings")]
        [SerializeField] private int _initialSafeCount = 10;
        [SerializeField] private float _aheadSpawnDistance = 20f;
        [SerializeField] private float _behindDespawnDistance = 20f;

        private float _currentSpawnY = 0f;
        private Queue<ActiveChunkData> _activeChunks = new Queue<ActiveChunkData>();

        private int _nextTerrainIndex = 0;
        private bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Dipanggil eksklusif oleh GameManager untuk menginisialisasi TerrainSpawner beserta dependensinya.
        /// </summary>
        public void Initialize(PoolManager poolManager, Transform playerTransform)
        {
            if (_isInitialized) return;

            _poolManager = poolManager;
            _playerTransform = playerTransform;

            _nextTerrainIndex = 0;
            SpawnInitialTerrains();
            HandleEndlessSpawning();

            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized || _playerTransform == null || _poolManager == null) return;

            HandleEndlessSpawning();
            HandleChunkDespawning();
        }

        private void HandleEndlessSpawning()
        {
            while (_playerTransform.position.y + _aheadSpawnDistance > _currentSpawnY)
            {
                if (_terrainConfigs.Count == 0) break;

                TerrainChunkConfig randomConfig = _terrainConfigs[UnityEngine.Random.Range(0, _terrainConfigs.Count)];
                SpawnChunk(randomConfig);
            }
        }

        /// <summary>
        /// Men-spawn chunk awal sebanyak _initialSafeCount secara berurutan.
        /// </summary>
        private void SpawnInitialTerrains()
        {
            for (int i = 0; i < _initialSafeCount; i++)
            {
                SpawnSingleSequentialChunk();
            }
        }

        /// <summary>
        /// Mengambil terrain secara berurutan 0, 1, 2... lalu kembali ke 0 (looping).
        /// </summary>
        private void SpawnSingleSequentialChunk()
        {
            for (int i = 0; i < _initialSafeCount; i++)
            {                
                if (_terrainConfigs == null || _terrainConfigs.Count == 0) return;

                TerrainChunkConfig config = _terrainConfigs[_nextTerrainIndex];
                SpawnChunk(config);

                _nextTerrainIndex = (_nextTerrainIndex + 1) % _terrainConfigs.Count; 
            }
        }

        private void SpawnChunk(TerrainChunkConfig config)
        {
            if (config == null || config.PoolId == null || _poolManager == null) return;

            Vector3 spawnPosition = new Vector3(0f, _currentSpawnY, 0f);
            GameObject chunkInstance = _poolManager.Spawn(config.PoolId, spawnPosition, Quaternion.identity);

            if (chunkInstance == null) return;

            List<SpawnedObstacle> spawnedObstacles = new List<SpawnedObstacle>();

            foreach (ObstaclePlacement placement in config.ObstaclePlacements)
            {
                SpawnObstacleAtPlacement(placement, chunkInstance, config.GridSize, spawnedObstacles);
            }

            _activeChunks.Enqueue(new ActiveChunkData(chunkInstance, config.PoolId, _currentSpawnY, config.TotalLength, spawnedObstacles));
            _currentSpawnY += config.TotalLength;
        }

        private void SpawnObstacleAtPlacement(ObstaclePlacement placement, GameObject chunkInstance, float gridSize, List<SpawnedObstacle> spawnedObstacles)
        {
            if (placement == null || placement.ObstacleConfig == null) return;

            float rowY = _currentSpawnY + (placement.GridOffset * gridSize);
            Vector3 rowPosition = new Vector3(0f, rowY, 0f);

            placement.ObstacleConfig.SpawnObstacle(_poolManager, rowPosition, chunkInstance, placement.GridOffset, spawnedObstacles);
        }

        private void HandleChunkDespawning()
        {
            while (_activeChunks.Count > 0)
            {
                ActiveChunkData oldestChunk = _activeChunks.Peek();
                float chunkTopPosition = oldestChunk.YPosition + oldestChunk.TotalLength;

                if (_playerTransform.position.y - _behindDespawnDistance <= chunkTopPosition)
                {
                    break;
                }

                _activeChunks.Dequeue();
                DespawnChunkObstacles(oldestChunk.Obstacles);
                _poolManager.Despawn(oldestChunk.PoolId, oldestChunk.Instance);
            }
        }

        private void DespawnChunkObstacles(List<SpawnedObstacle> obstacles)
        {
            if (obstacles == null) return;

            foreach (SpawnedObstacle obstacle in obstacles)
            {
                if (obstacle.Instance == null) continue;

                if (obstacle.PoolId != null)
                {
                    _poolManager.Despawn(obstacle.PoolId, obstacle.Instance);
                }
                else
                {
                    Destroy(obstacle.Instance);
                }
            }
        }
    }
}