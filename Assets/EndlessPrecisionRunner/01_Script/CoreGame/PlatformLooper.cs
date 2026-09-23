using UnityEngine;

namespace EndlessPrecisionRunner.CoreGame
{    
    /// <summary>
    /// Recycles a fixed set of ground platform segments to create an endless
    /// scrolling track. Instead of destroying segments, they get moved to the
    /// front of the track once the player has passed far enough beyond them.
    /// </summary>
    public class PlatformLooper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _player;
        [SerializeField] private Transform[] _platformSegments;

        [Header("Settings")]
        [SerializeField] private float _segmentLength = 10f;
        [SerializeField] private float _recycleDistanceBehindPlayer = 15f;

        /// <summary>
        /// Gets the total track length covered by all platform segments combined.
        /// </summary>
        public float TotalTrackLength => _segmentLength * _platformSegments.Length;

        private void Update()
        {
            RecycleSegmentsBehindPlayer();
        }

        /// <summary>
        /// Checks every platform segment and moves it to the front of the track
        /// once the player has moved far enough past it.
        /// </summary>
        private void RecycleSegmentsBehindPlayer()
        {
            foreach (Transform segment in _platformSegments)
            {
                float distanceBehindPlayer = _player.position.x - segment.position.x;

                if (distanceBehindPlayer > _recycleDistanceBehindPlayer)
                {
                    MoveSegmentToFront(segment);
                }
            }
        }

        /// <summary>
        /// Moves a single platform segment so it becomes the new furthest segment ahead.
        /// </summary>
        private void MoveSegmentToFront(Transform segment)
        {
            float furthestX = GetFurthestSegmentX();

            Vector3 newPosition = segment.position;
            newPosition.x = furthestX + _segmentLength;
            segment.position = newPosition;
        }

        /// <summary>
        /// Finds the X position of whichever platform segment is currently furthest ahead.
        /// </summary>
        private float GetFurthestSegmentX()
        {
            float furthestX = float.MinValue;

            foreach (Transform segment in _platformSegments)
            {
                if (segment.position.x > furthestX)
                {
                    furthestX = segment.position.x;
                }
            }

            return furthestX;
        }
    }
}