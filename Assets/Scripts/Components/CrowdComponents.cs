using Unity.Entities;
using Unity.Mathematics;

namespace CrowdSimulation.Components
{
    public struct NPCTag : IComponentData { }

    public struct CrowdAgent : IComponentData
    {
        public float MaxSpeed;
        public float Radius;
        public int GroupID;
        public float MaxForce;
    }

    public struct AvoidanceData : IComponentData
    {
        public float AvoidanceRadius;
        public float MaxAvoidanceForce;
        public float SeparationWeight;
        public float AlignmentWeight;
        public float CohesionWeight;
    }

    public struct CrowdStats : IComponentData
    {
        public float DistanceTraveled;
        public float TimeAlive;
        public int WaypointsReached;
    }
}