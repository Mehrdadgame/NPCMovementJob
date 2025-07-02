using Unity.Entities;
using UnityEngine;

/* The CrowdManagerAuthoring class in C# contains properties for global crowd settings, performance
settings, and behavior weights for managing crowds in a game. */
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

/* The `CrowdManagerComponent` struct in the provided C# code is implementing the `IComponentData`
interface. This indicates that it is intended to be used as a component data structure in the Unity
ECS (Entity Component System) framework. */
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

/* The CrowdManagerBaker class extends the Baker class and implements the Bake method to create a
CrowdManagerComponent entity based on the CrowdManagerAuthoring data. */
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