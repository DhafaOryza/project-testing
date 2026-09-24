using UnityEngine;
using Assets.PoolingSystem;
using TopDownArenaSurvival.CoreGame;

namespace TopDownArenaSurvival.CoreGame
{    
    /// <summary>
    /// Moves forward in a straight line after being launched, and returns itself
    /// to its pool after a short lifetime or when it hits an enemy.
    /// </summary>
    public class Projectile : MonoBehaviour
    {

        [Header("Settings")]
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private int _damage = 10;

        private Vector3 _direction;
        private PoolIdSO _poolId;
        private PoolManager _poolManager;
        private float _lifetimeTimer;

        private void Update()
        {
            MoveForward();
            HandleLifetime();
        }

        /// <summary>
        /// Sets up the projectile's travel direction and resets its lifetime timer.
        /// Call this right after the projectile is spawned from the pool.
        /// </summary>
        public void Launch(Vector3 direction, PoolIdSO poolId, PoolManager poolManager)
        {
            _direction = direction;
            _poolId = poolId;
            _poolManager = poolManager;
            _lifetimeTimer = 0f;
        }

        /// <summary>
        /// Moves the projectile forward every frame along its launch direction.
        /// </summary>
        private void MoveForward()
        {
            transform.position += _direction * _speed * Time.deltaTime;
        }

        /// <summary>
        /// Returns the projectile to its pool once its lifetime runs out, so stray shots don't linger forever.
        /// </summary>
        private void HandleLifetime()
        {
            _lifetimeTimer += Time.deltaTime;

            if (_lifetimeTimer >= _lifetime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy") && other.TryGetComponent(out HealthManager health))
            {
                health.TakeDamage(_damage);
                ReturnToPool();
            }
        }

        /// <summary>
        /// Deactivates the projectile and sends it back to its pool for reuse.
        /// </summary>
        private void ReturnToPool()
        {
            _poolManager.Despawn(_poolId, gameObject);
        }
    }
}