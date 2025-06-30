using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using CrowdSimulation.Components;

namespace CrowdSimulation.Authoring
{
    public class NPCAuthoring : MonoBehaviour
    {
        [Header("Agent Settings")]
        public float maxSpeed = 5f;
        public float radius = 0.5f;
        public int groupID = 0;
        public float maxForce = 2f;

        [Header("Avoidance Settings")]
        public float avoidanceRadius = 2f;
        public float separationWeight = 1.5f;
        public float alignmentWeight = 1f;
        public float cohesionWeight = 1f;

        [Header("Path Settings")]
        public float waypointReachDistance = 1.5f;
        public bool loopPath = true;
        public Transform[] waypoints;

        class NPCBaker : Baker<NPCAuthoring>
        {
            public override void Bake(NPCAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<NPCTag>(entity);

                AddComponent(entity, new CrowdAgent
                {
                    MaxSpeed = authoring.maxSpeed,
                    Radius = authoring.radius,
                    GroupID = authoring.groupID,
                    MaxForce = authoring.maxForce
                });

                AddComponent(entity, new AvoidanceData
                {
                    AvoidanceRadius = authoring.avoidanceRadius,
                    MaxAvoidanceForce = authoring.maxForce,
                    SeparationWeight = authoring.separationWeight,
                    AlignmentWeight = authoring.alignmentWeight,
                    CohesionWeight = authoring.cohesionWeight
                });

                AddComponent<Velocity>(entity);
                AddComponent<DesiredVelocity>(entity);
                AddComponent<SteeringForce>(entity);
                AddComponent<MovementState>(entity);
                AddComponent<CrowdStats>(entity);

                if (authoring.waypoints != null && authoring.waypoints.Length > 0)
                {
                    AddComponent(entity, new PathFollower
                    {
                        CurrentWaypointIndex = 0,
                        ReachDistance = authoring.waypointReachDistance,
                        IsLooping = authoring.loopPath,
                        HasReachedEnd = false,
                        PathProgress = 0f
                    });

                    AddComponent<PathTarget>(entity);

                    var pathBuffer = AddBuffer<PathData>(entity);
                    foreach (var waypoint in authoring.waypoints)
                    {
                        if (waypoint != null)
                        {
                            pathBuffer.Add(new PathData { Waypoint = waypoint.position });
                        }
                    }
                }
            }
        }
    }
}