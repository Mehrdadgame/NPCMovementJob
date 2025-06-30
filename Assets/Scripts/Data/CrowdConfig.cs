using UnityEngine;

namespace CrowdSimulation.Data
{
    [CreateAssetMenu(fileName = "CrowdConfig", menuName = "Crowd Simulation/Crowd Config")]
    public class CrowdConfig : ScriptableObject
    {
        [Header("Spawn Settings")]
        [Range(1, 1000)]
        public int MaxNPCCount = 200;
        public float SpawnRadius = 5f;
        public float SpawnInterval = 0.1f;

        [Header("Movement Settings")]
        [Range(0.5f, 20f)]
        public float DefaultSpeed = 3.5f;
        [Range(1f, 25f)]
        public float MaxSpeed = 7f;
        [Range(0.1f, 5f)]
        public float MaxForce = 2f;

        [Header("Avoidance Settings")]
        [Range(0.5f, 10f)]
        public float AvoidanceRadius = 2f;
        [Range(0.1f, 5f)]
        public float SeparationWeight = 1.5f;
        [Range(0.1f, 5f)]
        public float AlignmentWeight = 1f;
        [Range(0.1f, 5f)]
        public float CohesionWeight = 1f;

        [Header("Path Settings")]
        [Range(0.1f, 5f)]
        public float WaypointReachDistance = 1.5f;
        public bool UseLoopingPaths = true;

        [Header("Performance Settings")]
        [Range(1, 64)]
        public int SpatialGridSize = 32;
        [Range(0.5f, 10f)]
        public float SpatialCellSize = 4f;

        [Header("Debug Settings")]
        public bool ShowDebugPaths = true;
        public bool ShowAvoidanceRadius = false;
        public bool ShowPerformanceStats = true;
        public Color PathColor = Color.yellow;
        public Color AvoidanceColor = Color.red;
    }
}