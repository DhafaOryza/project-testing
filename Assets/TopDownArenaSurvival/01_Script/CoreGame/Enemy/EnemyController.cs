using UnityEngine;
using TopDownArenaSurvival.CoreGame;
using Assets.PoolingSystem;

namespace TopDownArenaSurvival.CoreGame
{
    /// <summary>
    /// Makes the enemy chase and face the player on the X-Z plane, and deal contact damage on touch.
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PoolIdSO _poolIdSO;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 3f;

        [Header("Attack Settings")]
        [SerializeField] private int _contactDamage = 10;
        [SerializeField] private float _attackCooldown = 1f;

        private Transform _player;
        private Rigidbody _rigidbody;
        private float _attackTimer;
        private HealthManager _healthManager;
        private PoolManager _poolManager;

        /// <summary>
        /// Gets or sets the target player transform.
        /// </summary>
        public Transform Player
        {
            get { return _player; }
            set { _player = value; }
        }

        /// <summary>
        /// Inisialisasi yang dipanggil oleh EnemySpawner saat memunculkan musuh dari pool.
        /// </summary>
        public void Initialize(PoolManager poolManager, Transform player)
        {
            _poolManager = poolManager;
            _player = player;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _healthManager = GetComponent<HealthManager>();
        }

        private void OnEnable()
        {
            if (_healthManager != null)
            {
                _healthManager.OnDied += HandleEnemyDeath;
                _healthManager.ResetHealth(); // Mengembalikan nyawa ke penuh setiap kali spawn dari pool
            }
        }

        private void OnDisable()
        {
            if (_healthManager != null)
            {
                _healthManager.OnDied -= HandleEnemyDeath;
            }
        }

        private void HandleEnemyDeath()
        {
            // Kembalikan ke pool jika PoolManager tersedia, jika tidak baru jalankan Destroy
            if (_poolManager != null)
            {
                _poolManager.Despawn(_poolIdSO, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            FindPlayerIfMissing();
        }

        private void Update()
        {
            _attackTimer += Time.deltaTime;
            RotateToPlayer();
        }

        private void FixedUpdate()
        {
            ChasePlayer();
        }

        private void FindPlayerIfMissing()
        {
            if (_player == null)
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                if (playerObject != null)
                {
                    _player = playerObject.transform;
                }
            }
        }

        private void RotateToPlayer()
        {
            if (_player == null)
            {
                return;
            }

            Vector3 direction = _player.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void ChasePlayer()
        {
            if (_player == null)
            {
                return;
            }

            Vector3 direction = _player.position - transform.position;
            direction.y = 0f;
            direction.Normalize();

            Vector3 targetVelocity = direction * _moveSpeed;
            _rigidbody.linearVelocity = new Vector3(targetVelocity.x, _rigidbody.linearVelocity.y, targetVelocity.z);
        }

        private void OnCollisionStay(Collision collision)
        {
            TryDealContactDamage(collision.gameObject);
        }

        private void TryDealContactDamage(GameObject target)
        {
            if (!target.CompareTag("Player") || _attackTimer < _attackCooldown)
            {
                return;
            }

            if (target.TryGetComponent(out HealthManager health))
            {
                health.TakeDamage(_contactDamage);
                _attackTimer = 0f;
            }
        }
    }
}