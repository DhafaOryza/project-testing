using System;
using UnityEngine;

namespace TopDownArenaSurvival.CoreGame
{
    /// <summary>
    /// Generic health manager component suitable for both Player and Enemies.
    /// Manages health state, damage, healing, and broadcasts events on health changes or death.
    /// </summary>
    public class HealthManager : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private int _maxHealth = 100;

        private int _currentHealth;

        /// <summary>
        /// Event fired when health changes. Passes (currentHealth, maxHealth).
        /// </summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>
        /// Event fired when health reaches zero.
        /// </summary>
        public event Action OnDied;

        /// <summary>
        /// Gets or sets the maximum health value.
        /// </summary>
        public int MaxHealth
        {
            get { return _maxHealth; }
            set { _maxHealth = value; }
        }

        /// <summary>
        /// Gets the current health value.
        /// </summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>
        /// Gets whether the owner has died.
        /// </summary>
        public bool IsDead => _currentHealth <= 0;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        /// <summary>
        /// Reduces health by the given amount and invokes death event if health reaches zero.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                OnDied?.Invoke();
            }
        }

        /// <summary>
        /// Restores health by the given amount without exceeding max health.
        /// </summary>
        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>
        /// Resets current health back to maximum health. Useful for pooling or respawning.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }
}