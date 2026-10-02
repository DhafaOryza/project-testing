using System;
using UnityEngine;
using Assets.PolyRef;

namespace TrafficCrossing.CoreGame.Serializeble
{
    [Serializable]
    public class ObstaclePlacement
    {
        [Tooltip("Grid row offset from the start of this terrain chunk (0 = the chunk's first row).")]
        [SerializeField] private int _gridOffset;

        [SerializeReference, SubclassSelector]
        private BaseObstacleConfig _obstacleConfig;

        public int GridOffset => _gridOffset;
        public BaseObstacleConfig ObstacleConfig => _obstacleConfig;
    }
}