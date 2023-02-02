using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

[AlwaysUpdateSystem, DisableAutoCreation]
public class CleanUpS2System : SystemBase
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
        bool cleanUp = S2SO.allowCleanUp;

        //bounce
        if (!cleanUp) return;

        //ecb parallel writer
        EntityCommandBuffer.ParallelWriter ecbParallel = S2SO.ecbS.CreateCommandBuffer().AsParallelWriter();


        //clean up
        Dependency = Entities.WithAny<C1S2Data, C2S2Data, PS2Data>().ForEach((Entity entity, int entityInQueryIndex) =>
        {
            ecbParallel.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel(Dependency);

        //force clean
        Dependency.Complete();

        //reset signal
        S2SO.allowCleanUp = !cleanUp && S2SO.allowCleanUp;
    }
}
