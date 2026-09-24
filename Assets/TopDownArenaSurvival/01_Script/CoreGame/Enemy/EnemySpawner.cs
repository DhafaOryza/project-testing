using UnityEngine;
using Assets.PoolingSystem;

namespace TopDownArenaSurvival.CoreGame
{
    /// <summary>
    /// Spawns enemies periodically along a circle boundary around the player using the pooling system.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolIdSO _enemyPoolId;
        [SerializeField] private Transform _playerTransform;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private float _spawnRadius = 12f;

        private float _spawnTimer;

        public float SpawnRadius => _spawnRadius;
        public float SpawnInterval => _spawnInterval;

        /// <summary>
        /// Inisialisasi spawner dari GameManager.
        /// </summary>
        public void Initialize(PoolManager poolManager)
        {
            _poolManager = poolManager; // [TAMBAHAN] Terima PoolManager dari GameManager
            FindPlayerIfMissing();
        }

        private void Update()
        {
            HandleSpawnTimer();
        }

        private void FindPlayerIfMissing()
        {
            if (_playerTransform == null)
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                if (playerObject != null)
                {
                    _playerTransform = playerObject.transform;
                }
            }
        }

        private void HandleSpawnTimer()
        {
            if (_playerTransform == null || _poolManager == null)
            {
                return;
            }

            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= _spawnInterval)
            {
                SpawnEnemy();
                _spawnTimer = 0f;
            }
        }

        private void SpawnEnemy()
        {
            Vector2 randomCirclePoint = Random.insideUnitCircle.normalized * _spawnRadius;
            Vector3 spawnPosition = _playerTransform.position + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);

            GameObject enemyInstance = _poolManager.Spawn(_enemyPoolId, spawnPosition, Quaternion.identity);

            if (enemyInstance != null && enemyInstance.TryGetComponent(out EnemyController enemyController))
            {
                // [PERBAIKAN] Langsung inisialisasi PoolManager & Player sekaligus ke enemy
                enemyController.Initialize(_poolManager, _playerTransform);
            }
        }
    }
}