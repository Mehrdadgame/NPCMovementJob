using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using CrowdSimulation.Components;

namespace CrowdSimulation.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class DebugVisualizationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Draw paths
            Entities.ForEach((in LocalTransform transform,
                             in PathFollower pathFollower,
                             in DynamicBuffer<PathData> pathData) =>
            {
                if (pathData.Length <= 1) return;

                // Draw path lines
                for (int i = 0; i < pathData.Length - 1; i++)
                {
                    Debug.DrawLine(pathData[i].Waypoint, pathData[i + 1].Waypoint, Color.yellow);
                }

                // Draw loop connection
                if (pathFollower.IsLooping && pathData.Length > 2)
                {
                    Debug.DrawLine(pathData[pathData.Length - 1].Waypoint, pathData[0].Waypoint, Color.yellow);
                }

                // Draw current target
                if (pathFollower.CurrentWaypointIndex < pathData.Length)
                {
                    var targetPos = pathData[pathFollower.CurrentWaypointIndex].Waypoint;
                    Debug.DrawLine(transform.Position, targetPos, Color.green);
                }

            }).WithoutBurst().Run();

            // Draw avoidance radius
            Entities.ForEach((in LocalTransform transform,
                             in AvoidanceData avoidanceData) =>
            {
                // Draw avoidance circle (simplified)
                var pos = transform.Position;
                var radius = avoidanceData.AvoidanceRadius;

                for (int i = 0; i < 16; i++)
                {
                    var angle1 = (i / 16f) * 2 * math.PI;
                    var angle2 = ((i + 1) / 16f) * 2 * math.PI;

                    var point1 = pos + new float3(math.cos(angle1) * radius, 0, math.sin(angle1) * radius);
                    var point2 = pos + new float3(math.cos(angle2) * radius, 0, math.sin(angle2) * radius);

                    Debug.DrawLine(point1, point2, Color.red);
                }

            }).WithoutBurst().Run();
        }
    }
}