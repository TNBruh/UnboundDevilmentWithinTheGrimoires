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
public class B2S1System : SystemBase
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

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in Rotation rotation, in B2S1Data b2S1Data) =>
        {
            //move bullet
            float3 forwardVec = SpellManagerMB.CalculateForward(rotation.Value);
            translation.Value += forwardVec * time * b2S1Data.speed;
        }).ScheduleParallel();
    }
    /*
    protected override void OnStopRunning()
    {
        ///ecb parallel writer
        EntityCommandBuffer.ParallelWriter ecbParallel = S1SO.ecbS.CreateCommandBuffer().AsParallelWriter();

        Entities.WithAll<B2S1Data>().ForEach((Entity entity, int entityInQueryIndex) =>
        {
            ecbParallel.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel();
    }
    */
}
