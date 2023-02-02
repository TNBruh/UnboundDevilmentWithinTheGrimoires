using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

[AlwaysUpdateSystem, DisableAutoCreation]
public class FS3System : SystemBase
{
    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        S3SO.petalPrefabs = new Entity[S3SO.petalSpawnerCount];
        S3SO.petalPrefabs.Initialize();
        S3SO.intro = true;

        for (int i = 0; i < S3SO.petalSpawnerCount; i++)
        {
            //instantiate prefab
            S3SO.petalPrefabs[i] = EntityManager.Instantiate(S3SO.f);

            //set as prefab
            EntityManager.AddComponent(S3SO.petalPrefabs[i], typeof(Prefab));

            //generate random data component
            RandomizerData r = new RandomizerData();
            r.value.InitState((uint)(S3SO.randomSeed + i));

            //set randomization data component
            SetComponent(S3SO.petalPrefabs[i], r);
        }
    }

    protected override void OnUpdate()
    {
        //delta time
        float time = Time.DeltaTime;


        if (S3SO.petalFire && !S3SO.intro)
        {
            //ecb parallel writer
            EntityCommandBuffer.ParallelWriter ecbParallel = S1SO.ecbS.CreateCommandBuffer().AsParallelWriter();

            NativeArray<Entity> randPetals = new NativeArray<Entity>(S3SO.petalPrefabs, Allocator.TempJob);

            Dependency = new PetalFire
            {
                randPetals = randPetals,
                randomizerPool = GetComponentDataFromEntity<RandomizerData>(),
                ecbParallel = ecbParallel

            }.Schedule(6, 2, Dependency);

            randPetals.Dispose(Dependency);

            S3SO.petalFire = false;
        }

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Rotation rotation, ref Translation translation, in FS3Data data) =>
        {
            //fall
            float3 forwardVec = SpellManagerMB.CalculateForward(data.fallDirection);
            translation.Value += forwardVec * time * data.speed;

            //rotate
            rotation.Value = math.mul(rotation.Value, SpellManagerMB.Degrees2Quaternion(data.rotSpeed * time));

        }).ScheduleParallel();
    }

    [BurstCompile]
    struct PetalFire : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] public NativeArray<Entity> randPetals;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<RandomizerData> randomizerPool;
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        public void Execute(int index)
        {
            //retrieve component
            RandomizerData randomizerData = randomizerPool[randPetals[index]];

            //spawn area
            float3 spawnArea = randomizerData.value.NextFloat3(S3SO.petalSpawnPos[0], S3SO.petalSpawnPos[1]);
            Translation translation = new Translation
            {
                Value = spawnArea
            };

            //set randomization on prefab petal
            ecbParallel.SetComponent(index, randPetals[index], randomizerData);

            //spawned entity
            Entity spawned = ecbParallel.Instantiate(index, randPetals[index]);

            //set data
            FS3Data fS3Data = new FS3Data
            {
                fallDirection = quaternion.RotateZ(math.PI),
                speed = randomizerData.value.NextFloat(S3SO.petalSpeedRange[0], S3SO.petalSpeedRange[1]),
                rotSpeed = randomizerData.value.NextFloat(S3SO.petalRotSpeedRange[0], S3SO.petalRotSpeedRange[1]),
            };
            ecbParallel.SetComponent(index, spawned, fS3Data);
            ecbParallel.SetComponent(index, spawned, translation);
            ecbParallel.SetComponent(index, spawned, new Rotation
            {
                Value = SpellManagerMB.Degrees2Quaternion(randomizerData.value.NextFloat(0f, 360f))
            });

        }
    }

}
