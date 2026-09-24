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
        private Transform _player;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 3f;

        [Header("Attack Settings")]
        [SerializeField] private int _contactDamage = 10;
        [SerializeField] private float _attackCooldown = 1f;

        private Rigidbody _rigidbody;
        private float _attackTimer;
        private HealthManager _healthManager;
        private PoolManager _poolmanager;

        /// <summary>
        /// Gets or sets the target player transform.
        /// </summary>
        public Transform Player
        {
            get { return _player; }
            set { _player = value; }
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
            Destroy(gameObject);
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

        /// <summary>
        /// Automatically finds and assigns the player reference if not assigned in Inspector.
        /// </summary>
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

        /// <summary>
        /// Rotates the enemy transform to face directly toward the player on the X-Z plane.
        /// </summary>
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

        /// <summary>
        /// Moves the enemy toward the player's current position on the X-Z plane.
        /// </summary>
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

        /// <summary>
        /// Deals contact damage to the player once the attack cooldown has elapsed.
        /// </summary>
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