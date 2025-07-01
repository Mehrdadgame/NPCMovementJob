using Unity.Entities;
using Unity.Mathematics;

public struct MovementComponent : IComponentData
{
    public float3 Velocity;
    public float3 DesiredVelocity;
    public float3 SteeringForce;
    public float MaxForce;
}