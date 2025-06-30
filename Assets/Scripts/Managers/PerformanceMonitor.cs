using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using CrowdSimulation.Components;

namespace CrowdSimulation.Managers
{
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        public bool enableProfiling = true;
        public float sampleInterval = 0.5f;

        [Header("Performance Thresholds")]
        public float targetFPS = 60f;
        public float warningFPS = 30f;
        public int maxNPCCount = 200;

        private EntityManager entityManager;
        private EntityQuery npcQuery;
        private float[] fpsHistory;
        private int historyIndex;
        private float lastSampleTime;
        private bool performanceWarning;

        public struct PerformanceData
        {
            public int NPCCount;
            public float CurrentFPS;
            public float AverageFPS;
            public float MemoryUsage;
            public bool IsPerformanceGood;
        }

        void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            npcQuery = entityManager.CreateEntityQuery(typeof(NPCTag));
            fpsHistory = new float[60]; // 30 seconds of history at 0.5s intervals
        }

        void Update()
        {
            if (!enableProfiling) return;

            if (Time.time - lastSampleTime >= sampleInterval)
            {
                SamplePerformance();
                lastSampleTime = Time.time;
            }
        }

        void SamplePerformance()
        {
            var currentFPS = 1f / Time.deltaTime;
            fpsHistory[historyIndex] = currentFPS;
            historyIndex = (historyIndex + 1) % fpsHistory.Length;

            var performanceData = GetPerformanceData();
            CheckPerformanceThresholds(performanceData);
        }

        public PerformanceData GetPerformanceData()
        {
            var npcCount = npcQuery.CalculateEntityCount();
            var currentFPS = 1f / Time.deltaTime;
            var averageFPS = CalculateAverageFPS();
            var memoryUsage = (float)System.GC.GetTotalMemory(false) / (1024 * 1024); // MB

            return new PerformanceData
            {
                NPCCount = npcCount,
                CurrentFPS = currentFPS,
                AverageFPS = averageFPS,
                MemoryUsage = memoryUsage,
                IsPerformanceGood = averageFPS >= warningFPS
            };
        }

        float CalculateAverageFPS()
        {
            float sum = 0f;
            int count = 0;

            for (int i = 0; i < fpsHistory.Length; i++)
            {
                if (fpsHistory[i] > 0)
                {
                    sum += fpsHistory[i];
                    count++;
                }
            }

            return count > 0 ? sum / count : 0f;
        }

        void CheckPerformanceThresholds(PerformanceData data)
        {
            bool wasWarning = performanceWarning;
            performanceWarning = data.AverageFPS < warningFPS;

            if (performanceWarning && !wasWarning)
            {
                Debug.LogWarning($"Performance Warning: FPS dropped to {data.AverageFPS:F1} with {data.NPCCount} NPCs");
            }
            else if (!performanceWarning && wasWarning)
            {
                Debug.Log($"Performance Recovered: FPS back to {data.AverageFPS:F1}");
            }
        }

        void OnDestroy()
        {
            if (npcQuery.IsEmpty == false)
            {
                npcQuery.Dispose();
            }
        }
    }
}
