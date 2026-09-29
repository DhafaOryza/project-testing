using UnityEngine;
using Assets.PoolingSystem;

namespace TrafficCrossing.CoreGame.Obstacle
{
    /// <summary>
    /// Spawns vehicle instances periodically using PoolManager instead of direct instantiation.
    /// </summary>
    public class VehicleSpawner : MonoBehaviour
    {
        [Header("Pool References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolIdSO _vehiclePoolId;

        [Header("Spawn Settings")]
        [SerializeField] private float _minSpawnInterval = 1.5f;
        [SerializeField] private float _maxSpawnInterval = 3.5f;
        [SerializeField] private float _vehicleSpeed = 6f;
        [SerializeField] private Vector3 _moveDirection = Vector3.right;

        private float _spawnTimer;
        private float _nextSpawnInterval;

        /// <summary>
        /// Gets the current movement direction configured for spawned vehicles.
        /// </summary>
        public Vector3 MoveDirection
        {
            get { return _moveDirection; }
            private set { _moveDirection = value; }
        }

        private void Start()
        {
            SetRandomSpawnInterval();
        }

        private void Update()
        {
            HandleSpawnTimer();
        }

        /// <summary>
        /// Accumulates frame time and spawns a vehicle from the pool when reaching the interval threshold.
        /// </summary>
        private void HandleSpawnTimer()
        {
            if (_poolManager == null || _vehiclePoolId == null)
            {
                return;
            }

            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= _nextSpawnInterval)
            {
                SpawnVehicle();
                _spawnTimer = 0f;
                SetRandomSpawnInterval();
            }
        }

        /// <summary>
        /// Spawns a vehicle instance from PoolManager and configures its properties.
        /// </summary>
        private void SpawnVehicle()
        {
            Quaternion rotation = Quaternion.LookRotation(_moveDirection);
            GameObject vehicleInstance = _poolManager.Spawn(_vehiclePoolId, transform.position, rotation);

            if (vehicleInstance != null && vehicleInstance.TryGetComponent(out Vehicle vehicle))
            {
                vehicle.Speed = _vehicleSpeed;
                vehicle.Direction = _moveDirection;
                vehicle.PoolManager = _poolManager;
                vehicle.VehiclePoolId = _vehiclePoolId;
            }
        }

        /// <summary>
        /// Calculates a random duration before spawning the next vehicle.
        /// </summary>
        private void SetRandomSpawnInterval()
        {
            _nextSpawnInterval = Random.Range(_minSpawnInterval, _maxSpawnInterval);
        }
    }
}