using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;
using TrafficCrossing.CoreGame.Obstacle;
using TrafficCrossing.CoreGame;

namespace TrafficCrossing.CoreGame.Serializeble
{
    [Serializable]
    public class VehicleObstacleConfig : BaseObstacleConfig
    {
        [SerializeField] private PoolIdSO _obstaclePoolId;
        [SerializeField] private Vector3 _moveDirection = Vector3.right;
        [SerializeField] private float _speed = 5f;

        [Header("Vehicle Spawn Position Settings")]
        [SerializeField] private float _spawnXOffset = 10f;

        [Header("Vehicle Distance & Grid Settings")]
        [SerializeField] private float _minVehicleDistance = 3f;
        [SerializeField] private float _maxVehicleDistance = 5f;
        [SerializeField] private float _gridSize = 1f;

        public PoolIdSO ObstaclePoolId => _obstaclePoolId;
        public Vector3 MoveDirection => _moveDirection;
        public float Speed => _speed;
        public float SpawnXOffset => _spawnXOffset;
        public float MinVehicleDistance => _minVehicleDistance;
        public float MaxVehicleDistance => _maxVehicleDistance;
        public float GridSize => _gridSize;

        public override void SpawnObstacle(
            PoolManager poolManager, 
            Vector3 position, 
            GameObject chunkInstance, 
            int gridOffset, 
            List<TerrainSpawner.SpawnedObstacle> spawnedObstacles)
        {
            if (poolManager == null || _obstaclePoolId == null) return;

            GameObject spawnerObject = new GameObject($"VehicleSpawner_Row{gridOffset}");
            spawnerObject.transform.SetParent(chunkInstance.transform);
            spawnerObject.transform.position = position;

            VehicleSpawner vehicleSpawner = spawnerObject.AddComponent<VehicleSpawner>();
            vehicleSpawner.Initialize(poolManager, this);

            // PoolId null karena ini object anchor logic-only
            spawnedObstacles.Add(new TerrainSpawner.SpawnedObstacle { Instance = spawnerObject, PoolId = null });
        }
    }
}