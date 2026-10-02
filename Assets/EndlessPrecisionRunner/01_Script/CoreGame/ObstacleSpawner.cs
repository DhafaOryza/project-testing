using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;

namespace EndlessPrecisionRunner.CoreGame
{    
    /// <summary>
    /// Spawns obstacles ahead of the player at random time intervals using PoolManager,
    /// and returns obstacles to their pool once they fall far behind the player.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct ObstacleSpawnData
        {
            [SerializeField] private PoolIdSO _poolId;
            [SerializeField] private float _spawnHeightY;

            public PoolIdSO PoolId => _poolId;
            public float SpawnHeightY => _spawnHeightY;
        }

        private struct ActiveObstacle
        {
            public GameObject Instance { get; set; }
            public PoolIdSO PoolId { get; set; }
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
        private bool _isInitialized;
        private readonly List<ActiveObstacle> _activeObstacles = new List<ActiveObstacle>();

        public bool IsInitialized => _isInitialized;
        public int ActiveObstacleCount => _activeObstacles.Count;

        /// <summary>
        /// Dipanggil eksklusif oleh GameManager untuk menginisialisasi ObstacleSpawner.
        /// </summary>
        public void Initialize(PoolManager poolManager, Transform player)
        {
            if (_isInitialized) return;

            _poolManager = poolManager;
            _player = player;

            PickNextSpawnInterval();
            _isInitialized = true;
        }

        private void Update()
        {
            // Mencegah eksekusi jika GameManager belum menyelesaikan Initialize()
            if (!_isInitialized || _player == null || _poolManager == null) return;

            HandleSpawnTimer();
            DespawnObstaclesBehindPlayer();
        }

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

        private void PickNextSpawnInterval()
        {
            _nextSpawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        }

        private void SpawnObstacle()
        {
            if (_obstacleSpawnData == null || _obstacleSpawnData.Length == 0) return;

            ObstacleSpawnData selectedObstacle = _obstacleSpawnData[Random.Range(0, _obstacleSpawnData.Length)];
            if (selectedObstacle.PoolId == null) return;

            Vector3 spawnPosition = new Vector3(
                _player.position.x + _spawnDistanceAheadOfPlayer,
                selectedObstacle.SpawnHeightY,
                _player.position.z);

            GameObject instance = _poolManager.Spawn(selectedObstacle.PoolId, spawnPosition, Quaternion.identity);

            if (instance != null)
            {
                _activeObstacles.Add(new ActiveObstacle 
                { 
                    Instance = instance, 
                    PoolId = selectedObstacle.PoolId 
                });
            }
        }

        private void DespawnObstaclesBehindPlayer()
        {
            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                ActiveObstacle obstacle = _activeObstacles[i];

                if (obstacle.Instance == null)
                {
                    _activeObstacles.RemoveAt(i);
                    continue;
                }

                float distanceBehindPlayer = _player.position.x - obstacle.Instance.transform.position.x;

                if (distanceBehindPlayer > _despawnDistanceBehindPlayer)
                {
                    _poolManager.Despawn(obstacle.PoolId, obstacle.Instance);
                    _activeObstacles.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Mengembalikan seluruh obstacle aktif ke pool (dipanggil oleh GameManager saat reset/restart).
        /// </summary>
        public void DespawnAllObstacles()
        {
            if (_poolManager == null) return;

            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                ActiveObstacle obstacle = _activeObstacles[i];
                if (obstacle.Instance != null && obstacle.PoolId != null)
                {
                    _poolManager.Despawn(obstacle.PoolId, obstacle.Instance);
                }
            }

            _activeObstacles.Clear();
        }
    }
}