using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;
using Assets.PolyRef;
using TrafficCrossing.CoreGame.Obstacle;

namespace TrafficCrossing.CoreGame
{
    #region Obstacle Configurations (Polymorphic)

    [Serializable]
    public abstract class BaseObstacleConfig
    {
        public abstract void SpawnObstacle(
            PoolManager poolManager, 
            Vector3 position, 
            GameObject chunkInstance, 
            int gridOffset, 
            List<SpawnedObstacle> spawnedObstacles);
    }

    [Serializable]
    public class VehicleObstacleConfig : BaseObstacleConfig
    {
        [SerializeField] private PoolIdSO _obstaclePoolId;
        [SerializeField] private Vector3 _moveDirection = Vector3.right;
        [SerializeField] private float _speed = 5f;

        [Header("Vehicle Spawn Position Settings")]
        [SerializeField] private float _spawnXOffset = 10f;

        [Header("Vehicle Distance & Grid Settings")]
        [SerializeField] private float _minVehicleDistance = 3f;
        [SerializeField] private float _maxVehicleDistance = 5f;
        [SerializeField] private float _gridSize = 1f;


        public PoolIdSO ObstaclePoolId => _obstaclePoolId;
        public Vector3 MoveDirection => _moveDirection;
        public float Speed => _speed;
        public float SpawnXOffset => _spawnXOffset;
        public float MinVehicleDistance => _minVehicleDistance;
        public float MaxVehicleDistance => _maxVehicleDistance;
        public float GridSize => _gridSize;

        public override void SpawnObstacle(
            PoolManager poolManager, 
            Vector3 position, 
            GameObject chunkInstance, 
            int gridOffset, 
            List<SpawnedObstacle> spawnedObstacles)
        {
            if (poolManager == null || _obstaclePoolId == null) return;

            GameObject spawnerObject = new GameObject($"VehicleSpawner_Row{gridOffset}");
            spawnerObject.transform.SetParent(chunkInstance.transform);
            spawnerObject.transform.position = position;

            VehicleSpawner vehicleSpawner = spawnerObject.AddComponent<VehicleSpawner>();
            vehicleSpawner.Initialize(poolManager, this);

            // PoolId null karena ini object anchor logic-only
            spawnedObstacles.Add(new SpawnedObstacle { Instance = spawnerObject, PoolId = null });
        }
    }

    [Serializable]
    public class MovingPlatformObstacleConfig : BaseObstacleConfig
    {
        [SerializeField] private PoolIdSO _obstaclePoolId;
        [SerializeField] private PoolIdSO _waterPoolId;
        [SerializeField] private float _speed = 5f;

        [Header("Moving Platform Bounds")]
        [SerializeField] private float _leftBound = -5f;
        [SerializeField] private float _rightBound = 5f;
        [SerializeField] private SpawnSide _spawnSide = SpawnSide.Left;

        public PoolIdSO ObstaclePoolId => _obstaclePoolId;
        public PoolIdSO WaterPoolId => _waterPoolId;
        public float Speed => _speed;
        public float LeftBound => _leftBound;
        public float RightBound => _rightBound;
        public SpawnSide SpawnSide => _spawnSide;

        public override void SpawnObstacle(
            PoolManager poolManager, 
            Vector3 position, 
            GameObject chunkInstance, 
            int gridOffset, 
            List<SpawnedObstacle> spawnedObstacles)
        {
            if (poolManager == null) return;

            // 1. Spawn Water jika ada
            if (_waterPoolId != null)
            {
                GameObject waterInstance = poolManager.Spawn(_waterPoolId, position, Quaternion.identity);
                if (waterInstance != null)
                {
                    spawnedObstacles.Add(new SpawnedObstacle { Instance = waterInstance, PoolId = _waterPoolId });
                }
            }

            // 2. Spawn Moving Platform
            if (_obstaclePoolId != null)
            {
                GameObject obstacleInstance = poolManager.Spawn(_obstaclePoolId, position, Quaternion.identity);

                if (obstacleInstance != null)
                {
                    if (obstacleInstance.TryGetComponent(out MovingPlatform movingPlatform))
                    {
                        movingPlatform.MoveSpeed = _speed;
                        movingPlatform.LeftBound = _leftBound;
                        movingPlatform.RightBound = _rightBound;
                        movingPlatform.SpawnSide = _spawnSide;
                    }

                    spawnedObstacles.Add(new SpawnedObstacle { Instance = obstacleInstance, PoolId = _obstaclePoolId });
                }
            }
        }
    }

    #endregion

    [Serializable]
    public class ObstaclePlacement
    {
        [Tooltip("Grid row offset from the start of this terrain chunk (0 = the chunk's first row).")]
        [SerializeField] private int _gridOffset;

        [SerializeReference, SubclassSelector]
        private BaseObstacleConfig _obstacleConfig;

        public int GridOffset => _gridOffset;
        public BaseObstacleConfig ObstacleConfig => _obstacleConfig;
    }

    [Serializable]
    public class TerrainChunkConfig
    {
        [Header("Platform Settings")]
        [SerializeField] private PoolIdSO _poolId;

        [Tooltip("How many grid cells long this terrain chunk is (e.g. 30 means it spans 30 rows).")]
        [SerializeField] private int _gridCount = 1;

        [Tooltip("World-space size of one grid cell. Should match the player's hop grid size.")]
        [SerializeField] private float _gridSize = 1f;

        [SerializeField] private bool _isSafeStartingTerrain;

        [Header("Obstacle Placements")]
        [SerializeField] private List<ObstaclePlacement> _obstaclePlacements = new List<ObstaclePlacement>();

        public PoolIdSO PoolId => _poolId;
        public int GridCount => _gridCount;
        public float GridSize => _gridSize;
        public float TotalLength => _gridCount * _gridSize;
        public bool IsSafeStartingTerrain => _isSafeStartingTerrain;
        public List<ObstaclePlacement> ObstaclePlacements => _obstaclePlacements;
    }

    public struct SpawnedObstacle
    {
        public GameObject Instance;
        public PoolIdSO PoolId;
    }

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

        // Awake() dan Start() DIHAPUS agar tidak ada spawning ganda!

        private void Update()
        {
            // Hanya berjalan jika sudah di-Initialize oleh GameManager
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