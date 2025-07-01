using Unity.Entities;
using Unity.Mathematics;

public struct AvoidanceComponent : IComponentData
{
    public float AvoidanceRadius;
    public float AvoidanceForce;
    public float3 AvoidanceDirection;
    public int NeighborCount;
}