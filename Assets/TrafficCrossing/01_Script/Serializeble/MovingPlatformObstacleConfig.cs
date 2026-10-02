using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;
using TrafficCrossing.CoreGame.Obstacle;
using TrafficCrossing.CoreGame.Struct;

namespace TrafficCrossing.CoreGame.Serializeble
{
    [Serializable]
    public class MovingPlatformObstacleConfig : BaseObstacleConfig
    {
        [SerializeField] private PoolIdSO _obstaclePoolId;
        [SerializeField] private PoolIdSO _waterPoolId;
        [SerializeField] private float _speed = 5f;

        [Header("Moving Platform Bounds")]
        [SerializeField] private float _leftBound = -5f;
        [SerializeField] private float _rightBound = 5f;
        [SerializeField] private SpawnSide _spawnSide = SpawnSide.Left;

        public PoolIdSO ObstaclePoolId => _obstaclePoolId;
        public PoolIdSO WaterPoolId => _waterPoolId;
        public float Speed => _speed;
        public float LeftBound => _leftBound;
        public float RightBound => _rightBound;
        public SpawnSide SpawnSide => _spawnSide;

        public override void SpawnObstacle(
            PoolManager poolManager, 
            Vector3 position, 
            GameObject chunkInstance, 
            int gridOffset, 
            List<SpawnedObstacle> spawnedObstacles)
        {
            if (poolManager == null) return;

            // 1. Spawn Water jika ada
            if (_waterPoolId != null)
            {
                GameObject waterInstance = poolManager.Spawn(_waterPoolId, position, Quaternion.identity);
                if (waterInstance != null)
                {
                    spawnedObstacles.Add(new SpawnedObstacle { Instance = waterInstance, PoolId = _waterPoolId });
                }
            }

            // 2. Spawn Moving Platform
            if (_obstaclePoolId != null)
            {
                GameObject obstacleInstance = poolManager.Spawn(_obstaclePoolId, position, Quaternion.identity);

                if (obstacleInstance != null)
                {
                    if (obstacleInstance.TryGetComponent(out MovingPlatform movingPlatform))
                    {
                        movingPlatform.MoveSpeed = _speed;
                        movingPlatform.LeftBound = _leftBound;
                        movingPlatform.RightBound = _rightBound;
                        movingPlatform.SpawnSide = _spawnSide;
                    }

                    spawnedObstacles.Add(new SpawnedObstacle { Instance = obstacleInstance, PoolId = _obstaclePoolId });
                }
            }
        }
    }
}