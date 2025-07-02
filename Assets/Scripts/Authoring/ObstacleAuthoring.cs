using Unity.Entities;
using UnityEngine;

/* The ObstacleAuthoring class defines properties for an obstacle in Unity and visualizes it in the
scene view using Gizmos. */
public class ObstacleAuthoring : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public float Radius = 1.0f;
    public float Height = 2.0f;
    public bool IsStatic = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);

        if (Height > 0)
        {
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * Height);
        }
    }
}

/* The ObstacleBaker class extends the Baker class and implements the Bake method to create obstacle
entities based on ObstacleAuthoring data. */
public class ObstacleBaker : Baker<ObstacleAuthoring>
{
    public override void Bake(ObstacleAuthoring authoring)
    {
        var entity = GetEntity(authoring.IsStatic ? TransformUsageFlags.Renderable : TransformUsageFlags.Dynamic);

        AddComponent(entity, new ObstacleComponent
        {
            Radius = authoring.Radius,
            Height = authoring.Height,
            IsStatic = authoring.IsStatic
        });
    }
}