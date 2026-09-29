using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.PoolingSystem;

namespace TrafficCrossing.CoreGame
{
    /// <summary>
    /// Moves a vehicle continuously in a given direction and despawns via PoolManager when reaching distance limits.
    /// </summary>
    public class Vehicle : MonoBehaviour
    {

        [Header("Despawn Settings")]
        [SerializeField] private float _maxTravelDistance = 40f;

        [Header("Pool References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolIdSO _vehiclePoolId;

        private float _speed = 5f;
        private Vector3 _direction = Vector3.right;
        private Vector3 _startPosition;

        /// <summary>
        /// Gets or sets the movement speed of the vehicle.
        /// </summary>
        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /// <summary>
        /// Gets or sets the normalized direction vector for movement.
        /// </summary>
        public Vector3 Direction
        {
            get { return _direction; }
            set { _direction = value.normalized; }
        }

        /// <summary>
        /// Gets or sets the PoolManager reference used for despawning.
        /// </summary>
        public PoolManager PoolManager
        {
            get { return _poolManager; }
            set { _poolManager = value; }
        }

        /// <summary>
        /// Gets or sets the PoolIdSO identifier for this vehicle.
        /// </summary>
        public PoolIdSO VehiclePoolId
        {
            get { return _vehiclePoolId; }
            set { _vehiclePoolId = value; }
        }

        private void Awake()
        {
            FindPoolManagerIfMissing();
        }

        private void OnEnable()
        {
            _startPosition = transform.position;
            FindPoolManagerIfMissing();
        }

        private void Update()
        {
            MoveVehicle();
            CheckTravelDistance();
        }

        /// <summary>
        /// Automatically finds and assigns the PoolManager instance in the scene if missing.
        /// </summary>
        private void FindPoolManagerIfMissing()
        {
            if (_poolManager == null)
            {
                _poolManager = FindFirstObjectByType<PoolManager>();
            }
        }

        /// <summary>
        /// Advances the vehicle in world space according to direction and speed.
        /// </summary>
        private void MoveVehicle()
        {
            transform.Translate(_direction * (_speed * Time.deltaTime), Space.World);
        }

        /// <summary>
        /// Despawns the vehicle back to the pool once it travels past the max distance threshold.
        /// </summary>
        private void CheckTravelDistance()
        {
            if (Vector3.Distance(_startPosition, transform.position) >= _maxTravelDistance)
            {
                DespawnVehicle();
            }
        }

        /// <summary>
        /// Returns the vehicle instance to PoolManager or destroys it as fallback.
        /// </summary>
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

        private void OnTriggerEnter(Collider other)
        {
            CheckPlayerCollision(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckPlayerCollision(collision.gameObject);
        }

        /// <summary>
        /// Evaluates if the collided object is tagged as Player and reloads the scene on impact.
        /// </summary>
        private void CheckPlayerCollision(GameObject target)
        {
            if (target.CompareTag("Player"))
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// Reloads the current active scene upon player collision.
        /// </summary>
        private void TriggerGameOver()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}