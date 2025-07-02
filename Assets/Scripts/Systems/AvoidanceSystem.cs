using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

/* These attributes are used to specify the update order of the `AvoidanceSystem` in relation to other
systems within the ECS framework. */
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(MovementSystem))]
[UpdateAfter(typeof(PathFollowingSystem))]
public partial class AvoidanceSystem : SystemBase
{
    private EntityQuery m_AgentQuery;

    /// <summary>
    /// The OnCreate function in C# is used to set up an EntityQuery with specified component types.
    /// </summary>
    protected override void OnCreate()
    {
        m_AgentQuery = GetEntityQuery(ComponentType.ReadOnly<LocalTransform>(),
                                     ComponentType.ReadOnly<CrowdAgentComponent>());
    }

    /// <summary>
    /// The function OnUpdate calculates the number of agents, retrieves their positions and components,
    /// then schedules a job for avoidance calculations.
    /// </summary>
    /// <returns>
    /// If the `agentCount` variable is equal to 0, the method will return early and not execute the
    /// rest of the code within the `OnUpdate` method.
    /// </returns>
    protected override void OnUpdate()
    {
        var agentCount = m_AgentQuery.CalculateEntityCount();
        if (agentCount == 0) return;

        var positions = m_AgentQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var agents = m_AgentQuery.ToComponentDataArray<CrowdAgentComponent>(Allocator.TempJob);

        var avoidanceJob = new AvoidanceJob
        {
            AllPositions = positions,
            AllAgents = agents,
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        Dependency = avoidanceJob.ScheduleParallel(Dependency);

        positions.Dispose(Dependency);
        agents.Dispose(Dependency);
    }
}

[BurstCompile]
/* The `AvoidanceJob` struct in the provided code snippet is a job that implements the `IJobEntity`
interface in Unity's ECS framework. This job is responsible for calculating avoidance behavior for
entities in the simulation. */
public partial struct AvoidanceJob : IJobEntity
{
    [ReadOnly] public NativeArray<LocalTransform> AllPositions;
    [ReadOnly] public NativeArray<CrowdAgentComponent> AllAgents;
    public float DeltaTime;

    public void Execute(
        [EntityIndexInQuery] int entityIndex,
        ref MovementComponent movement,
        ref AvoidanceComponent avoidance,
        in LocalTransform transform,
        in CrowdAgentComponent agent)
    {
        float3 separationForce = float3.zero;
        int neighborCount = 0;

        for (int i = 0; i < AllPositions.Length; i++)
        {
            if (i == entityIndex) continue;

            var otherPosition = AllPositions[i].Position;
            var direction = transform.Position - otherPosition;
            var distance = math.length(direction);

            if (distance < agent.AvoidanceRadius && distance > 0.1f)
            {
                var normalizedDirection = math.normalize(direction);
                var force = normalizedDirection / distance; // Closer agents have stronger repulsion
                separationForce += force;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            separationForce = math.normalize(separationForce) * agent.MaxSpeed;
            var steeringForce = (separationForce - movement.Velocity) * agent.SeparationWeight;
            movement.SteeringForce += steeringForce;
        }

        avoidance.NeighborCount = neighborCount;
        avoidance.AvoidanceDirection = separationForce;
    }
}