using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[AlwaysUpdateSystem, DisableAutoCreation]
public class PlayerBulletSystem : SystemBase
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
        float time = Time.DeltaTime;

        //RW Translation, R Rotation, R PlayerBulletData
        Entities.ForEach((ref Translation translation, in Rotation rotation, in PlayerBulletData playerBulletData) =>
        {
            //calculates the forward vector
            float3 forwardVec = math.mul(rotation.Value, new float3(0, 1, 0));

            //moves entity by the forward vector
            translation.Value += forwardVec * time * playerBulletData.speed;

        }).ScheduleParallel();
    }
}
