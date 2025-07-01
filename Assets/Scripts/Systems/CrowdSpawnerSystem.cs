using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class CrowdSpawnerSystem : SystemBase
{
    private EntityCommandBufferSystem m_ECBSystem;

    protected override void OnCreate()
    {
        m_ECBSystem = World.GetOrCreateSystemManaged<BeginInitializationEntityCommandBufferSystem>();
        RequireForUpdate<CrowdSpawnerComponent>();
    }

    protected override void OnUpdate()
    {
        var ecb = m_ECBSystem.CreateCommandBuffer();

        Entities
            .WithAll<CrowdSpawnerComponent>()
            .ForEach((Entity spawnerEntity, ref CrowdSpawnerComponent spawner, in LocalTransform transform) =>
            {
                if (!spawner.ShouldSpawn) return;

                var random = Unity.Mathematics.Random.CreateFromIndex((uint)(spawnerEntity.Index + SystemAPI.Time.ElapsedTime));

                for (int i = 0; i < spawner.SpawnCount; i++)
                {
                    var spawnPosition = transform.Position +
                        new float3(
                            random.NextFloat(-spawner.SpawnRadius, spawner.SpawnRadius),
                            0,
                            random.NextFloat(-spawner.SpawnRadius, spawner.SpawnRadius)
                        );

                    var agentEntity = ecb.Instantiate(spawner.AgentPrefab);

                    ecb.SetComponent(agentEntity, LocalTransform.FromPosition(spawnPosition));
                    ecb.SetComponent(agentEntity, new CrowdAgentComponent
                    {
                        MaxSpeed = spawner.DefaultMaxSpeed,
                        AccelerationForce = spawner.DefaultAcceleration,
                        AvoidanceRadius = spawner.DefaultAvoidanceRadius,
                        SeparationWeight = 1.0f,
                        PathFollowingWeight = 1.0f,
                        CurrentWaypointIndex = 0,
                        HasReachedDestination = false
                    });

                    ecb.SetComponent(agentEntity, new MovementComponent
                    {
                        MaxForce = spawner.DefaultAcceleration
                    });
                }

                spawner.ShouldSpawn = false;
            }).Schedule();

        m_ECBSystem.AddJobHandleForProducer(Dependency);
    }
}

public struct CrowdSpawnerComponent : IComponentData
{
    public Entity AgentPrefab;
    public int SpawnCount;
    public float SpawnRadius;
    public bool ShouldSpawn;
    public float DefaultMaxSpeed;
    public float DefaultAcceleration;
    public float DefaultAvoidanceRadius;
}