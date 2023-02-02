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
using Unity.Burst;

[AlwaysUpdateSystem, DisableAutoCreation]
public class C1S2System : SystemBase
{

    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        S2SO.icePrefabs = new Entity[S2SO.iceCount];
        for (int i = 0; i < S2SO.iceCount; i++)
        {
            S2SO.icePrefabs[i] = EntityManager.Instantiate(S2SO.c1);
            EntityManager.AddComponent<Prefab>(S2SO.icePrefabs[i]);
            RandomizerData rand = new RandomizerData { };
            rand.value.InitState(S2SO.randomSeed + (uint)i);
            SetComponent(S2SO.icePrefabs[i], rand);
        }
    }

    protected override void OnUpdate()
    {
        //delta time
        float time = Time.DeltaTime;

        //c1 fire signal
        bool c1Fire = S2SO.c1Fire;

        //c2 fire signal
        bool c2Fire = S2SO.c2Fire;

        //freeze signal
        bool freeze = S2SO.freeze;

        //melt signal
        bool melt = S2SO.melt;

        //parallel buffer
        EntityCommandBuffer.ParallelWriter ecbParallel = S2SO.ecbS.CreateCommandBuffer().AsParallelWriter();

        //single buffer
        EntityCommandBuffer ecbSingle = S2SO.ecbS.CreateCommandBuffer();

        //allocated native array
        NativeArray<Entity> allocatedEntities = new NativeArray<Entity>(new Entity[] { S2SO.c2, S2SO.c1 }, Allocator.TempJob);

        //fire ice
        if (c1Fire)
        {
            //calculate shot direction and normalize it
            Translation playerTranslation = GetComponent<Translation>(PlayerSystem.player);
            Translation npcTranslation = GetComponent<Translation>(S2SO.npc);
            float3 dirVector = playerTranslation.Value - npcTranslation.Value;
            dirVector = math.normalize(dirVector);

            //convert direction to radian
            float dirRad = math.atan((dirVector.y / dirVector.x));

            //calculate shot spread in radian
            float shotSpread = 6.283185f / S2SO.iceCount;

            NativeArray<Entity> ices = new NativeArray<Entity>(S2SO.icePrefabs, Allocator.TempJob);

            Dependency = new SpawnIce
            {
                dirRad = dirRad,
                ecbParallel = ecbParallel,
                ices = ices,
                randomizerPool = GetComponentDataFromEntity<RandomizerData>(),
                shotSpread = shotSpread,
                spawnTranslation = npcTranslation
            }.Schedule((int)S2SO.iceCount, 1, Dependency);

            ices.Dispose(Dependency);

            /*
            //npc rand
            RandomizerData randomizerData = GetComponent<RandomizerData>(S2SO.npc);

            for (int i = 0; i < S2SO.iceCount; i++)
            {
                //shifts the state to ensure "true" randomness
                randomizerData.value.state += (uint)i;

                //converts direction rad into quaternion
                quaternion dirQuat = quaternion.RotateZ(i * shotSpread + dirRad);

                //spawn ice
                Entity spawnedIce = ecbSingle.Instantiate(allocatedEntities[1]);

                //sets ice's components
                ecbSingle.SetComponent(spawnedIce, npcTranslation);
                ecbSingle.SetComponent(spawnedIce, new Rotation
                {
                    Value = dirQuat
                });
                ecbSingle.SetComponent(spawnedIce, randomizerData);
            }

            //set new rand data for npc
            ecbSingle.SetComponent(S2SO.npc, randomizerData);
            */
        }

        Entities.WithDisposeOnCompletion(allocatedEntities).ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref RandomizerData randomizer, in Rotation rotation, in C1S2Data c1S2Data)=> {

            NativeArray<Entity>.ReadOnly allocated = allocatedEntities.AsReadOnly();

            //move
            float3 forwardVec = SpellManagerMB.CalculateForward(rotation.Value);
            translation.Value += forwardVec * time * c1S2Data.speed;

            //spawn
            if (c2Fire && !freeze && !melt)
            {
                //random 3 positions
                for (int i = 0; i < 3; i++)
                {
                    //variables and randomization
                    float spawnRad = randomizer.value.NextFloat(0, 2 * math.PI);
                    float xDist, yDist;
                    math.sincos(spawnRad, out xDist, out yDist);

                    //translation
                    float2 relativePos = new float2
                    {
                        x = xDist,
                        y = yDist,
                    } * S2SO.c2SpawnDist;
                    Translation blossomPosition = new Translation
                    {
                        Value = new float3
                        {
                            x = relativePos.x + translation.Value.x,
                            y = relativePos.y + translation.Value.y
                        }
                    };

                    //rotation
                    Rotation blossomRotation = new Rotation
                    {
                        Value = quaternion.RotateZ(spawnRad)
                    };

                    //randomizer
                    RandomizerData rand = new RandomizerData
                    {
                        value = randomizer.value
                    };
                    //ensures randomness
                    rand.value.state += (uint)i;

                    if (SpellManagerMB.IsInBarrier(blossomPosition.Value))
                    {
                        //spawn
                        Entity spawnedC2 = ecbParallel.Instantiate(entityInQueryIndex, allocated[0]);

                        //set component
                        ecbParallel.SetComponent(entityInQueryIndex, spawnedC2, rand);
                        ecbParallel.SetComponent(entityInQueryIndex, spawnedC2, blossomPosition);
                        ecbParallel.SetComponent(entityInQueryIndex, spawnedC2, blossomRotation);
                    }
                }
            }
        }).ScheduleParallel();

        

        //synchronized signal reset
        S2SO.c1Fire = !c1Fire && S2SO.c1Fire;
        S2SO.c2Fire = !c2Fire && S2SO.c2Fire;

    }

    [BurstCompile]
    struct SpawnIce : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeArray<Entity> ices;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<RandomizerData> randomizerPool;
        public Translation spawnTranslation;
        public float shotSpread;
        public float dirRad;
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        public void Execute(int index)
        {
            //shifts the state to ensure "true" randomness
            RandomizerData randomizerData = randomizerPool[ices[index]];
            randomizerData.value.NextBool();

            //converts direction rad into quaternion
            quaternion dirQuat = quaternion.RotateZ(index * shotSpread + dirRad);

            //spawn ice
            Entity spawned = ecbParallel.Instantiate(index, ices[index]);

            //set
            ecbParallel.SetComponent(index, spawned, spawnTranslation);
            ecbParallel.SetComponent(index, spawned, new Rotation
            {
                Value = dirQuat
            });
            ecbParallel.SetComponent(index, spawned, randomizerData);

            //set new random
            ecbParallel.SetComponent(index, ices[index], randomizerData);

        }
    }
}
