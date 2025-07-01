using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct PathFollowingComponent : IComponentData
{
    public BlobAssetReference<PathData> PathBlob;
    public int CurrentPathIndex;
    public float WaypointRadius;
    public bool IsLooping;
}

public struct PathData
{
    public BlobArray<float3> Waypoints;
}