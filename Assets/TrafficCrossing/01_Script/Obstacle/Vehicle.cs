using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.PoolingSystem;

namespace TrafficCrossing.CoreGame.Obstacle
{
    public class Vehicle : BaseObstacle
    {
        [Header("Vehicle Despawn Settings")]
        [SerializeField] private float _maxTravelDistance = 40f;

        [Header("Pool References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolIdSO _vehiclePoolId;

        private Vector3 _startPosition;

        public PoolManager PoolManager
        {
            get => _poolManager;
            set => _poolManager = value;
        }

        public PoolIdSO VehiclePoolId
        {
            get => _vehiclePoolId;
            set => _vehiclePoolId = value;
        }

        private void OnEnable()
        {
            _startPosition = transform.position;
            FindPoolManagerIfMissing();
        }

        protected override void Move()
        {
            // Pergerakan konstan ke satu arah
            transform.Translate(MoveDirection * (_moveSpeed * Time.deltaTime), Space.World);
            CheckTravelDistance();
        }

        protected override void HandlePlayerTouch(GameObject player)
        {
            // Menabrak mobil = Game Over
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void CheckTravelDistance()
        {
            if (Vector3.Distance(_startPosition, transform.position) >= _maxTravelDistance)
            {
                DespawnVehicle();
            }
        }

        private void DespawnVehicle()
        {
            if (_poolManager != null && _vehiclePoolId != null)
            {
                _poolManager.Despawn(_vehiclePoolId, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void FindPoolManagerIfMissing()
        {
            if (_poolManager == null)
            {
                _poolManager = FindFirstObjectByType<PoolManager>();
            }
        }
    }
}