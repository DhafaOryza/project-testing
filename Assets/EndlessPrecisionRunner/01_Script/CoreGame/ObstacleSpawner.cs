using UnityEngine;
using System.Collections.Generic;
using Assets.PoolingSystem;

namespace EndlessPrecisionRunner.CoreGame
{    
    /// <summary>
    /// Spawns obstacles ahead of the player at random time intervals using PoolManager,
    /// and returns obstacles to their pool once they fall far behind the player.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        /// <summary>
        /// Defines the pool ID and its specific spawn height to handle different obstacle sizes.
        /// </summary>
        [System.Serializable]
        public struct ObstacleSpawnData
        {
            [SerializeField] private PoolIdSO _poolId;
            [SerializeField] private float _spawnHeightY;

            public PoolIdSO PoolId 
            { 
                get { return _poolId; } 
                private set { _poolId = value; } 
            }

            public float SpawnHeightY 
            { 
                get { return _spawnHeightY; } 
                private set { _spawnHeightY = value; } 
            }
        }

        /// <summary>
        /// Pairs a spawned obstacle instance with the pool it came from, so it can be despawned correctly.
        /// </summary>
        private struct ActiveObstacle
        {
            private GameObject _instance;
            private PoolIdSO _poolId;

            public GameObject Instance 
            { 
                get { return _instance; } 
                set { _instance = value; } 
            }

            public PoolIdSO PoolId 
            { 
                get { return _poolId; } 
                set { _poolId = value; } 
            }
        }

        [Header("References")]
        [SerializeField] private Transform _player;
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private ObstacleSpawnData[] _obstacleSpawnData;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnDistanceAheadOfPlayer = 20f;
        [SerializeField] private float _minSpawnInterval = 1f;
        [SerializeField] private float _maxSpawnInterval = 2.5f;

        [Header("Cleanup Settings")]
        [SerializeField] private float _despawnDistanceBehindPlayer = 15f;

        private float _spawnTimer;
        private float _nextSpawnInterval;
        private readonly List<ActiveObstacle> _activeObstacles = new List<ActiveObstacle>();

        // public Transform Player 
        // { 
        //     get { return _player; } 
        //     private set { _player = value; } 
        // }

        // public PoolManager PoolManager 
        // { 
        //     get { return _poolManager; } 
        //     private set { _poolManager = value; } 
        // }

        // public ObstacleSpawnData[] ObstacleSpawnDataArray 
        // { 
        //     get { return _obstacleSpawnData; } 
        //     private set { _obstacleSpawnData = value; } 
        // }

        // public float SpawnDistanceAheadOfPlayer 
        // { 
        //     get { return _spawnDistanceAheadOfPlayer; } 
        //     private set { _spawnDistanceAheadOfPlayer = value; } 
        // }

        // public float MinSpawnInterval 
        // { 
        //     get { return _minSpawnInterval; } 
        //     private set { _minSpawnInterval = value; } 
        // }

        // public float MaxSpawnInterval 
        // { 
        //     get { return _maxSpawnInterval; } 
        //     private set { _maxSpawnInterval = value; } 
        // }

        // public float DespawnDistanceBehindPlayer 
        // { 
        //     get { return _despawnDistanceBehindPlayer; } 
        //     private set { _despawnDistanceBehindPlayer = value; } 
        // }

        /// <summary>
        /// Gets how many obstacles are currently active in the scene.
        /// </summary>
        public int ActiveObstacleCount => _activeObstacles.Count;

        /// <summary>
        /// Initializes the first spawn interval on start.
        /// </summary>
        private void Start()
        {
            PickNextSpawnInterval();
        }

        /// <summary>
        /// Handles timing for spawning and cleans up objects that fall behind the player every frame.
        /// </summary>
        private void Update()
        {
            HandleSpawnTimer();
            DespawnObstaclesBehindPlayer();
        }

        /// <summary>
        /// Counts up the spawn timer and triggers a new obstacle spawn once it reaches the target interval.
        /// </summary>
        private void HandleSpawnTimer()
        {
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= _nextSpawnInterval)
            {
                SpawnObstacle();
                _spawnTimer = 0f;
                PickNextSpawnInterval();
            }
        }

        /// <summary>
        /// Picks a new random interval to wait before spawning the next obstacle.
        /// </summary>
        private void PickNextSpawnInterval()
        {
            _nextSpawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        }

        /// <summary>
        /// Spawns a random obstacle pool at a fixed distance ahead of the player using its unique Y height.
        /// </summary>
        private void SpawnObstacle()
        {
            if (_obstacleSpawnData.Length == 0)
            {
                return;
            }

            ObstacleSpawnData selectedObstacle = _obstacleSpawnData[Random.Range(0, _obstacleSpawnData.Length)];
            
            Vector3 spawnPosition = new Vector3(
                _player.position.x + _spawnDistanceAheadOfPlayer,
                selectedObstacle.SpawnHeightY,
                _player.position.z);

            GameObject instance = _poolManager.Spawn(selectedObstacle.PoolId, spawnPosition, Quaternion.identity);

            if (instance != null)
            {
                _activeObstacles.Add(new ActiveObstacle { Instance = instance, PoolId = selectedObstacle.PoolId });
            }
        }

        /// <summary>
        /// Returns obstacles to their pool once they have gone far enough behind the player.
        /// </summary>
        private void DespawnObstaclesBehindPlayer()
        {
            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                ActiveObstacle obstacle = _activeObstacles[i];
                float distanceBehindPlayer = _player.position.x - obstacle.Instance.transform.position.x;

                if (distanceBehindPlayer > _despawnDistanceBehindPlayer)
                {
                    _poolManager.Despawn(obstacle.PoolId, obstacle.Instance);
                    _activeObstacles.RemoveAt(i);
                }
            }
        }
    }
}