using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;

[/* The MovementSystem class in C# schedules a MovementJob to update movement based on delta time in a
SimulationSystemGroup after the AvoidanceSystem. */
UpdateInGroup(typeof(SimulationSystemGroup))]
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

/* The `[BurstCompile]` attribute in C# is used to instruct the Burst compiler to optimize the
specified method for performance. In this case, the `MovementJob` struct implements the `IJobEntity`
interface, which is used in Unity's Job System for parallel processing of entities. The `Execute`
method within the struct defines the logic for updating the movement of entities based on various
parameters such as steering force, velocity, position, and rotation. */
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