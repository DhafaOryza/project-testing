using UnityEngine;

namespace TopDownArenaSurvival.Enemy.CoreGame
{
    /// <summary>
    /// Generic health component that can be attached to the player or any enemy.
    /// Handles taking damage and reports when the owner has died.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int _maxHealth = 100;

        private int _currentHealth;

        /// <summary>
        /// Gets the current health value.
        /// </summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>
        /// Gets whether this owner has run out of health.
        /// </summary>
        public bool IsDead => _currentHealth <= 0;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        /// <summary>
        /// Reduces current health by the given amount, and handles death once it reaches zero.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (IsDead)
            {
                return;
            }

            _currentHealth -= amount;

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                HandleDeath();
            }
        }

        /// <summary>
        /// Handles what happens once health reaches zero. For now it just destroys the GameObject.
        /// </summary>
        private void HandleDeath()
        {
            // TODO: replace with pooling despawn / death VFX / score once those systems exist
            Destroy(gameObject);
        }
    }
}
