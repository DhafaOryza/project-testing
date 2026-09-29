using UnityEngine;

namespace TopDownArenaSurvival.CoreGame
{
    /// <summary>
    /// Smoothly follows a target transform position without copying its rotation.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, 0f);
        [SerializeField] private float _smoothSpeed = 10f;

        /// <summary>
        /// Gets the target transform currently being followed.
        /// </summary>
        public Transform Target
        {
            get { return _target; }
            private set { _target = value; }
        }

        private void LateUpdate()
        {
            FollowTarget();
        }

        /// <summary>
        /// Smoothly interpolates the camera position toward the target position plus offset.
        /// </summary>
        private void FollowTarget()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 targetPosition = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
        }
    }
}