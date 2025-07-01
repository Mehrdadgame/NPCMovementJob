using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(MovementSystem))]
public partial class PathFollowingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        var pathFollowingJob = new PathFollowingJob
        {
            DeltaTime = deltaTime
        };

        Dependency = pathFollowingJob.ScheduleParallel(Dependency);
    }
}

[BurstCompile]
public partial struct PathFollowingJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(
        ref MovementComponent movement,
        ref PathFollowingComponent pathFollowing,
        ref CrowdAgentComponent agent,
        in LocalTransform transform)
    {
        if (!pathFollowing.PathBlob.IsCreated || pathFollowing.PathBlob.Value.Waypoints.Length == 0)
            return;

        ref var waypoints = ref pathFollowing.PathBlob.Value.Waypoints;

        if (pathFollowing.CurrentPathIndex >= waypoints.Length)
        {
            if (pathFollowing.IsLooping)
            {
                pathFollowing.CurrentPathIndex = 0;
            }
            else
            {
                agent.HasReachedDestination = true;
                return;
            }
        }

        var targetWaypoint = waypoints[pathFollowing.CurrentPathIndex];
        var currentPosition = transform.Position;
        var direction = targetWaypoint - currentPosition;
        var distanceToWaypoint = math.length(direction);
        direction.y = 0;
        if (distanceToWaypoint < pathFollowing.WaypointRadius)
        {
            pathFollowing.CurrentPathIndex++;
            return;
        }

        var desiredVelocity = math.normalize(direction) * agent.MaxSpeed;
        var steeringForce = (desiredVelocity - movement.Velocity) * agent.PathFollowingWeight;

        movement.SteeringForce += steeringForce;
    }
}