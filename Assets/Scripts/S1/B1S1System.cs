using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

[AlwaysUpdateSystem, DisableAutoCreation]
public class B1S1System : SystemBase
{
    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {

    }

    protected override void OnUpdate()
    {
        //delta time
        float time = Time.DeltaTime;

        //ecb parallel writer
        EntityCommandBuffer.ParallelWriter ecbParallel = S1SO.ecbS.CreateCommandBuffer().AsParallelWriter();

        //allocated entities
        NativeArray<Entity> allocatedEntities = new NativeArray<Entity>(new Entity[1] { S1SO.b2 }, Allocator.TempJob);

        Entities.WithDisposeOnCompletion(allocatedEntities).ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref Translation translation, in Rotation rotation, in B1S1Data b1S1Data) =>
        {
            //mark as read-only
            NativeArray<Entity>.ReadOnly allocated = allocatedEntities.AsReadOnly();

            //move bullet
            float3 forwardVec = SpellManagerMB.CalculateForward(rotation.Value);
            translation.Value += forwardVec * time * b1S1Data.speed;

            //convert
            bool bounce = SpellManagerMB.IsInBarrier(translation.Value);
            if (!bounce)
            {
                //queue for destruction
                ecbParallel.DestroyEntity(entityInQueryIndex, entity);

                //spawns and set bullet
                Entity bounced = ecbParallel.Instantiate(entityInQueryIndex, allocated[0]);
                quaternion bouncedRot = math.mul(rotation.Value, SpellManagerMB.Degrees2Quaternion(180));
                ecbParallel.SetComponent(entityInQueryIndex, bounced, translation);
                ecbParallel.SetComponent(entityInQueryIndex, bounced, new Rotation { Value = bouncedRot });
            }

        }).ScheduleParallel();

        
    }
    /*
    protected override void OnStopRunning()
    {
        //ecb parallel writer
        EntityCommandBuffer.ParallelWriter ecbParallel = S1SO.ecbS.CreateCommandBuffer().AsParallelWriter();

        Entities.WithAll<B1S1Data>().ForEach((Entity entity, int entityInQueryIndex) =>
        {
            ecbParallel.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel();
    }
    */
}
