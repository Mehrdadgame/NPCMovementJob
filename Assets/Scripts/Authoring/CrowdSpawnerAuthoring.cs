using CrowdSimulation;
using Unity.Entities;
using UnityEngine;

public class CrowdSpawnerAuthoring : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject AgentPrefab;
    public int SpawnCount = 50;
    public float SpawnRadius = 10.0f;

    [Header("Default Agent Settings")]
    public float DefaultMaxSpeed = 3.0f;
    public float DefaultAcceleration = 5.0f;
    public float DefaultAvoidanceRadius = 2.0f;

    [Header("Control")]
    public bool AutoSpawnOnStart = true;

    private void Start()
    {
        if (AutoSpawnOnStart)
        {
            SpawnCrowd();
        }
    }

    [ContextMenu("Spawn Crowd")]
    public void SpawnCrowd()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;

        var spawnerEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(spawnerEntity, new CrowdSpawnerComponent
        {
            AgentPrefab = entityManager.GetBuffer<LinkedEntityGroup>(GetEntity())[1].Value,
            SpawnCount = SpawnCount,
            SpawnRadius = SpawnRadius,
            ShouldSpawn = true,
            DefaultMaxSpeed = DefaultMaxSpeed,
            DefaultAcceleration = DefaultAcceleration,
            DefaultAvoidanceRadius = DefaultAvoidanceRadius
        });
    }

    private Entity GetEntity()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(CrowdSpawnerAuthoring));
        if (query.CalculateEntityCount() > 0)
            return query.GetSingletonEntity();
        return Entity.Null;
    }
}

public class CrowdSpawnerBaker : Baker<CrowdSpawnerAuthoring>
{
    public override void Bake(CrowdSpawnerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new CrowdSpawnerComponent
        {
            AgentPrefab = GetEntity(authoring.AgentPrefab, TransformUsageFlags.Dynamic),
            SpawnCount = authoring.SpawnCount,
            SpawnRadius = authoring.SpawnRadius,
            ShouldSpawn = authoring.AutoSpawnOnStart,
            DefaultMaxSpeed = authoring.DefaultMaxSpeed,
            DefaultAcceleration = authoring.DefaultAcceleration,
            DefaultAvoidanceRadius = authoring.DefaultAvoidanceRadius
        });
    }
}