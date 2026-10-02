using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;

namespace TrafficCrossing.CoreGame.Serializeble
{
    [Serializable]
    public class TerrainChunkConfig
    {
        [Header("Platform Settings")]
        [SerializeField] private PoolIdSO _poolId;

        [Tooltip("How many grid cells long this terrain chunk is (e.g. 30 means it spans 30 rows).")]
        [SerializeField] private int _gridCount = 1;

        [Tooltip("World-space size of one grid cell. Should match the player's hop grid size.")]
        [SerializeField] private float _gridSize = 1f;

        [SerializeField] private bool _isSafeStartingTerrain;

        [Header("Obstacle Placements")]
        [SerializeField] private List<ObstaclePlacement> _obstaclePlacements = new List<ObstaclePlacement>();

        public PoolIdSO PoolId => _poolId;
        public int GridCount => _gridCount;
        public float GridSize => _gridSize;
        public float TotalLength => _gridCount * _gridSize;
        public bool IsSafeStartingTerrain => _isSafeStartingTerrain;
        public List<ObstaclePlacement> ObstaclePlacements => _obstaclePlacements;
    }
}