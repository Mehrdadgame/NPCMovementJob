using Unity.Collections;
using Unity.Mathematics;

namespace CrowdSimulation.Utilities
{
    public static class SpatialHashing
    {
        public static int GetCellKey(float3 position, float cellSize)
        {
            int x = (int)math.floor(position.x / cellSize);
            int z = (int)math.floor(position.z / cellSize);

            // Simple hash function
            return (x * 73856093) ^ (z * 19349663);
        }

        public static int2 GetCellCoords(float3 position, float cellSize)
        {
            return new int2(
                (int)math.floor(position.x / cellSize),
                (int)math.floor(position.z / cellSize)
            );
        }

        public static NativeArray<int> GetNeighborCells(int2 cellCoords, int gridWidth, int gridHeight, Allocator allocator)
        {
            var neighbors = new NativeList<int>(9, allocator);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    var neighborX = cellCoords.x + dx;
                    var neighborZ = cellCoords.y + dz;

                    if (neighborX >= 0 && neighborX < gridWidth &&
                        neighborZ >= 0 && neighborZ < gridHeight)
                    {
                        neighbors.Add(neighborZ * gridWidth + neighborX);
                    }
                }
            }

            return neighbors.AsArray();
        }

        public static bool IsValidCellCoord(int2 coords, int gridWidth, int gridHeight)
        {
            return coords.x >= 0 && coords.x < gridWidth &&
                   coords.y >= 0 && coords.y < gridHeight;
        }
    }
}
