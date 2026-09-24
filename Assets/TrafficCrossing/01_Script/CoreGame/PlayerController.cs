using System.Collections;
using UnityEngine;

namespace TrafficCrossing.CoreGame
{
    /// <summary>
    /// Handles grid-based hop movement for the Traffic Crossing prototype using mouse input.
    /// Supports forward movement on click and horizontal movement on mouse drag without backward hops.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float _gridSize = 1f;

        [Header("Hop Animation Settings")]
        [SerializeField] private float _hopDuration = 0.15f;
        [SerializeField] private float _hopHeight = 0.5f;

        [Header("Mouse Input Settings")]
        [SerializeField] private float _swipeThreshold = 50f;

        private bool _isHopping;
        private Vector3 _startMousePosition;
        private bool _hasSwiped;

        /// <summary>
        /// Gets the cell size used for grid movement.
        /// </summary>
        public float GridSize
        {
            get { return _gridSize; }
            private set { _gridSize = value; }
        }

        /// <summary>
        /// Gets whether the player is currently executing a hop animation.
        /// </summary>
        public bool IsHopping
        {
            get { return _isHopping; }
            private set { _isHopping = value; }
        }

        private void Update()
        {
            ReadMouseInput();
        }

        /// <summary>
        /// Reads left mouse button input to trigger forward movement on click or sideways movement on drag.
        /// </summary>
        private void ReadMouseInput()
        {
            if (_isHopping)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _startMousePosition = Input.mousePosition;
                _hasSwiped = false;
            }

            if (Input.GetMouseButton(0) && !_hasSwiped)
            {
                float deltaX = Input.mousePosition.x - _startMousePosition.x;

                if (Mathf.Abs(deltaX) >= _swipeThreshold)
                {
                    _hasSwiped = true;
                    Vector3 direction = deltaX > 0f ? Vector3.right : Vector3.left;
                    TryHop(direction);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!_hasSwiped)
                {
                    TryHop(Vector3.forward);
                }
            }
        }

        /// <summary>
        /// Initiates a hop toward the target direction if valid.
        /// </summary>
        private void TryHop(Vector3 direction)
        {
            Vector3 targetPosition = transform.position + direction * _gridSize;
            StartCoroutine(HopToPosition(targetPosition, direction));
        }

        /// <summary>
        /// Animates the arc movement to the target position and snaps to the grid.
        /// </summary>
        private IEnumerator HopToPosition(Vector3 targetPosition, Vector3 direction)
        {
            _isHopping = true;

            Vector3 startPosition = transform.position;
            transform.rotation = Quaternion.LookRotation(direction);

            float elapsedTime = 0f;

            while (elapsedTime < _hopDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _hopDuration;

                Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, progress);
                float hopArc = Mathf.Sin(progress * Mathf.PI) * _hopHeight;

                transform.position = new Vector3(flatPosition.x, startPosition.y + hopArc, flatPosition.z);

                yield return null;
            }

            transform.position = SnapToGrid(targetPosition);
            _isHopping = false;
        }

        /// <summary>
        /// Snaps position to exact grid coordinates to avoid floating point drift.
        /// </summary>
        private Vector3 SnapToGrid(Vector3 position)
        {
            float snappedX = Mathf.Round(position.x / _gridSize) * _gridSize;
            float snappedZ = Mathf.Round(position.z / _gridSize) * _gridSize;
            return new Vector3(snappedX, position.y, snappedZ);
        }
    }
}