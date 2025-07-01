using Unity.Entities;

namespace CrowdSimulation
{
    public struct CrowdDebugComponent : IComponentData
    {
        public bool ShowAvoidanceRadius;
        public bool ShowVelocity;
        public bool ShowPath;
        public bool ShowSteeringForce;
        public bool ShowNeighbors;
    }
}