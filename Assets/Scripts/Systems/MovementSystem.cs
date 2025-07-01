using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(AvoidanceSystem))]
public partial class MovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        var movementJob = new MovementJob
        {
            DeltaTime = deltaTime
        };

        Dependency = movementJob.ScheduleParallel(Dependency);
    }
}

[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float DeltaTime;

    public void Execute(
        ref LocalTransform transform,
        ref MovementComponent movement,
        in CrowdAgentComponent agent)
    {
        if (agent.HasReachedDestination) return;
        movement.SteeringForce.y = 0;
        movement.Velocity.y = 0;

        // Limit steering force
        if (math.length(movement.SteeringForce) > movement.MaxForce)
        {
            movement.SteeringForce = math.normalize(movement.SteeringForce) * movement.MaxForce;
        }

        // Update velocity
        movement.Velocity += movement.SteeringForce * DeltaTime;

        // Limit velocity
        if (math.length(movement.Velocity) > agent.MaxSpeed)
        {
            movement.Velocity = math.normalize(movement.Velocity) * agent.MaxSpeed;
        }

        // Update position
        transform.Position += movement.Velocity * DeltaTime;

        // Update rotation to face movement direction
        if (math.length(movement.Velocity) > 0.1f)
        {
            var lookDirection = math.normalize(movement.Velocity);
            transform.Rotation = quaternion.LookRotation(lookDirection, math.up());
        }

        // Reset steering force for next frame
        movement.SteeringForce = float3.zero;
    }
}