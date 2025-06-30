using Unity.Collections;
using Unity.Mathematics;

namespace CrowdSimulation.Data
{
    public struct SpatialCell
    {
        public NativeList<int> EntityIndices;
        public int Count;
    }

    public struct SpatialGrid : System.IDisposable
    {
        public float CellSize;
        public int GridWidth;
        public int GridHeight;
        public float3 WorldMin;
        public float3 WorldMax;
        public NativeArray<SpatialCell> Cells;

        public SpatialGrid(float cellSize, int width, int height, float3 worldMin, float3 worldMax, Allocator allocator)
        {
            CellSize = cellSize;
            GridWidth = width;
            GridHeight = height;
            WorldMin = worldMin;
            WorldMax = worldMax;
            Cells = new NativeArray<SpatialCell>(width * height, allocator);

            for (int i = 0; i < Cells.Length; i++)
            {
                Cells[i] = new SpatialCell
                {
                    EntityIndices = new NativeList<int>(8, allocator),
                    Count = 0
                };
            }
        }

        public int GetCellIndex(float3 position)
        {
            int x = (int)math.floor((position.x - WorldMin.x) / CellSize);
            int z = (int)math.floor((position.z - WorldMin.z) / CellSize);

            x = math.clamp(x, 0, GridWidth - 1);
            z = math.clamp(z, 0, GridHeight - 1);

            return z * GridWidth + x;
        }

        public void Dispose()
        {
            if (Cells.IsCreated)
            {
                for (int i = 0; i < Cells.Length; i++)
                {
                    if (Cells[i].EntityIndices.IsCreated)
                        Cells[i].EntityIndices.Dispose();
                }
                Cells.Dispose();
            }
        }
    }
}