using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[AlwaysUpdateSystem, DisableAutoCreation]
public class CleanUpS3System : SystemBase
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
        //clean up snapshot
        bool cleanUp = S3SO.allowCleanUp;

        //bounce
        if (!cleanUp) return;

        //ecb parallel writer
        EntityCommandBuffer.ParallelWriter ecbParallel = S3SO.ecbS.CreateCommandBuffer().AsParallelWriter();


        //clean up
        Dependency = Entities.WithAny<BulletTag>().ForEach((Entity entity, int entityInQueryIndex) =>
        {
            ecbParallel.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel(Dependency);

        //force clean
        Dependency.Complete();

        //reset signal
        S3SO.allowCleanUp = !cleanUp && S3SO.allowCleanUp;
    }
}