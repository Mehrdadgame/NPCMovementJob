using Unity.Mathematics;
using Unity.Collections;

public static class CrowdUtils
{
    public static float3 Seek(float3 currentPosition, float3 currentVelocity, float3 targetPosition, float maxSpeed)
    {
        var desired = math.normalize(targetPosition - currentPosition) * maxSpeed;
        return desired - currentVelocity;
    }

    public static float3 Separate(float3 currentPosition, NativeArray<float3> neighborPositions, float separationRadius)
    {
        float3 steer = float3.zero;
        int count = 0;

        for (int i = 0; i < neighborPositions.Length; i++)
        {
            var distance = math.distance(currentPosition, neighborPositions[i]);
            if (distance > 0 && distance < separationRadius)
            {
                var diff = math.normalize(currentPosition - neighborPositions[i]) / distance;
                steer += diff;
                count++;
            }
        }

        if (count > 0)
        {
            steer /= count;
            return math.normalize(steer);
        }

        return float3.zero;
    }

    public static bool IsNearWaypoint(float3 position, float3 waypoint, float radius)
    {
        return math.distance(position, waypoint) < radius;
    }
}