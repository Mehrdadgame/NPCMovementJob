using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

/* The ObstacleAvoidanceSystem class in C# is responsible for avoiding obstacles in a simulation,
updating before the MovementSystem and after the AvoidanceSystem. */
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(MovementSystem))]
[UpdateAfter(typeof(AvoidanceSystem))]
public partial class ObstacleAvoidanceSystem : SystemBase
{
    private EntityQuery m_ObstacleQuery;

    protected override void OnCreate()
    {
        m_ObstacleQuery = GetEntityQuery(ComponentType.ReadOnly<LocalTransform>(),
                                        ComponentType.ReadOnly<ObstacleComponent>());
    }

    protected override void OnUpdate()
    {
        var obstacleCount = m_ObstacleQuery.CalculateEntityCount();
        if (obstacleCount == 0) return;

        var obstacleTransforms = m_ObstacleQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        var obstacles = m_ObstacleQuery.ToComponentDataArray<ObstacleComponent>(Allocator.TempJob);

        var obstacleAvoidanceJob = new ObstacleAvoidanceJob
        {
            ObstaclePositions = obstacleTransforms,
            Obstacles = obstacles,
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        Dependency = obstacleAvoidanceJob.ScheduleParallel(Dependency);

        obstacleTransforms.Dispose(Dependency);
        obstacles.Dispose(Dependency);
    }
}

/* The `[BurstCompile]` attribute in C# is used to instruct the Burst compiler to optimize the
specified method or struct for performance. In this case, the `ObstacleAvoidanceJob` struct is
marked with `[BurstCompile]`, indicating that the Burst compiler should optimize the `Execute`
method within this struct for better performance when running on supported platforms. */
[BurstCompile]
public partial struct ObstacleAvoidanceJob : IJobEntity
{
    [ReadOnly] public NativeArray<LocalTransform> ObstaclePositions;
    [ReadOnly] public NativeArray<ObstacleComponent> Obstacles;
    public float DeltaTime;

    public void Execute(
        ref MovementComponent movement,
        in LocalTransform transform,
        in CrowdAgentComponent agent)
    {
        float3 avoidanceForce = float3.zero;

        for (int i = 0; i < ObstaclePositions.Length; i++)
        {
            var obstaclePos = ObstaclePositions[i].Position;
            var obstacle = Obstacles[i];

            var direction = transform.Position - obstaclePos;
            var distance = math.length(direction);
            var avoidanceRadius = obstacle.Radius + agent.AvoidanceRadius;

            if (distance < avoidanceRadius && distance > 0.1f)
            {
                var normalizedDirection = math.normalize(direction);
                var force = normalizedDirection * (avoidanceRadius - distance) / avoidanceRadius;
                avoidanceForce += force * 2.0f; // Stronger force for obstacles
            }
        }

        if (math.length(avoidanceForce) > 0.1f)
        {
            movement.SteeringForce += avoidanceForce;
        }
    }
}