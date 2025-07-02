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

    /// <summary>
    /// The function "SpawnCrowd" creates a spawner entity with specified components for spawning a crowd
    /// in a game.
    /// </summary>
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

    /// <summary>
    /// The GetEntity function retrieves a specific entity from the EntityManager based on a query for a
    /// specific component type.
    /// </summary>
    /// <returns>
    /// The `GetEntity` method returns an Entity object. If there is at least one entity that matches the
    /// query for `CrowdSpawnerAuthoring` component, the method returns that entity. Otherwise, it returns
    /// `Entity.Null`.
    /// </returns>
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

/* The CrowdSpawnerBaker class extends the Baker class and implements the Bake method to create a
CrowdSpawnerComponent entity based on the CrowdSpawnerAuthoring data. */
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