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
public class TCS3System : SystemBase
{
    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        S3SO.tulipIntro = false;
        S3SO.tulipCanFire = false;
    }

    protected override void OnUpdate()
    {
        //delta time
        float time = Time.DeltaTime;

        //intro signal
        bool intro = S3SO.intro;

        //npc
        Entity npc = NPCSystem.NPC;

        //tulip intro
        bool tulipIntro = S3SO.tulipIntro;

        //open fire
        bool tulipFire = S3SO.tulipFire;


        //tulip can begin firing
        bool tulipCanFire = S3SO.tulipCanFire;


        if (tulipFire && tulipCanFire) //fire bullet barrage
        {
            //parallel buffer
            EntityCommandBuffer.ParallelWriter ecbParallel = S3SO.ecbS.CreateCommandBuffer().AsParallelWriter();

            //allocated
            NativeArray<Entity> allocated = new NativeArray<Entity>(new Entity[] { S3SO.e, S3SO.b1 }, Allocator.TempJob);

            Dependency = Entities.WithDisposeOnCompletion(allocated).WithAny<TCS3Data, TWS3Data>().ForEach((Entity entity, int entityInQueryIndex, in LocalToWorld localToWorld) =>
            {
                NativeArray<Entity>.ReadOnly readOnly = allocated.AsReadOnly();
                Entity spawned = ecbParallel.Instantiate(entityInQueryIndex, readOnly[1]);

                ecbParallel.SetComponent(entityInQueryIndex, spawned, new Translation { Value = localToWorld.Position });
                ecbParallel.SetComponent(entityInQueryIndex, spawned, new Rotation
                {
                    Value = localToWorld.Rotation
                });
            }).ScheduleParallel(Dependency);

            Dependency.Complete();
        }

        S3SO.tulipFire = !tulipFire && S3SO.tulipFire;


        if (!intro) //wave
        {
            Entities.ForEach((Entity entity, int entityInQueryIndex, ref Rotation rotation, ref TCS3Data data) =>
            {
                data.wavePhase = (data.wavePhase + time * data.waveSpeed) % (2 * math.PI);
                float lerpProg = (math.sin(data.wavePhase) + 1) / 2;
                float lerpProgEuler = math.lerp(-S3SO.waveDeviation, S3SO.waveDeviation, lerpProg);
                rotation.Value = SpellManagerMB.Degrees2Quaternion(lerpProgEuler + 180);

            }).ScheduleParallel();
        }

    }
}