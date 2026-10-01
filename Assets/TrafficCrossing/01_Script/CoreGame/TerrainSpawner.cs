using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;
using TrafficCrossing.CoreGame.Obstacle;

namespace TrafficCrossing.CoreGame
{
    public enum ObstacleType
    {
        None,
        Vehicle,
        MovingPlatform
    }

    [System.Serializable]
    public class ObstacleConfig
    {
        [SerializeField] private ObstacleType _type = ObstacleType.None;
        [SerializeField] private PoolIdSO _obstaclePoolId;
        [SerializeField] private Vector3 _moveDirection = Vector3.right;
        [SerializeField] private float _speed = 5f;

        [Header("Vehicle Spawn Position Settings")]
        [SerializeField] private float _spawnXOffset = 10f;

        [Header("Vehicle Distance & Grid Settings")]
        [SerializeField] private float _minVehicleDistance = 3f;
        [SerializeField] private float _gridSize = 1f;
        [SerializeField] private int _minGridGap = 3;
        [SerializeField] private int _maxGridGap = 6;

        [Header("Vehicle Spawn Pattern Settings")]
        [Tooltip("Repeating slot pattern, one grid unit per slot. True = vehicle allowed to spawn on this slot, False = leave it empty. Leave this array empty to fall back to the random Min/Max Grid Gap settings above.")]
        [SerializeField] private bool[] _spawnPattern = new bool[0];

        [Header("Vehicle Limit Settings")]
        [Tooltip("Centang untuk membatasi total kendaraan yang muncul pada platform ini.")]
        [SerializeField] private bool _useMaxVehicleCount = false;

        [Tooltip("Jumlah maksimal kendaraan yang dimunculkan.")]
        [SerializeField] private int _maxVehicleCount = 5;

        [Header("Moving Platform Bounds")]
        [SerializeField] private float _leftBound = -5f;
        [SerializeField] private float _rightBound = 5f;

        public ObstacleType Type => _type;
        public PoolIdSO ObstaclePoolId => _obstaclePoolId;
        public Vector3 MoveDirection => _moveDirection;
        public float Speed => _speed;
        public float SpawnXOffset => _spawnXOffset;
        public float MinVehicleDistance => _minVehicleDistance;
        public float GridSize => _gridSize;
        public int MinGridGap => _minGridGap;
        public int MaxGridGap => _maxGridGap;
        public bool[] SpawnPattern => _spawnPattern;
        public bool UseMaxVehicleCount => _useMaxVehicleCount;
        public int MaxVehicleCount => _maxVehicleCount;
        public float LeftBound => _leftBound;
        public float RightBound => _rightBound;
    }

    [System.Serializable]
    public class TerrainChunkConfig
    {
        [Header("Platform Settings")]
        [SerializeField] private PoolIdSO _poolId;
        [SerializeField] private float _stepOffset = 1f;
        [SerializeField] private bool _isSafeStartingTerrain;

        [Header("Obstacle Settings")]
        [SerializeField] private ObstacleConfig _obstacleConfig;

        public PoolIdSO PoolId => _poolId;
        public float StepOffset => _stepOffset;
        public bool IsSafeStartingTerrain => _isSafeStartingTerrain;
        public ObstacleConfig ObstacleConfig => _obstacleConfig;
    }

    public struct ActiveChunkData
    {
        public GameObject Instance;
        public PoolIdSO PoolId;
        public float YPosition;
        public GameObject ObstacleInstance;
        public PoolIdSO ObstaclePoolId;

        public ActiveChunkData(GameObject instance, PoolIdSO poolId, float yPosition, GameObject obstacleInstance = null, PoolIdSO obstaclePoolId = null)
        {
            Instance = instance;
            PoolId = poolId;
            YPosition = yPosition;
            ObstacleInstance = obstacleInstance;
            ObstaclePoolId = obstaclePoolId;
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
                    TerrainChunkConfig safeConfig = _safeConfigs[Random.Range(0, _safeConfigs.Count)];
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

                TerrainChunkConfig randomConfig = _terrainConfigs[Random.Range(0, _terrainConfigs.Count)];
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

            GameObject obstacleInstance = null;
            PoolIdSO obstaclePoolId = null;

            if (chunkInstance != null && config.ObstacleConfig != null)
            {
                ObstacleConfig obstacle = config.ObstacleConfig;

                if (obstacle.Type == ObstacleType.Vehicle && obstacle.ObstaclePoolId != null)
                {
                    if (!chunkInstance.TryGetComponent(out VehicleSpawner vehicleSpawner))
                    {
                        vehicleSpawner = chunkInstance.AddComponent<VehicleSpawner>();
                    }

                    vehicleSpawner.Initialize(
                        _poolManager,
                        obstacle.ObstaclePoolId,
                        obstacle.Speed,
                        obstacle.MoveDirection,
                        obstacle.SpawnXOffset,
                        obstacle.MinVehicleDistance,
                        obstacle.GridSize,
                        obstacle.MinGridGap,
                        obstacle.MaxGridGap,
                        obstacle.UseMaxVehicleCount,
                        obstacle.MaxVehicleCount,
                        obstacle.SpawnPattern
                    );
                }
                else if (obstacle.Type == ObstacleType.MovingPlatform && obstacle.ObstaclePoolId != null)
                {
                    obstacleInstance = _poolManager.Spawn(obstacle.ObstaclePoolId, spawnPosition, Quaternion.identity);
                    obstaclePoolId = obstacle.ObstaclePoolId;

                    if (obstacleInstance != null && obstacleInstance.TryGetComponent(out MovingPlatform movingPlatform))
                    {
                        movingPlatform.MoveSpeed = obstacle.Speed;
                        movingPlatform.LeftBound = obstacle.LeftBound;
                        movingPlatform.RightBound = obstacle.RightBound;
                    }
                }

                _activeChunks.Enqueue(new ActiveChunkData(chunkInstance, config.PoolId, _currentSpawnY, obstacleInstance, obstaclePoolId));
            }

            _currentSpawnY += config.StepOffset;
        }

        private void HandleChunkDespawning()
        {
            while (_activeChunks.Count > 0)
            {
                ActiveChunkData oldestChunk = _activeChunks.Peek();

                if (_playerTransform.position.y - _behindDespawnDistance > oldestChunk.YPosition)
                {
                    _activeChunks.Dequeue();

                    if (oldestChunk.ObstacleInstance != null && oldestChunk.ObstaclePoolId != null)
                    {
                        _poolManager.Despawn(oldestChunk.ObstaclePoolId, oldestChunk.ObstacleInstance);
                    }

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