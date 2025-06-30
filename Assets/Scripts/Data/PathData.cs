// PathData must be a struct and all fields must be value types
using Unity.Mathematics;
using Unity.Entities;

[InternalBufferCapacity(8)]
public struct PathData : IBufferElementData
{
    public float3 Waypoint;
}