using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;

namespace TrafficCrossing.CoreGame.Struct
{
    public struct ActiveChunkData
    {
        public GameObject Instance;
        public PoolIdSO PoolId;
        public float YPosition;
        public float TotalLength;
        public List<SpawnedObstacle> Obstacles;

        public ActiveChunkData(GameObject instance, PoolIdSO poolId, float yPosition, float totalLength, List<SpawnedObstacle> obstacles)
        {
            Instance = instance;
            PoolId = poolId;
            YPosition = yPosition;
            Obstacles = obstacles;
            TotalLength = totalLength;
        }
    }
}