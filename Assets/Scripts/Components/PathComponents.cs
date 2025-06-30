using Unity.Entities;
using Unity.Mathematics;

namespace CrowdSimulation.Components
{
    public struct PathFollower : IComponentData
    {
        public int CurrentWaypointIndex;
        public float ReachDistance;
        public bool IsLooping;
        public bool HasReachedEnd;
        public float PathProgress;
    }

    public struct PathData : IBufferElementData
    {
        public float3 Waypoint;
    }

    public struct PathTarget : IComponentData
    {
        public float3 Position;
        public float3 Direction;
        public float Distance;
    }
}