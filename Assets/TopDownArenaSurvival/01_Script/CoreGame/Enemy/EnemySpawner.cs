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

        /// <summary>
        /// Gets the radius distance from the player where enemies will spawn.
        /// </summary>
        public float SpawnRadius
        {
            get { return _spawnRadius; }
            private set { _spawnRadius = value; }
        }

        /// <summary>
        /// Gets the interval in seconds between each enemy spawn.
        /// </summary>
        public float SpawnInterval
        {
            get { return _spawnInterval; }
            private set { _spawnInterval = value; }
        }

        private void Start()
        {
            FindPlayerIfMissing();
        }

        private void Update()
        {
            HandleSpawnTimer();
        }

        /// <summary>
        /// Automatically assigns the player reference if omitted in the Inspector.
        /// </summary>
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

        /// <summary>
        /// Accumulates time and triggers enemy spawn once the spawn interval is reached.
        /// </summary>
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

        /// <summary>
        /// Spawns an enemy instance from the pool at a random point along the perimeter of the spawn radius.
        /// </summary>
        private void SpawnEnemy()
        {
            // IMPORTANT: Normalizing the 2D circle vector ensures the position is on the outer edge, not inside the circle.
            Vector2 randomCirclePoint = Random.insideUnitCircle.normalized * _spawnRadius;
            Vector3 spawnPosition = _playerTransform.position + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);

            GameObject enemyInstance = _poolManager.Spawn(_enemyPoolId, spawnPosition, Quaternion.identity);

            if (enemyInstance != null && enemyInstance.TryGetComponent(out EnemyController enemyController))
            {
                enemyController.Player = _playerTransform;
            }
        }
    }
}