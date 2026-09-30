using UnityEngine;
using UnityEngine.SceneManagement;

namespace EndlessPrecisionRunner.CoreGame
{    
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _forwardSpeed = 8f;
        [SerializeField] private float _jumpForce = 7f;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayer;

        private Rigidbody2D _rigidbody;
        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            CheckGrounded();
            HandleJumpInput();
        }

        private void FixedUpdate()
        {
            MoveForward();
        }

        private void MoveForward()
        {
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = _forwardSpeed;
            _rigidbody.linearVelocity = velocity;
        }

        private void HandleJumpInput()
        {
            bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);

            if (jumpPressed && _isGrounded)
            {
                Vector2 velocity = _rigidbody.linearVelocity;
                velocity.y = _jumpForce;
                _rigidbody.linearVelocity = velocity;
            }
        }

        private void CheckGrounded()
        {
            _isGrounded = Physics2D.OverlapCircle(_groundCheckPoint.position, _groundCheckRadius, _groundLayer);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                HandlePlayerDeath();
            }
        }

        private void HandlePlayerDeath()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheckPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckRadius);
            }
        }
    }
}