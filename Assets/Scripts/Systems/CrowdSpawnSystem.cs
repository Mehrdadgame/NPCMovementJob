using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using CrowdSimulation.Authoring;

namespace CrowdSimulation.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CrowdSpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CrowdSpawner>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTime = (float)SystemAPI.Time.ElapsedTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (spawner, spawnPoints, entity) in
                     SystemAPI.Query<RefRW<CrowdSpawner>, DynamicBuffer<SpawnPoint>>()
                     .WithEntityAccess())
            {
                if (spawner.ValueRO.CurrentCount >= spawner.ValueRO.MaxCount)
                    continue;

                if (currentTime < spawner.ValueRO.NextSpawnTime)
                    continue;

                // Select random spawn point
                var random = Random.CreateFromIndex((uint)(currentTime * 1000));
                var spawnIndex = random.NextInt(0, spawnPoints.Length);
                var spawnPoint = spawnPoints[spawnIndex];

                // Calculate random position around spawn point
                var randomOffset = random.NextFloat3Direction() * random.NextFloat(0f, spawner.ValueRO.SpawnRadius);
                randomOffset.y = 0; // Keep on ground
                var spawnPosition = spawnPoint.Position + randomOffset;

                // Spawn NPC
                var npcEntity = ecb.Instantiate(spawner.ValueRO.NPCPrefab);
                ecb.SetComponent(npcEntity, LocalTransform.FromPositionRotation(spawnPosition, spawnPoint.Rotation));

                // Update spawner state
                spawner.ValueRW.CurrentCount++;
                spawner.ValueRW.NextSpawnTime = currentTime + spawner.ValueRO.SpawnInterval;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}