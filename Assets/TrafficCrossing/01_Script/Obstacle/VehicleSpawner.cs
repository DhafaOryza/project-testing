using UnityEngine;
using Assets.PoolingSystem;

namespace TrafficCrossing.CoreGame.Obstacle
{
    public class VehicleSpawner : MonoBehaviour
    {
        [Header("Pool References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolIdSO _vehiclePoolId;

        [Header("Vehicle Physics Settings")]
        [SerializeField] private float _vehicleSpeed = 6f;
        [SerializeField] private Vector3 _moveDirection = Vector3.right;

        [Header("Spawn Position Settings")]
        [SerializeField] private float _spawnXOffset = 10f;

        [Header("Distance & Grid Settings")]
        [SerializeField] private float _minVehicleDistance = 3f;
        [SerializeField] private float _maxVehicleDistance = 5f;
        [SerializeField] private float _gridSize = 1f;
        [SerializeField] private int _minGridGap = 3;
        [SerializeField] private int _maxGridGap = 6;

        [Header("Spawn Pattern Settings")]
        [Tooltip("Repeating slot pattern, one grid unit per slot. True = vehicle allowed to spawn on this slot, False = leave it empty. Leave this array empty to fall back to the random Min/Max Grid Gap behavior above.")]
        [SerializeField] private bool[] _spawnPattern = new bool[0];

        [Header("Vehicle Limit Settings")]
        [Tooltip("Centang jika ingin membatasi total kendaraan yang muncul di jalur ini.")]
        [SerializeField] private bool _useMaxVehicleCount = false;

        [Tooltip("Jumlah maksimal kendaraan yang akan di-spawn sebelum spawner berhenti.")]
        [SerializeField] private int _maxVehicleCount = 5;

        private float _spawnTimer;
        private float _nextSpawnInterval;
        private int _spawnedCount; // Menghitung total kendaraan yang sudah muncul

        private float _slotTimer;
        private int _patternIndex;

        public Vector3 MoveDirection
        {
            get { return _moveDirection; }
            private set { _moveDirection = value; }
        }

        private void Start()
        {
            SetNextGridSpawnInterval();
        }

        private void Update()
        {
            HandleSpawnTimer();
        }

        /// <summary>
        /// Dipanggil oleh TerrainSpawner untuk menginisialisasi parameter kendaraan.
        /// </summary>
        public void Initialize(
            PoolManager poolManager,
            PoolIdSO vehiclePoolId,
            float speed,
            Vector3 direction,
            float spawnXOffset,
            float minVehicleDistance,
            float maxVehicleDistance,
            float gridSize,
            int minGridGap,
            int maxGridGap,
            bool useMaxVehicleCount,
            int maxVehicleCount,
            bool[] spawnPattern = null)
        {
            _poolManager = poolManager;
            _vehiclePoolId = vehiclePoolId;
            _vehicleSpeed = speed;
            _moveDirection = direction.normalized;
            _spawnXOffset = spawnXOffset;
            _minVehicleDistance = Mathf.Max(1f, minVehicleDistance);
            _maxVehicleDistance = Mathf.Max(_minVehicleDistance,maxVehicleDistance);
            _gridSize = Mathf.Max(0.1f, gridSize);
            _minGridGap = Mathf.Max(1, minGridGap);
            _maxGridGap = Mathf.Max(_minGridGap, maxGridGap);

            _useMaxVehicleCount = useMaxVehicleCount;
            _maxVehicleCount = maxVehicleCount;

            // Only overwrite the Inspector-assigned pattern if the caller actually provided one.
            if (spawnPattern != null && spawnPattern.Length > 0)
            {
                _spawnPattern = spawnPattern;
            }

            _spawnedCount = 0; // Reset hitungan kendaraan tiap kali platform dibuat/di-pool
            _spawnTimer = 0f;
            _slotTimer = 0f;
            _patternIndex = 0;
            SetNextGridSpawnInterval();
        }

        private void HandleSpawnTimer()
        {
            if (_poolManager == null || _vehiclePoolId == null)
            {
                return;
            }

            // Jika batas maksimal diaktifkan dan kuota sudah habis, stop spawning
            if (_useMaxVehicleCount && _spawnedCount >= _maxVehicleCount)
            {
                return;
            }

            if (HasSpawnPattern())
            {
                HandlePatternSpawnTimer();
            }
            else
            {
                HandleRandomSpawnTimer();
            }
        }

        /// <summary>
        /// Gets whether a custom spawn pattern has been assigned for this lane.
        /// </summary>
        private bool HasSpawnPattern()
        {
            return _spawnPattern != null && _spawnPattern.Length > 0;
        }

        /// <summary>
        /// Steps through the spawn pattern one grid slot at a time, only spawning on slots marked true.
        /// This produces a fixed, designed traffic rhythm instead of pure randomness.
        /// </summary>
        private void HandlePatternSpawnTimer()
        {
            float timePerSlot = _gridSize / Mathf.Max(0.01f, _vehicleSpeed);
            _slotTimer += Time.deltaTime;

            if (_slotTimer < timePerSlot)
            {
                return;
            }

            _slotTimer -= timePerSlot;

            bool canSpawnThisSlot = _spawnPattern[_patternIndex % _spawnPattern.Length];
            _patternIndex++;

            if (canSpawnThisSlot)
            {
                SpawnVehicle();
            }
        }

        /// <summary>
        /// Original random-gap spawn timing, used as a fallback when no spawn pattern is assigned.
        /// </summary>
        private void HandleRandomSpawnTimer()
        {
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= _nextSpawnInterval)
            {
                SpawnVehicle();
                _spawnTimer = 0f;
                SetNextGridSpawnInterval();
            }
        }

        private void SpawnVehicle()
        {
            Vector3 spawnPosition = transform.position;

            if (_moveDirection.x > 0f)
            {
                spawnPosition.x -= _spawnXOffset;
            }
            else if (_moveDirection.x < 0f)
            {
                spawnPosition.x += _spawnXOffset;
            }

            float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject vehicleInstance = _poolManager.Spawn(_vehiclePoolId, spawnPosition, rotation);

            if (vehicleInstance != null)
            {
                _spawnedCount++; // Tambahkan hitungan jumlah kendaraan

                if (vehicleInstance.TryGetComponent(out Vehicle vehicle))
                {
                    vehicle.MoveSpeed = _vehicleSpeed;
                    vehicle.MoveDirection = _moveDirection;
                    vehicle.PoolManager = _poolManager;
                    vehicle.VehiclePoolId = _vehiclePoolId;
                }
            }
        }

        private void SetNextGridSpawnInterval()
        {
            if (_vehicleSpeed <= 0f || _gridSize <= 0f)
            {
                _nextSpawnInterval = 2f;
                return;
            }

            float randomVehicleDistance = Random.Range(_minVehicleDistance, _maxVehicleDistance);
            float minTimeByDistance = randomVehicleDistance / _vehicleSpeed;
            float timePerGridUnit = _gridSize / _vehicleSpeed;
            int randomGridGap = Random.Range(_minGridGap, _maxGridGap + 1);
            float timeByGrid = randomGridGap * timePerGridUnit;

            _nextSpawnInterval = Mathf.Max(minTimeByDistance, timeByGrid);
        }
    }
}