using Unity.Mathematics;

namespace CrowdSimulation.Utilities
{
    public static class MathUtils
    {
        public static float3 Seek(float3 position, float3 target, float3 velocity, float maxSpeed, float maxForce)
        {
            var desired = math.normalize(target - position) * maxSpeed;
            var steer = desired - velocity;
            return LimitMagnitude(steer, maxForce);
        }

        public static float3 Separate(float3 position, float3[] neighbors, float separationRadius, float maxSpeed, float maxForce)
        {
            float3 steer = float3.zero;
            int count = 0;

            foreach (var neighbor in neighbors)
            {
                var distance = math.distance(position, neighbor);
                if (distance > 0 && distance < separationRadius)
                {
                    var diff = math.normalize(position - neighbor);
                    diff /= distance; // Weight by distance
                    steer += diff;
                    count++;
                }
            }

            if (count > 0)
            {
                steer /= count;
                steer = math.normalize(steer) * maxSpeed;
                steer -= float3.zero; // current velocity would go here
                steer = LimitMagnitude(steer, maxForce);
            }

            return steer;
        }

        public static float3 LimitMagnitude(float3 vector, float maxMagnitude)
        {
            var magnitude = math.length(vector);
            if (magnitude > maxMagnitude)
            {
                return math.normalize(vector) * maxMagnitude;
            }
            return vector;
        }

        public static float3 RandomPointInCircle(Random random, float3 center, float radius)
        {
            var angle = random.NextFloat(0, 2 * math.PI);
            var distance = random.NextFloat(0, radius);

            return center + new float3(
                math.cos(angle) * distance,
                0,
                math.sin(angle) * distance
            );
        }

        public static bool IsPointInBounds(float3 point, float3 min, float3 max)
        {
            return point.x >= min.x && point.x <= max.x &&
                   point.z >= min.z && point.z <= max.z;
        }

        public static float3 WrapPosition(float3 position, float3 bounds)
        {
            var result = position;

            if (result.x > bounds.x) result.x = -bounds.x;
            else if (result.x < -bounds.x) result.x = bounds.x;

            if (result.z > bounds.z) result.z = -bounds.z;
            else if (result.z < -bounds.z) result.z = bounds.z;

            return result;
        }
    }
}