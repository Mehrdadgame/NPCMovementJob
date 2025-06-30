using UnityEngine;
using Unity.Entities;
using CrowdSimulation.Data;
using CrowdSimulation.Components;
using UnityEditor;

namespace CrowdSimulation.Managers
{
    public class CrowdManager : MonoBehaviour
    {
        [Header("Configuration")]
        public CrowdConfig crowdConfig;

        [Header("Runtime Controls")]
        [Range(0, 1000)]
        public int targetNPCCount = 100;

        [Header("Performance Monitoring")]
        public bool showPerformanceStats = true;
        public float updateInterval = 1f;

        private EntityManager entityManager;
        private EntityQuery npcQuery;
        private float lastUpdateTime;
        private int currentNPCCount;
        private float averageFPS;
        private float frameCount;

        void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            npcQuery = entityManager.CreateEntityQuery(typeof(NPCTag));

            if (crowdConfig == null)
            {
                Debug.LogWarning("CrowdConfig not assigned to CrowdManager!");
            }
        }

        void Update()
        {
            UpdatePerformanceStats();

            if (showPerformanceStats && Time.time - lastUpdateTime > updateInterval)
            {
                UpdateNPCCount();
                lastUpdateTime = Time.time;
            }
        }

        void UpdatePerformanceStats()
        {
            frameCount++;
            averageFPS = frameCount / Time.time;
        }

        void UpdateNPCCount()
        {
            currentNPCCount = npcQuery.CalculateEntityCount();
        }

        void OnGUI()
        {
            if (!showPerformanceStats) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");

            GUILayout.Label("Crowd Simulation Stats", EditorStyles.boldLabel);
            GUILayout.Label($"NPCs: {currentNPCCount}");
            GUILayout.Label($"Target: {targetNPCCount}");
            GUILayout.Label($"FPS: {averageFPS:F1}");
            GUILayout.Label($"Frame Time: {Time.deltaTime * 1000:F1}ms");

            if (GUILayout.Button("Clear All NPCs"))
            {
                ClearAllNPCs();
            }

            if (GUILayout.Button("Spawn Batch"))
            {
                SpawnNPCBatch(10);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        public void ClearAllNPCs()
        {
            entityManager.DestroyEntity(npcQuery);
        }

        public void SpawnNPCBatch(int count)
        {
            // This would integrate with your spawn system
            Debug.Log($"Spawning {count} NPCs");
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