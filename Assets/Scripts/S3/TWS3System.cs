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
public class TWS3System : SystemBase
{
    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        //reset wing prog
        S3SO.wingProg = 0;
    }

    protected override void OnUpdate()
    {
        //delta time
        float time = Time.DeltaTime;


        if (S3SO.tulipIntro)
        {
            //wing prog
            float wingProg = S3SO.wingProg;

            //wing prog speed
            float wingProgSpeed = S3SO.wingProgSpeed;

            wingProg = math.clamp(wingProg + time * wingProgSpeed, 0, 1);

            Entities.ForEach((Entity entity, int entityInQueryIndex, ref TWS3Data data, ref Rotation rotation) =>
            {

                rotation.Value = SpellManagerMB.Degrees2Quaternion(math.lerp(data.initRot, data.endRot, wingProg));
            }).ScheduleParallel();

            S3SO.wingProg = wingProg;

            if (wingProg == 1)
            {
                S3SO.tulipIntro = false;
                S3SO.intro = false;
            }
        }

        
    }
}
