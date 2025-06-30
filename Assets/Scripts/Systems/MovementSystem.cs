using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using CrowdSimulation.Components;

namespace CrowdSimulation.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AvoidanceSystem))]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CrowdAgent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var movementJob = new MovementJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            movementJob.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct MovementJob : IJobEntity
    {
        public float DeltaTime;

        public void Execute(ref LocalTransform transform,
                           ref Velocity velocity,
                           ref SteeringForce steeringForce,
                           ref MovementState movementState,
                           ref CrowdStats stats,
                           in DesiredVelocity desiredVelocity,
                           in CrowdAgent agent)
        {
            // Calculate seek force toward desired velocity
            var seek = desiredVelocity.Value - velocity.Value;
            seek = math.normalize(seek) * math.min(math.length(seek), agent.MaxForce);

            // Combine all steering forces
            steeringForce.Seek = seek;
            steeringForce.Final = seek + steeringForce.Separation + steeringForce.Alignment + steeringForce.Cohesion;

            // Limit steering force
            if (math.length(steeringForce.Final) > agent.MaxForce)
            {
                steeringForce.Final = math.normalize(steeringForce.Final) * agent.MaxForce;
            }

            // Apply steering force to velocity
            velocity.Value += steeringForce.Final * DeltaTime;

            // Limit velocity
            if (math.length(velocity.Value) > agent.MaxSpeed)
            {
                velocity.Value = math.normalize(velocity.Value) * agent.MaxSpeed;
            }

            // Store old position for stuck detection
            var oldPosition = transform.Position;

            // Apply velocity to position
            transform.Position += velocity.Value * DeltaTime;

            // Update movement state
            var distanceMoved = math.distance(oldPosition, transform.Position);
            movementState.IsMoving = distanceMoved > 0.01f;

            if (!movementState.IsMoving)
            {
                movementState.StuckTimer += DeltaTime;
                movementState.IsBlocked = movementState.StuckTimer > 2f;
            }
            else
            {
                movementState.StuckTimer = 0f;
                movementState.IsBlocked = false;
            }

            movementState.LastPosition = oldPosition;

            // Update stats
            stats.DistanceTraveled += distanceMoved;
            stats.TimeAlive += DeltaTime;

            // Update rotation to face movement direction
            if (math.length(velocity.Value) > 0.1f)
            {
                var lookDirection = math.normalize(velocity.Value);
                transform.Rotation = quaternion.LookRotationSafe(lookDirection, math.up());
            }
        }
    }
}