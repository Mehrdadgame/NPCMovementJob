using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(MovementSystem))]
[UpdateAfter(typeof(PathFollowingSystem))]
public partial class AvoidanceSystem : SystemBase
{
    private EntityQuery m_AgentQuery;

    protected override void OnCreate()
    {
        m_AgentQuery = GetEntityQuery(ComponentType.ReadOnly<LocalTransform>(),
                                     ComponentType.ReadOnly<CrowdAgentComponent>());
    }

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