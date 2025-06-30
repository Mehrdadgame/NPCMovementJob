using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using CrowdSimulation.Data;

namespace CrowdSimulation.Authoring
{
    public struct CrowdSpawner : IComponentData
    {
        public Entity NPCPrefab;
        public int MaxCount;
        public float SpawnRadius;
        public float SpawnInterval;
        public float NextSpawnTime;
        public int CurrentCount;
    }

    public struct SpawnPoint : IBufferElementData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    public class CrowdSpawnerAuthoring : MonoBehaviour
    {
        [Header("Spawn Settings")]
        public GameObject npcPrefab;
        public int maxNPCs = 100;
        public float spawnRadius = 5f;
        public float spawnInterval = 0.1f;

        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        [Header("Configuration")]
        public CrowdConfig crowdConfig;

        class CrowdSpawnerBaker : Baker<CrowdSpawnerAuthoring>
        {
            public override void Bake(CrowdSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new CrowdSpawner
                {
                    NPCPrefab = GetEntity(authoring.npcPrefab, TransformUsageFlags.Dynamic),
                    MaxCount = authoring.maxNPCs,
                    SpawnRadius = authoring.spawnRadius,
                    SpawnInterval = authoring.spawnInterval,
                    NextSpawnTime = 0f,
                    CurrentCount = 0
                });

                if (authoring.spawnPoints != null && authoring.spawnPoints.Length > 0)
                {
                    var spawnBuffer = AddBuffer<SpawnPoint>(entity);
                    foreach (var point in authoring.spawnPoints)
                    {
                        if (point != null)
                        {
                            spawnBuffer.Add(new SpawnPoint
                            {
                                Position = point.position,
                                Rotation = point.rotation
                            });
                        }
                    }
                }
            }
        }
    }
}