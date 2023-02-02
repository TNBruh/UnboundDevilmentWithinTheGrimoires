using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;
using Random = Unity.Mathematics.Random;

[DisableAutoCreation, AlwaysUpdateSystem]
public class C2S2System : SystemBase 
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

        //parallel ecb
        EntityCommandBuffer.ParallelWriter ecbParallel = S2SO.ecbS.CreateCommandBuffer().AsParallelWriter();

        //melt signal
        bool melt = S2SO.melt;

        //freeze signal
        bool freeze = S2SO.freeze;

        //allocated native array
        NativeArray<Entity> allocatedEntities = new NativeArray<Entity>(new Entity[2] { S2SO.c2, S2SO.p }, Allocator.TempJob);

        Entities.WithDisposeOnCompletion(allocatedEntities).ForEach((Entity entity, int entityInQueryIndex, ref RandomizerData randomizerData, ref Translation translation, ref C2S2Data c2S2Data, in Rotation rotation) => {

            NativeArray<Entity>.ReadOnly allocated = allocatedEntities.AsReadOnly();

            if (melt) //yes, i'm fully aware that this is slightly bloated. but i'm letting this go for the sake of clarity
            {
                if (!SpellManagerMB.IsInBarrier(translation.Value, S2SO.blossomClamp[0], S2SO.blossomClamp[1]))
                {
                    //destroy origin
                    ecbParallel.DestroyEntity(entityInQueryIndex, entity);

                    return;
                }

                //they all have the same translation as the origin

                //their data
                C2S2Data blossomData = new C2S2Data
                {
                    speed = c2S2Data.speed,
                    frozen = false,
                };

                for (int i = 0; i < S2SO.blossomFireCount; i++)
                {
                    //generate random rotation in float
                    float rotEuler = randomizerData.value.NextFloat(360f);

                    //their rot
                    Rotation blossomRot = new Rotation
                    {
                        Value = SpellManagerMB.Degrees2Quaternion(rotEuler)
                    };

                    //inherits random data
                    RandomizerData blossomRand = new RandomizerData
                    {
                        value = randomizerData.value
                    };

                    //generate chance to be a pepe
                    bool2 isPepe = randomizerData.value.NextBool2();

                    //placeholder variable for spawned entity
                    Entity spawned;
                    
                    //spawn pepe
                    if (isPepe.x && isPepe.y) 
                    {
                        spawned = ecbParallel.Instantiate(entityInQueryIndex, allocated[1]);

                        //assign data
                        //pepe data
                        PS2Data pepeData = GetComponent<PS2Data>(allocated[1]);
                        pepeData.originalRot = rotEuler;
                        ecbParallel.SetComponent(entityInQueryIndex, spawned, pepeData);
                    } else
                    {
                        spawned = ecbParallel.Instantiate(entityInQueryIndex, allocated[0]);

                        //assign data
                        //blossom data
                        ecbParallel.SetComponent(entityInQueryIndex, spawned, blossomData);
                        //randomizer
                        ecbParallel.SetComponent(entityInQueryIndex, spawned, blossomRand);
                    }

                    //rotation
                    ecbParallel.SetComponent(entityInQueryIndex, spawned, blossomRot);
                    //translation
                    ecbParallel.SetComponent(entityInQueryIndex, spawned, translation);
                }


                //destroy origin
                ecbParallel.DestroyEntity(entityInQueryIndex, entity);

            } else if (freeze)
            {
                c2S2Data.frozen = true;
            } else if (!melt && !freeze && !c2S2Data.frozen)
            {
                //move bullet
                float3 forwardVec = SpellManagerMB.CalculateForward(rotation.Value);
                translation.Value += forwardVec * time * c2S2Data.speed;
            }
        }).ScheduleParallel();

        //synchronized signal reset
        S2SO.melt = !melt && S2SO.melt;
        S2SO.freeze = !freeze && S2SO.freeze;
    }


}