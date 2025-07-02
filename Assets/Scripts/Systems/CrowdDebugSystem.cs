using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CrowdSimulation
{
    /* The CrowdDebugSystem class in C# provides debug visualization for crowd agents, paths, and obstacles
    in a game or simulation environment. */
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class CrowdDebugSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!Application.isPlaying) return;

            // Debug visualization for crowd agents
            Entities
                .WithAll<CrowdDebugComponent>()
                .ForEach((Entity entity, in LocalTransform transform,
                         in CrowdAgentComponent agent, in MovementComponent movement,
                         in CrowdDebugComponent debug) =>
                {
                    var position = transform.Position;

                    // Show avoidance radius
                    if (debug.ShowAvoidanceRadius)
                    {
                        DebugExtensions.DrawWireSphere(position, agent.AvoidanceRadius, Color.red, 0.1f);
                    }

                    // Show velocity
                    if (debug.ShowVelocity && math.length(movement.Velocity) > 0.1f)
                    {
                        Debug.DrawLine(position, position + movement.Velocity, Color.blue, 0.1f);
                    }

                    // Show steering force
                    if (debug.ShowSteeringForce && math.length(movement.SteeringForce) > 0.1f)
                    {
                        Debug.DrawLine(position, position + movement.SteeringForce, Color.yellow, 0.1f);
                    }

                }).WithoutBurst().Run();

            // Show paths
            Entities
                .WithAll<PathFollowingComponent, CrowdDebugComponent>()
                .ForEach((Entity entity, in LocalTransform transform,
                         in PathFollowingComponent pathFollowing,
                         in CrowdDebugComponent debug) =>
                {
                    if (!debug.ShowPath || !pathFollowing.PathBlob.IsCreated) return;

                    ref var waypoints = ref pathFollowing.PathBlob.Value.Waypoints;
                    var position = transform.Position;

                    // Draw path
                    for (int i = 0; i < waypoints.Length - 1; i++)
                    {
                        Debug.DrawLine(waypoints[i], waypoints[i + 1], Color.green, 0.1f);
                    }

                    // Draw line to current target
                    if (pathFollowing.CurrentPathIndex < waypoints.Length)
                    {
                        Debug.DrawLine(position, waypoints[pathFollowing.CurrentPathIndex], Color.cyan, 0.1f);
                    }

                    // Draw waypoint spheres
                    for (int i = 0; i < waypoints.Length; i++)
                    {
                        var color = i == pathFollowing.CurrentPathIndex ? Color.yellow : Color.green;
                        DebugExtensions.DrawWireSphere(waypoints[i], pathFollowing.WaypointRadius, color, 0.1f);
                    }

                }).WithoutBurst().Run();

            // Show obstacles
            Entities
                .WithAll<ObstacleComponent>()
                .ForEach((Entity entity, in LocalTransform transform, in ObstacleComponent obstacle) =>
                {
                    DebugExtensions.DrawWireSphere(transform.Position, obstacle.Radius, Color.red, 0.1f);

                    if (obstacle.Height > 0)
                    {
                        Debug.DrawLine(transform.Position,
                                     transform.Position + new float3(0, obstacle.Height, 0),
                                     Color.red, 0.1f);
                    }

                }).WithoutBurst().Run();
        }
    }

    /* The DebugExtensions class in C# provides a method to draw a wire sphere using Debug.DrawLine for
    visualization purposes. */
    public static class DebugExtensions
    {
        public static void DrawWireSphere(Vector3 center, float radius, Color color, float duration)
        {
            var segments = 16;
            var angleStep = 2 * Mathf.PI / segments;

            // Draw horizontal circle
            for (int i = 0; i < segments; i++)
            {
                var angle1 = i * angleStep;
                var angle2 = (i + 1) * angleStep;

                var point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0, Mathf.Sin(angle1) * radius);
                var point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0, Mathf.Sin(angle2) * radius);

                Debug.DrawLine(point1, point2, color, duration);
            }

            // Draw vertical circles
            for (int i = 0; i < segments; i++)
            {
                var angle1 = i * angleStep;
                var angle2 = (i + 1) * angleStep;

                var point1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0);
                var point2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0);

                Debug.DrawLine(point1, point2, color, duration);

                // Second vertical circle
                point1 = center + new Vector3(0, Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius);
                point2 = center + new Vector3(0, Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius);

                Debug.DrawLine(point1, point2, color, duration);
            }
        }
    }
}