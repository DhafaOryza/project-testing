using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;
using Assets.PolyRef; // Menggunakan SubclassSelector
using TrafficCrossing.CoreGame.Obstacle;

namespace TrafficCrossing.CoreGame
{
    #region Obstacle Configurations (Polymorphic)

    [Serializable]
    public abstract class BaseObstacleConfig
    {
        // Class dasar untuk semua konfigurasi obstacle
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
        [SerializeField] private int _minGridGap = 3;
        [SerializeField] private int _maxGridGap = 6;

        // [Header("Vehicle Spawn Pattern Settings")]
        // [Tooltip("Repeating slot pattern. True = vehicle, False = empty.")]
        // [SerializeField] private bool[] _spawnPattern = new bool[0];

        [Header("Vehicle Limit Settings")]
        [SerializeField] private bool _useMaxVehicleCount = false;
        [SerializeField] private int _maxVehicleCount = 5;

        public PoolIdSO ObstaclePoolId => _obstaclePoolId;
        public Vector3 MoveDirection => _moveDirection;
        public float Speed => _speed;
        public float SpawnXOffset => _spawnXOffset;
        public float MinVehicleDistance => _minVehicleDistance;
        public float MaxVehicleDistance => _maxVehicleDistance;
        public float GridSize => _gridSize;
        public int MinGridGap => _minGridGap;
        public int MaxGridGap => _maxGridGap;
        // public bool[] SpawnPattern => _spawnPattern;
        public bool UseMaxVehicleCount => _useMaxVehicleCount;
        public int MaxVehicleCount => _maxVehicleCount;
    }

    [Serializable]
    public class MovingPlatformObstacleConfig : BaseObstacleConfig
    {
        [SerializeField] private PoolIdSO _obstaclePoolId;
        [SerializeField] private float _speed = 5f;

        [Header("Moving Platform Bounds")]
        [SerializeField] private float _leftBound = -5f;
        [SerializeField] private float _rightBound = 5f;

        public PoolIdSO ObstaclePoolId => _obstaclePoolId;
        public float Speed => _speed;
        public float LeftBound => _leftBound;
        public float RightBound => _rightBound;
    }

    #endregion

    /// <summary>
    /// Places one obstacle at an exact grid row inside a terrain chunk.
    /// Rows that have no placement entry stay completely empty.
    /// </summary>
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
        [Tooltip("Each entry places one obstacle at an exact grid row inside this terrain chunk (0 = first row, must be less than Grid Count). Rows you don't add here stay completely empty.")]
        [SerializeField] private List<ObstaclePlacement> _obstaclePlacements = new List<ObstaclePlacement>();

        public PoolIdSO PoolId => _poolId;
        public int GridCount => _gridCount;
        public float GridSize => _gridSize;

        /// <summary>
        /// Gets the total world-space length this chunk spans, i.e. GridCount multiplied by GridSize.
        /// </summary>
        public float TotalLength => _gridCount * _gridSize;

        public bool IsSafeStartingTerrain => _isSafeStartingTerrain;
        public List<ObstaclePlacement> ObstaclePlacements => _obstaclePlacements;
    }

    /// <summary>
    /// One spawned obstacle instance tracked by a chunk, so it can be despawned correctly later.
    /// PoolId is null for lightweight logic-only objects (like a VehicleSpawner anchor) that are
    /// just destroyed instead of being returned to a pool.
    /// </summary>
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
        public List<SpawnedObstacle> Obstacles;

        public ActiveChunkData(GameObject instance, PoolIdSO poolId, float yPosition, List<SpawnedObstacle> obstacles)
        {
            Instance = instance;
            PoolId = poolId;
            YPosition = yPosition;
            Obstacles = obstacles;
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

        private void InitializePoolManager()
        {
            if (_poolManager != null && !_poolManager.IsInitialized)
            {
                _poolManager.Initialize();
            }
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
                if (_terrainConfigs.Count == 0)
                {
                    break;
                }

                TerrainChunkConfig randomConfig = _terrainConfigs[UnityEngine.Random.Range(0, _terrainConfigs.Count)];
                SpawnChunk(randomConfig);
            }
        }

        private void SpawnChunk(TerrainChunkConfig config)
        {
            if (config == null || config.PoolId == null || _poolManager == null)
            {
                return;
            }

            Vector3 spawnPosition = new Vector3(0f, _currentSpawnY, 0f);
            GameObject chunkInstance = _poolManager.Spawn(config.PoolId, spawnPosition, Quaternion.identity);

            List<SpawnedObstacle> spawnedObstacles = new List<SpawnedObstacle>();

            if (chunkInstance != null)
            {
                foreach (ObstaclePlacement placement in config.ObstaclePlacements)
                {
                    SpawnObstacleAtPlacement(placement, chunkInstance, config.GridSize, spawnedObstacles);
                }
            }

            _activeChunks.Enqueue(new ActiveChunkData(chunkInstance, config.PoolId, _currentSpawnY, spawnedObstacles));
            _currentSpawnY += config.TotalLength;
        }

        /// <summary>
        /// Spawns a single obstacle at its configured grid row inside the current terrain chunk,
        /// dispatching to the right spawn logic based on the obstacle's concrete config type.
        /// Add a new "else if" branch here whenever a new obstacle type is introduced.
        /// </summary>
        private void SpawnObstacleAtPlacement(ObstaclePlacement placement, GameObject chunkInstance, float gridSize, List<SpawnedObstacle> spawnedObstacles)
        {
            if (placement == null || placement.ObstacleConfig == null)
            {
                return;
            }

            float rowY = _currentSpawnY + (placement.GridOffset * gridSize);
            Vector3 rowPosition = new Vector3(0f, rowY, 0f);

            if (placement.ObstacleConfig is VehicleObstacleConfig vehicleConfig && vehicleConfig.ObstaclePoolId != null)
            {
                SpawnVehicleRow(vehicleConfig, chunkInstance, rowPosition, placement.GridOffset, spawnedObstacles);
            }
            else if (placement.ObstacleConfig is MovingPlatformObstacleConfig platformConfig && platformConfig.ObstaclePoolId != null)
            {
                if (chunkInstance.TryGetComponent(out TerrainChunk terrainChunk))
                {
                    terrainChunk.HideRow(placement.GridOffset);
                }

                SpawnMovingPlatform(platformConfig, rowPosition, spawnedObstacles);
            }
        }

        /// <summary>
        /// Creates a dedicated VehicleSpawner anchor for one row, so each placed row keeps its own
        /// independent spawn timer/position even when several rows share the same terrain chunk.
        /// </summary>
        private void SpawnVehicleRow(VehicleObstacleConfig vehicleConfig, GameObject chunkInstance, Vector3 rowPosition, int gridOffset, List<SpawnedObstacle> spawnedObstacles)
        {
            GameObject spawnerObject = new GameObject($"VehicleSpawner_Row{gridOffset}");
            spawnerObject.transform.SetParent(chunkInstance.transform);
            spawnerObject.transform.position = rowPosition;

            VehicleSpawner vehicleSpawner = spawnerObject.AddComponent<VehicleSpawner>();
            vehicleSpawner.Initialize(
                _poolManager,
                vehicleConfig.ObstaclePoolId,
                vehicleConfig.Speed,
                vehicleConfig.MoveDirection,
                vehicleConfig.SpawnXOffset,
                vehicleConfig.MinVehicleDistance,
                vehicleConfig.MaxVehicleDistance,
                vehicleConfig.GridSize,
                vehicleConfig.MinGridGap,
                vehicleConfig.MaxGridGap,
                vehicleConfig.UseMaxVehicleCount,
                vehicleConfig.MaxVehicleCount
                // vehicleConfig.SpawnPattern
            );

            // PoolId left null: this is a lightweight logic-only object, not a pooled visual, so it's just destroyed on despawn.
            spawnedObstacles.Add(new SpawnedObstacle { Instance = spawnerObject, PoolId = null });
        }

        /// <summary>
        /// Spawns a single pooled moving platform at the given row position.
        /// </summary>
        private void SpawnMovingPlatform(MovingPlatformObstacleConfig platformConfig, Vector3 rowPosition, List<SpawnedObstacle> spawnedObstacles)
        {
            GameObject obstacleInstance = _poolManager.Spawn(platformConfig.ObstaclePoolId, rowPosition, Quaternion.identity);

            if (obstacleInstance == null)
            {
                return;
            }

            if (obstacleInstance.TryGetComponent(out MovingPlatform movingPlatform))
            {
                movingPlatform.MoveSpeed = platformConfig.Speed;
                movingPlatform.LeftBound = platformConfig.LeftBound;
                movingPlatform.RightBound = platformConfig.RightBound;
            }

            spawnedObstacles.Add(new SpawnedObstacle { Instance = obstacleInstance, PoolId = platformConfig.ObstaclePoolId });
        }

        private void HandleChunkDespawning()
        {
            while (_activeChunks.Count > 0)
            {
                ActiveChunkData oldestChunk = _activeChunks.Peek();

                if (_playerTransform.position.y - _behindDespawnDistance > oldestChunk.YPosition)
                {
                    _activeChunks.Dequeue();

                    DespawnChunkObstacles(oldestChunk.Obstacles);

                    _poolManager.Despawn(oldestChunk.PoolId, oldestChunk.Instance);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Despawns every obstacle placed inside a terrain chunk — pooled obstacles go back to their
        /// pool, non-pooled logic objects (like a VehicleSpawner anchor) are simply destroyed.
        /// </summary>
        private void DespawnChunkObstacles(List<SpawnedObstacle> obstacles)
        {
            if (obstacles == null)
            {
                return;
            }

            foreach (SpawnedObstacle obstacle in obstacles)
            {
                if (obstacle.Instance == null)
                {
                    continue;
                }

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