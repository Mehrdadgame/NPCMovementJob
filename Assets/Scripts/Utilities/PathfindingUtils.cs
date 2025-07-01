using Unity.Mathematics;
using Unity.Collections;

namespace CrowdSimulation
{
    public static class PathfindingUtils
    {

        public static NativeArray<float3> CreateSimplePath(float3 start, float3 end, Allocator allocator)
        {
            var path = new NativeArray<float3>(2, allocator);
            path[0] = start;
            path[1] = end;
            return path;
        }


        public static NativeArray<float3> CreateWaypointPath(NativeArray<float3> waypoints, Allocator allocator)
        {
            var path = new NativeArray<float3>(waypoints.Length, allocator);
            waypoints.CopyTo(path);
            return path;
        }


        public static float CalculatePathLength(NativeArray<float3> path)
        {
            float totalLength = 0f;
            for (int i = 0; i < path.Length - 1; i++)
            {
                totalLength += math.distance(path[i], path[i + 1]);
            }
            return totalLength;
        }


        public static bool IsPathClearOfObstacles(float3 start, float3 end,
            NativeArray<float3> obstaclePositions, NativeArray<float> obstacleRadii)
        {
            var direction = end - start;
            var distance = math.length(direction);
            var normalizedDirection = math.normalize(direction);

            for (int i = 0; i < obstaclePositions.Length; i++)
            {
                var obstaclePos = obstaclePositions[i];
                var obstacleRadius = obstacleRadii[i];

                var toObstacle = obstaclePos - start;
                var projectionLength = math.dot(toObstacle, normalizedDirection);

                if (projectionLength < 0 || projectionLength > distance)
                    continue;

                var projectionPoint = start + normalizedDirection * projectionLength;
                var distanceToLine = math.distance(obstaclePos, projectionPoint);

                if (distanceToLine < obstacleRadius)
                    return false;
            }

            return true;
        }


        public static float3 GetClosestPointOnPath(NativeArray<float3> path, float3 position)
        {
            float closestDistance = float.MaxValue;
            float3 closestPoint = path[0];

            for (int i = 0; i < path.Length - 1; i++)
            {
                var segmentStart = path[i];
                var segmentEnd = path[i + 1];
                var pointOnSegment = GetClosestPointOnSegment(segmentStart, segmentEnd, position);
                var distance = math.distance(position, pointOnSegment);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = pointOnSegment;
                }
            }

            return closestPoint;
        }

        private static float3 GetClosestPointOnSegment(float3 segmentStart, float3 segmentEnd, float3 point)
        {
            var segmentVector = segmentEnd - segmentStart;
            var segmentLength = math.length(segmentVector);

            if (segmentLength < 0.001f)
                return segmentStart;

            var normalizedSegment = segmentVector / segmentLength;
            var toPoint = point - segmentStart;
            var projectionLength = math.dot(toPoint, normalizedSegment);

            projectionLength = math.clamp(projectionLength, 0f, segmentLength);
            return segmentStart + normalizedSegment * projectionLength;
        }
    }
}