using Unity.Entities;
using Unity.Mathematics;

namespace CrowdSimulation.Components
{
    public struct Velocity : IComponentData
    {
        public float3 Value;
    }

    public struct DesiredVelocity : IComponentData
    {
        public float3 Value;
    }

    public struct SteeringForce : IComponentData
    {
        public float3 Seek;
        public float3 Separation;
        public float3 Alignment;
        public float3 Cohesion;
        public float3 Obstacle;
        public float3 Final;
    }

    public struct MovementState : IComponentData
    {
        public bool IsMoving;
        public bool IsBlocked;
        public float StuckTimer;
        public float3 LastPosition;
    }
}