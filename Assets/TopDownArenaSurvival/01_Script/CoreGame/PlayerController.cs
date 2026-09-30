using UnityEngine;

namespace TopDownArenaSurvival.CoreGame
{    
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;

        private Rigidbody2D _rigidbody;
        private Camera _mainCamera;
        private Vector2 _moveInput;
        private HealthManager _healthManager;

        public Vector2 MoveInput => _moveInput;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _rigidbody = GetComponent<Rigidbody2D>();
            _healthManager = GetComponent<HealthManager>();
        }

        private void OnEnable()
        {
            if (_healthManager != null)
            {
                _healthManager.OnHealthChanged += UpdateHealthUI;
                _healthManager.OnDied += HandlePlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (_healthManager != null)
            {
                _healthManager.OnHealthChanged -= UpdateHealthUI;
                _healthManager.OnDied -= HandlePlayerDeath;
            }
        }

        private void UpdateHealthUI(int current, int max)
        {
            Debug.Log($"Player Health: {current}/{max}");
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("Player Died!");
        }

        private void Update()
        {
            ReadMoveInput();
            RotateToMouse();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void ReadMoveInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            _moveInput = new Vector2(horizontal, vertical).normalized;
        }

        private void RotateToMouse()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 lookDirection = mouseWorldPos - transform.position;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        private void MovePlayer()
        {
            _rigidbody.linearVelocity = _moveInput * _moveSpeed;
        }
    }
}