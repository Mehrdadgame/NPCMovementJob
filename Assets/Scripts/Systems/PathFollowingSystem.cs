using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using CrowdSimulation.Components;

namespace CrowdSimulation.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(AvoidanceSystem))]
    public partial struct PathFollowingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PathFollower>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var pathJob = new PathFollowingJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            pathJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct PathFollowingJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref PathFollower pathFollower,
                           ref PathTarget pathTarget,
                           ref DesiredVelocity desiredVelocity,
                           in LocalTransform transform,
                           in DynamicBuffer<PathData> pathData,
                           in CrowdAgent agent)
        {
            if (pathData.Length == 0 || pathFollower.HasReachedEnd)
            {
                desiredVelocity.Value = float3.zero;
                return;
            }

            var currentPos = transform.Position;
            var targetWaypoint = pathData[pathFollower.CurrentWaypointIndex].Waypoint;

            // Calculate distance to current waypoint
            var directionToTarget = targetWaypoint - currentPos;
            directionToTarget.y = 0; // Keep movement on XZ plane
            var distanceToTarget = math.length(directionToTarget);

            // Update path target
            pathTarget.Position = targetWaypoint;
            pathTarget.Direction = math.normalize(directionToTarget);
            pathTarget.Distance = distanceToTarget;

            // Check if reached waypoint
            if (distanceToTarget <= pathFollower.ReachDistance)
            {
                pathFollower.CurrentWaypointIndex++;

                if (pathFollower.CurrentWaypointIndex >= pathData.Length)
                {
                    if (pathFollower.IsLooping)
                    {
                        pathFollower.CurrentWaypointIndex = 0;
                        pathFollower.PathProgress = 0f;
                    }
                    else
                    {
                        pathFollower.HasReachedEnd = true;
                        desiredVelocity.Value = float3.zero;
                        return;
                    }
                }

                // Update target for new waypoint
                if (pathFollower.CurrentWaypointIndex < pathData.Length)
                {
                    targetWaypoint = pathData[pathFollower.CurrentWaypointIndex].Waypoint;
                    directionToTarget = targetWaypoint - currentPos;
                    directionToTarget.y = 0;
                    pathTarget.Position = targetWaypoint;
                    pathTarget.Direction = math.normalize(directionToTarget);
                    pathTarget.Distance = math.length(directionToTarget);
                }
            }

            // Calculate desired velocity toward target
            if (pathTarget.Distance > 0.1f)
            {
                desiredVelocity.Value = pathTarget.Direction * agent.MaxSpeed;
            }
            else
            {
                desiredVelocity.Value = float3.zero;
            }

            // Update progress
            pathFollower.PathProgress = (float)pathFollower.CurrentWaypointIndex / pathData.Length;
        }
    }
}