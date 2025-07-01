using Unity.Entities;
using UnityEngine;

public class CrowdManagerAuthoring : MonoBehaviour
{
    [Header("Global Crowd Settings")]
    public bool EnableDebugVisualization = true;
    public bool EnableObstacleAvoidance = true;
    public bool EnableFlocking = true;

    [Header("Performance Settings")]
    public int MaxCrowdSize = 500;
    public float UpdateFrequency = 60.0f;
    public bool UseJobSystem = true;

    [Header("Behavior Weights")]
    [Range(0f, 5f)] public float GlobalSeparationWeight = 1.0f;
    [Range(0f, 5f)] public float GlobalPathFollowingWeight = 1.0f;
    [Range(0f, 5f)] public float GlobalObstacleAvoidanceWeight = 2.0f;
}

public struct CrowdManagerComponent : IComponentData
{
    public bool EnableDebugVisualization;
    public bool EnableObstacleAvoidance;
    public bool EnableFlocking;
    public int MaxCrowdSize;
    public float UpdateFrequency;
    public bool UseJobSystem;
    public float GlobalSeparationWeight;
    public float GlobalPathFollowingWeight;
    public float GlobalObstacleAvoidanceWeight;
}

public class CrowdManagerBaker : Baker<CrowdManagerAuthoring>
{
    public override void Bake(CrowdManagerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new CrowdManagerComponent
        {
            EnableDebugVisualization = authoring.EnableDebugVisualization,
            EnableObstacleAvoidance = authoring.EnableObstacleAvoidance,
            EnableFlocking = authoring.EnableFlocking,
            MaxCrowdSize = authoring.MaxCrowdSize,
            UpdateFrequency = authoring.UpdateFrequency,
            UseJobSystem = authoring.UseJobSystem,
            GlobalSeparationWeight = authoring.GlobalSeparationWeight,
            GlobalPathFollowingWeight = authoring.GlobalPathFollowingWeight,
            GlobalObstacleAvoidanceWeight = authoring.GlobalObstacleAvoidanceWeight
        });
    }
}