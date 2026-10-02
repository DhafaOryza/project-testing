using UnityEngine;
using TopDownArenaSurvival.CoreGame;
using Assets.PoolingSystem;

namespace TopDownArenaSurvival.CoreGame
{
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
        private Rigidbody2D _rigidbody;
        private float _attackTimer;
        private HealthManager _healthManager;
        private PoolManager _poolManager;

        public Transform Player
        {
            get { return _player; }
            set { _player = value; }
        }

        public void Initialize(PoolManager poolManager, Transform player)
        {
            _poolManager = poolManager;
            _player = player;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _healthManager = GetComponent<HealthManager>();
        }

        private void OnEnable()
        {
            if (_healthManager != null)
            {
                _healthManager.OnDied += HandleEnemyDeath;
                _healthManager.ResetHealth();
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
            if (_player == null) return;

            Vector2 direction = _player.position - transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void ChasePlayer()
        {
            if (_player == null) return;

            // 3. Kejar pemain di bidang 2D
            Vector2 direction = (_player.position - transform.position).normalized;
            _rigidbody.linearVelocity = direction * _moveSpeed;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDealContactDamage(collision.gameObject);
        }

        private void TryDealContactDamage(GameObject target)
        {
            if (!target.CompareTag("Player") || _attackTimer < _attackCooldown) return;

            if (target.TryGetComponent(out HealthManager health))
            {
                health.TakeDamage(_contactDamage);
                _attackTimer = 0f;
            }
        }
    }
}