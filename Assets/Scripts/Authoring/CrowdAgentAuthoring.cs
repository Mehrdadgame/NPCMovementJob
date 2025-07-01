using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CrowdAgentAuthoring : MonoBehaviour
{
    [Header("Movement Settings")]
    public float MaxSpeed = 3.0f;
    public float AccelerationForce = 5.0f;

    [Header("Avoidance Settings")]
    public float AvoidanceRadius = 2.0f;
    public float SeparationWeight = 1.0f;
    public float PathFollowingWeight = 1.0f;

    [Header("Path Settings")]
    public Transform[] Waypoints;
    public float WaypointRadius = 1.0f;
    public bool IsLooping = true;
}

public class CrowdAgentBaker : Baker<CrowdAgentAuthoring>
{
    public override void Bake(CrowdAgentAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new CrowdAgentComponent
        {
            MaxSpeed = authoring.MaxSpeed,
            AccelerationForce = authoring.AccelerationForce,
            AvoidanceRadius = authoring.AvoidanceRadius,
            SeparationWeight = authoring.SeparationWeight,
            PathFollowingWeight = authoring.PathFollowingWeight,
            CurrentWaypointIndex = 0,
            HasReachedDestination = false
        });

        AddComponent(entity, new MovementComponent
        {
            MaxForce = authoring.AccelerationForce
        });

        AddComponent(entity, new AvoidanceComponent
        {
            AvoidanceRadius = authoring.AvoidanceRadius
        });

        // Create path blob
        if (authoring.Waypoints != null && authoring.Waypoints.Length > 0)
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var pathData = ref builder.ConstructRoot<PathData>();

            var waypointsArray = builder.Allocate(ref pathData.Waypoints, authoring.Waypoints.Length);

            for (int i = 0; i < authoring.Waypoints.Length; i++)
            {
                waypointsArray[i] = authoring.Waypoints[i].position;
            }

            var pathBlob = builder.CreateBlobAssetReference<PathData>(Allocator.Persistent);

            AddComponent(entity, new PathFollowingComponent
            {
                PathBlob = pathBlob,
                CurrentPathIndex = 0,
                WaypointRadius = authoring.WaypointRadius,
                IsLooping = authoring.IsLooping
            });
        }
    }
}