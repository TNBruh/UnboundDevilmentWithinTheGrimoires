using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[AlwaysUpdateSystem, DisableAutoCreation]
public class B1S3System : SystemBase
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

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in Rotation rotation, in B1S3Data data) =>
        {
            float3 forwardVec = SpellManagerMB.CalculateForward(rotation.Value);
            translation.Value += forwardVec * time * data.speed;
        }).ScheduleParallel();
    }
}
