using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;
using TrafficCrossing.CoreGame.Serializeble;
using TrafficCrossing.CoreGame.Struct;

namespace TrafficCrossing.CoreGame
{
    public class TerrainSpawner : MonoBehaviour
    {
        [Header("Pool & Player References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private Transform _playerTransform;

        [Header("Terrain Configurations")]
        [SerializeField] private List<TerrainChunkConfig> _terrainConfigs = new List<TerrainChunkConfig>();

        [Header("Spawn Settings")]
        [SerializeField] private int _initialSafeCount = 4;
        [SerializeField] private float _aheadSpawnDistance = 15f;
        [SerializeField] private float _behindDespawnDistance = 8f;

        private float _currentSpawnY = 0f;
        private List<TerrainChunkConfig> _safeConfigs = new List<TerrainChunkConfig>();
        private Queue<ActiveChunkData> _activeChunks = new Queue<ActiveChunkData>();

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

            FilterSafeConfigs();
            SpawnInitialTerrains();

            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized || _playerTransform == null || _poolManager == null) return;

            HandleEndlessSpawning();
            HandleChunkDespawning();
        }

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

        private void SpawnInitialTerrains()
        {
            for (int i = 0; i < _initialSafeCount; i++)
            {
                if (_safeConfigs.Count > 0)
                {
                    TerrainChunkConfig safeConfig = _safeConfigs[UnityEngine.Random.Range(0, _safeConfigs.Count)];
                    SpawnChunk(safeConfig);
                }
                else if (_terrainConfigs.Count > 0)
                {
                    SpawnChunk(_terrainConfigs[0]);
                }
            }
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