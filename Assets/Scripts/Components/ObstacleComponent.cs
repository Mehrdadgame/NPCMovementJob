using Unity.Entities;
using Unity.Mathematics;

public struct ObstacleComponent : IComponentData
{
    public float Radius;
    public float Height;
    public bool IsStatic;
}

public struct ObstacleAvoidanceComponent : IComponentData
{
    public float AvoidanceRadius;
    public float AvoidanceForce;
    public float3 AvoidanceDirection;
}