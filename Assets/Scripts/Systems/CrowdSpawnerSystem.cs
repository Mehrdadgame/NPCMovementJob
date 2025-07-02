using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

namespace CrowdSimulation
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    /* The `CrowdSpawnerSystem` class is responsible for spawning crowd agents at specified positions
    with randomized attributes. */
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

                    // Create random seed based on entity and time for deterministic randomness
                    var random = Unity.Mathematics.Random.CreateFromIndex(
                        (uint)(spawnerEntity.Index + (SystemAPI.Time.ElapsedTime * 1000)));

                    for (int i = 0; i < spawner.SpawnCount; i++)
                    {
                        // Generate random spawn position within radius
                        var angle = random.NextFloat(0, 2 * math.PI);
                        var distance = random.NextFloat(0, spawner.SpawnRadius);

                        var spawnPosition = transform.Position + new float3(
                            math.cos(angle) * distance,
                            0,
                            math.sin(angle) * distance
                        );

                        // Instantiate agent
                        var agentEntity = ecb.Instantiate(spawner.AgentPrefab);

                        // Set transform
                        ecb.SetComponent(agentEntity, LocalTransform.FromPosition(spawnPosition));

                        // Set crowd agent component
                        ecb.SetComponent(agentEntity, new CrowdAgentComponent
                        {
                            MaxSpeed = spawner.DefaultMaxSpeed + random.NextFloat(-0.5f, 0.5f), // Add some variation
                            AccelerationForce = spawner.DefaultAcceleration,
                            AvoidanceRadius = spawner.DefaultAvoidanceRadius + random.NextFloat(-0.2f, 0.2f),
                            SeparationWeight = 1.0f,
                            PathFollowingWeight = 1.0f,
                            CurrentWaypointIndex = 0,
                            HasReachedDestination = false
                        });

                        // Set movement component
                        ecb.SetComponent(agentEntity, new MovementComponent
                        {
                            MaxForce = spawner.DefaultAcceleration,
                            Velocity = float3.zero,
                            DesiredVelocity = float3.zero,
                            SteeringForce = float3.zero
                        });

                        // Set avoidance component
                        ecb.SetComponent(agentEntity, new AvoidanceComponent
                        {
                            AvoidanceRadius = spawner.DefaultAvoidanceRadius,
                            AvoidanceForce = 0f,
                            AvoidanceDirection = float3.zero,
                            NeighborCount = 0
                        });

                        // Optionally add debug component
                        if (i < 10) // Only add debug to first 10 agents for performance
                        {
                            ecb.AddComponent(agentEntity, new CrowdDebugComponent
                            {
                                ShowAvoidanceRadius = true,
                                ShowVelocity = true,
                                ShowPath = true,
                                ShowSteeringForce = false,
                                ShowNeighbors = false
                            });
                        }
                    }

                    // Mark as spawned
                    spawner.ShouldSpawn = false;

                    Debug.Log($"Spawned {spawner.SpawnCount} crowd agents at position {transform.Position}");

                }).Schedule();

            m_ECBSystem.AddJobHandleForProducer(Dependency);
        }
    }

    // Component definition moved to separate file to avoid duplication
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
}