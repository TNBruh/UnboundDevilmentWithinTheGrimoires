using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[AlwaysUpdateSystem, DisableAutoCreation]
public class C1S3System : SystemBase
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

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref C1S3Data data, ref Translation translation, in Rotation rotation) =>
        {
            if (data.posProg < S3SO.c1TimeToFloat)
            {
                data.posProg += time;

                float3 forwardVec = SpellManagerMB.CalculateForward(rotation.Value);

                translation.Value += forwardVec * time * data.speed;
            }
        }).ScheduleParallel();
    }
}