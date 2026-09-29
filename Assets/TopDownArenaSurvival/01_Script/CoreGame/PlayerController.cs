using UnityEngine;

namespace TopDownArenaSurvival.CoreGame
{    
    /// <summary>
    /// Handles free 8-directional movement and mouse aiming for the Top-Down Arena Survival prototype.
    /// Moves and rotates the player on the X-Z plane to match the top-down perspective.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;

        private Rigidbody _rigidbody;
        private Camera _mainCamera;
        private Vector3 _moveInput;
        private HealthManager _healthManager;

        /// <summary>
        /// Gets the current movement direction the player is holding, as a normalized vector.
        /// </summary>
        public Vector3 MoveInput
        {
            get { return _moveInput; }
            private set { _moveInput = value; }
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
            _rigidbody = GetComponent<Rigidbody>();
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

        /// <summary>
        /// Callback for updating Player Health UI when health changes.
        /// </summary>
        private void UpdateHealthUI(int current, int max)
        {
            // Logika update UI Darah Player (Slider/Text)
            Debug.Log($"Player Health: {current}/{max}");
        }

        /// <summary>
        /// Callback when player dies.
        /// </summary>
        private void HandlePlayerDeath()
        {
            Debug.Log("Player Died!");
            // Logika Game Over
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

            _moveInput = new Vector3(horizontal, 0f, vertical).normalized;
        }

        private void RotateToMouse()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return;
                }
            }

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float entryDistance))
            {
                Vector3 targetPoint = ray.GetPoint(entryDistance);
                Vector3 lookDirection = targetPoint - transform.position;
                lookDirection.y = 0f;

                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }

        private void MovePlayer()
        {
            Vector3 targetVelocity = _moveInput * _moveSpeed;
            _rigidbody.linearVelocity = new Vector3(targetVelocity.x, _rigidbody.linearVelocity.y, targetVelocity.z);
        }
    }
}