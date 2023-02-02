using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[AlwaysUpdateSystem, DisableAutoCreation]
public class CleanUpS1System : SystemBase
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
        bool cleanUp = S1SO.allowCleanUp;

        //bounce
        if (!cleanUp) return;

        //ecb parallel writer
        EntityCommandBuffer.ParallelWriter ecbParallel = S1SO.ecbS.CreateCommandBuffer().AsParallelWriter();


        //clean up
        Dependency = Entities.WithAny<B1S1Data, B2S1Data, MC1S1Data>().ForEach((Entity entity, int entityInQueryIndex) =>
        {
            ecbParallel.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel(Dependency);

        //force clean
        Dependency.Complete();

        //reset signal
        S1SO.allowCleanUp = !cleanUp && S1SO.allowCleanUp;
    }
}
