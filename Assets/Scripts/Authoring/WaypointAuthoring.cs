using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class WaypointAuthoring : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public int WaypointIndex = 0;
    public Color GizmoColor = Color.blue;
    public float GizmoRadius = 0.5f;

    [Header("Connections")]
    public WaypointAuthoring NextWaypoint;
    public WaypointAuthoring[] ConnectedWaypoints;

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawWireSphere(transform.position, GizmoRadius);

        // Draw connections
        if (NextWaypoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, NextWaypoint.transform.position);
        }

        if (ConnectedWaypoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var waypoint in ConnectedWaypoints)
            {
                if (waypoint != null)
                    Gizmos.DrawLine(transform.position, waypoint.transform.position);
            }
        }
    }
}

public struct WaypointComponent : IComponentData
{
    public int WaypointIndex;
    public float3 Position;
    public Entity NextWaypoint;
}

public class WaypointBaker : Baker<WaypointAuthoring>
{
    public override void Bake(WaypointAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new WaypointComponent
        {
            WaypointIndex = authoring.WaypointIndex,
            Position = authoring.transform.position,
            NextWaypoint = authoring.NextWaypoint != null ?
                GetEntity(authoring.NextWaypoint, TransformUsageFlags.Dynamic) : Entity.Null
        });
    }
}