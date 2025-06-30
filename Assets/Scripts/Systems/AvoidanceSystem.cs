using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using CrowdSimulation.Components;
using CrowdSimulation.Data;
using Unity.Jobs;

namespace CrowdSimulation.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PathFollowingSystem))]
    [UpdateBefore(typeof(MovementSystem))]
    public partial struct AvoidanceSystem : ISystem
    {
        private SpatialGrid spatialGrid;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CrowdAgent>();

            // Initialize spatial grid
            spatialGrid = new SpatialGrid(
                4f, // Cell size
                32, // Width
                32, // Height
                new float3(-64, 0, -64), // World min
                new float3(64, 0, 64),   // World max
                Allocator.Persistent
            );
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            spatialGrid.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Clear spatial grid
            var clearGridJob = new ClearSpatialGridJob
            {
                SpatialGrid = spatialGrid
            };
            var clearHandle = clearGridJob.Schedule();

            // Populate spatial grid
            var populateGridJob = new PopulateSpatialGridJob
            {
                SpatialGrid = spatialGrid
            };
            var populateHandle = populateGridJob.ScheduleParallel(clearHandle);

            // Calculate avoidance forces
            var avoidanceJob = new AvoidanceJob
            {
                SpatialGrid = spatialGrid,
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            avoidanceJob.ScheduleParallel(populateHandle);
        }
    }

    [BurstCompile]
    public partial struct ClearSpatialGridJob : IJob
    {
        public SpatialGrid SpatialGrid;

        public void Execute()
        {
            for (int i = 0; i < SpatialGrid.Cells.Length; i++)
            {
                var cell = SpatialGrid.Cells[i];
                cell.EntityIndices.Clear();
                cell.Count = 0;
                SpatialGrid.Cells[i] = cell;
            }
        }
    }

    [BurstCompile]
    public partial struct PopulateSpatialGridJob : IJobEntity
    {
        public SpatialGrid SpatialGrid;

        public void Execute([EntityIndexInQuery] int entityIndex, in LocalTransform transform)
        {
            var cellIndex = SpatialGrid.GetCellIndex(transform.Position);
            var cell = SpatialGrid.Cells[cellIndex];
            cell.EntityIndices.Add(entityIndex);
            cell.Count++;
            SpatialGrid.Cells[cellIndex] = cell;
        }
    }

    [BurstCompile]
    public partial struct AvoidanceJob : IJobEntity
    {
        [ReadOnly] public SpatialGrid SpatialGrid;
        [ReadOnly] public float DeltaTime;

        public void Execute([EntityIndexInQuery] int entityIndex,
                           ref SteeringForce steeringForce,
                           in LocalTransform transform,
                           in CrowdAgent agent,
                           in AvoidanceData avoidanceData,
                           in Velocity velocity)
        {
            var position = transform.Position;
            var cellIndex = SpatialGrid.GetCellIndex(position);

            float3 separation = float3.zero;
            float3 alignment = float3.zero;
            float3 cohesion = float3.zero;
            int neighborCount = 0;

            // Check current cell and neighboring cells
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    var checkX = (cellIndex % SpatialGrid.GridWidth) + dx;
                    var checkZ = (cellIndex / SpatialGrid.GridWidth) + dz;

                    if (checkX < 0 || checkX >= SpatialGrid.GridWidth ||
                        checkZ < 0 || checkZ >= SpatialGrid.GridHeight)
                        continue;

                    var checkCellIndex = checkZ * SpatialGrid.GridWidth + checkX;
                    var cell = SpatialGrid.Cells[checkCellIndex];

                    for (int i = 0; i < cell.Count; i++)
                    {
                        var otherIndex = cell.EntityIndices[i];
                        if (otherIndex == entityIndex) continue;

                        // This is simplified - in real implementation you'd need to access other entities' data
                        // For now, we'll use basic separation logic
                        var distance = math.distance(position, position); // Placeholder

                        if (distance < avoidanceData.AvoidanceRadius && distance > 0.1f)
                        {
                            var diff = position - position; // Placeholder
                            diff = math.normalize(diff) / distance; // Weight by distance
                            separation += diff;
                            neighborCount++;
                        }
                    }
                }
            }

            // Apply avoidance forces
            if (neighborCount > 0)
            {
                separation = math.normalize(separation) * agent.MaxSpeed - velocity.Value;
                separation = math.normalize(separation) * math.min(math.length(separation), agent.MaxForce);
            }

            steeringForce.Separation = separation * avoidanceData.SeparationWeight;
            steeringForce.Alignment = alignment * avoidanceData.AlignmentWeight;
            steeringForce.Cohesion = cohesion * avoidanceData.CohesionWeight;
        }
    }
}