using Unity.Entities;
using Unity.Mathematics;

public struct CrowdAgentComponent : IComponentData
{
    public float MaxSpeed;
    public float AccelerationForce;
    public float AvoidanceRadius;
    public float SeparationWeight;
    public float PathFollowingWeight;
    public int CurrentWaypointIndex;
    public Entity TargetWaypoint;
    public bool HasReachedDestination;
}