using UnityEngine;
using Assets.PoolingSystem;

namespace TopDownArenaSurvival.CoreGame
{    
    /// <summary>
    /// Automatically fires projectiles at the nearest enemy at a fixed interval.
    /// Enemies must be tagged "Enemy" to be detected.
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PoolIdSO _projectilePoolId;
        [SerializeField] private Transform _firePoint;

        [Header("Fire Settings")]
        [SerializeField] private float _fireInterval = 0.5f;
        [SerializeField] private float _detectionRange = 10f;

        private float _fireTimer;

        /// <summary>
        /// Gets the currently detected nearest enemy, or null if none are in range.
        /// </summary>
        public Transform CurrentTarget { get; private set; }

        private void Update()
        {
            FindNearestEnemy();
            HandleFireTimer();
        }

        /// <summary>
        /// Searches all GameObjects tagged "Enemy" and keeps the closest one that's within detection range.
        /// </summary>
        private void FindNearestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearestEnemy = null;
            float nearestDistance = _detectionRange;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy.transform;
                }
            }

            CurrentTarget = nearestEnemy;
        }

        /// <summary>
        /// Counts up the fire timer and shoots once it reaches the fire interval, as long as a target exists.
        /// </summary>
        private void HandleFireTimer()
        {
            _fireTimer += Time.deltaTime;

            if (_fireTimer >= _fireInterval && CurrentTarget != null)
            {
                FireProjectile();
                _fireTimer = 0f;
            }
        }

        /// <summary>
        /// Spawns a projectile from the pool and launches it toward the current target.
        /// </summary>
        private void FireProjectile()
        {
            Vector3 direction = (CurrentTarget.position - _firePoint.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction);

            GameObject instance = _poolManager.Spawn(_projectilePoolId, _firePoint.position, rotation);

            if (instance != null && instance.TryGetComponent(out Projectile projectile))
            {
                projectile.Launch(direction, _projectilePoolId, _poolManager);
            }
        }
    }
}