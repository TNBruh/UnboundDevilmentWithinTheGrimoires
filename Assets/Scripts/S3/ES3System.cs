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
using Unity.Burst;

[AlwaysUpdateSystem, DisableAutoCreation]
public class ES3System : SystemBase
{
    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {

        S3SO.stormCirclePrefabs = new Entity[(int)S3SO.circlePerStorm];
        S3SO.stormCirclePrefabs.Initialize();

        for (int i = 0; i < S3SO.circlePerStorm; i++)
        {
            S3SO.stormCirclePrefabs[i] = EntityManager.Instantiate(S3SO.c1);
            //rotation
            Rotation rotation = new Rotation
            {
                Value = SpellManagerMB.Degrees2Quaternion(i * (360 / S3SO.circlePerStorm))
            };
            SetComponent(S3SO.stormCirclePrefabs[i], rotation);
            EntityManager.AddComponent<Prefab>(S3SO.stormCirclePrefabs[i]);
            Parent parent = new Parent
            {
                Value = S3SO.e
            };
            LocalToParent localToParent = new LocalToParent { };
            EntityManager.AddComponent<Parent>(S3SO.stormCirclePrefabs[i]);
            EntityManager.AddComponent<LocalToParent>(S3SO.stormCirclePrefabs[i]);
            SetComponent(S3SO.stormCirclePrefabs[i], parent);
            SetComponent(S3SO.stormCirclePrefabs[i], localToParent);
            //linkedEntityGroup.Add(S3SO.stormCirclePrefabs[i]);
        }

        DynamicBuffer<LinkedEntityGroup> linkedEntityGroup = EntityManager.AddBuffer<LinkedEntityGroup>(S3SO.e);

        linkedEntityGroup.Add(S3SO.e);

        foreach (Entity e in S3SO.stormCirclePrefabs)
        {
            linkedEntityGroup.Add(e);
        }

        /* leaving this as for a reason later on
        JobHandle preprocessStorm = new SpawnC
        {
            c1 = S3SO.c1,
            ecbParallel = S3SO.ecbS.CreateCommandBuffer().AsParallelWriter(),
            storm = S3SO.e,
            stormCircles = stormCircles,
        }.Schedule((int)S3SO.circlePerStorm, 4, Dependency);

        preprocessStorm.Complete();

        S3SO.stormCirclePrefabs = stormCircles.ToArray();

        EntityCommandBuffer ecbSingle = S3SO.ecbS.CreateCommandBuffer();

        foreach (Entity c in S3SO.stormCirclePrefabs)
        {
            linkedEntityGroup.Add(c);
            ecbSingle.AddComponent<Prefab>(c);
            Debug.Log(c == null);
            //EntityManager.AddComponent<Prefab>(c);
        }
        stormCircles.Dispose();
        */
    }

    protected override void OnUpdate()
    {
        //delta time
        float time = Time.DeltaTime;

        //parallel buffer
        EntityCommandBuffer.ParallelWriter ecbParallel = S3SO.ecbS.CreateCommandBuffer().AsParallelWriter();

        if (!S3SO.intro && S3SO.eFire)
        {
            //allocated
            NativeArray<Entity> allocated = new NativeArray<Entity>(new Entity[] { S3SO.e, S3SO.b1 }, Allocator.TempJob);
            float rndDir = UnityEngine.Random.Range(-S3SO.stormFallDeviation, S3SO.stormFallDeviation);
            quaternion rndRot = SpellManagerMB.Degrees2Quaternion(UnityEngine.Random.Range(0f, 90f));

            //original data
            ES3Data originData = GetComponent<ES3Data>(S3SO.e);

            Entities.WithAny<TCS3Data>().WithDisposeOnCompletion(allocated).ForEach((Entity entity, int entityInQueryIndex, in LocalToWorld localToWorld) =>
            {
                Entity spawned = ecbParallel.Instantiate(entityInQueryIndex, allocated[0]);

                //translation
                ecbParallel.SetComponent(entityInQueryIndex, spawned, new Translation
                {
                    Value = localToWorld.Position
                });

                //rotation
                ecbParallel.SetComponent(entityInQueryIndex, spawned, new Rotation
                {
                    Value = rndRot
                });

                //data
                ES3Data data = new ES3Data
                {
                    fallDirection = SpellManagerMB.Degrees2Quaternion(rndDir * ((entityInQueryIndex % 2 == 0) ? -1 : 1) + 180),
                    rotSpeed = originData.rotSpeed,
                    speed = originData.speed,
                };
                ecbParallel.SetComponent(entityInQueryIndex, spawned, data);
            }).ScheduleParallel();

            S3SO.eFire = false;
        }


        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation, in ES3Data eS3Data) =>
        {
            float3 forwardVec = SpellManagerMB.CalculateForward(eS3Data.fallDirection);
            translation.Value += forwardVec * eS3Data.speed * time;

            rotation.Value = math.mul(rotation.Value, SpellManagerMB.Degrees2Quaternion(eS3Data.rotSpeed * time));

            if (SpellManagerMB.IsInBarrier(translation.Value, customY: -8))
            {
                ecbParallel.DestroyEntity(entityInQueryIndex, entity);
            }
        }).ScheduleParallel();
    }

    [BurstCompile]
    struct SpawnC : IJobParallelFor
    {
        public Entity storm;
        public EntityCommandBuffer.ParallelWriter ecbParallel;
        public Entity c1;
        [NativeDisableParallelForRestriction] public NativeArray<Entity> stormCircles;
        public void Execute(int index)
        {
            Entity spawned = ecbParallel.Instantiate(index, c1);

            //set parent
            Parent parent = new Parent
            {
                Value = storm
            };
            LocalToParent localToParent = new LocalToParent { };
            ecbParallel.AddComponent<Parent>(index, spawned);
            ecbParallel.AddComponent<LocalToParent>(index, spawned);
            ecbParallel.SetComponent(index, spawned, parent);
            ecbParallel.SetComponent(index, spawned, localToParent);
            //set for destruction dependency
            stormCircles[index] = spawned;
            //ecbParallel.AppendToBuffer(index, spawned, linkedEntityGroup);
            /*
            DynamicBuffer<LinkedEntityGroup> linkedEntityGroup = ecbParallel.AddBuffer<LinkedEntityGroup>(index, storm);
            linkedEntityGroup.Add(spawned);
            */

            //translation
            ecbParallel.SetComponent(index, spawned, new Translation { });

            //rotation
            Rotation rotation = new Rotation
            {
                Value = SpellManagerMB.Degrees2Quaternion(index * (360 / S3SO.circlePerStorm))
            };
            ecbParallel.SetComponent(index, spawned, rotation);

            //prefab
            //ecbParallel.AddComponent<Prefab>(index, spawned);

            /*
            //set parent
            Parent parent = new Parent
            {
                Value = storms[stormNumber]
            };
            LocalToParent localToParent = new LocalToParent { };
            ecbParallel.SetComponent(index, spawned, parent);
            ecbParallel.SetComponent(index, spawned, localToParent);
            //set destruction dependency
            DynamicBuffer<LinkedEntityGroup> linkedEntityGroup = ecbParallel.AddBuffer<LinkedEntityGroup>(index, storms[stormNumber]);
            linkedEntityGroup.Add(spawned);

            //set rotation
            Rotation rotation = new Rotation
            {
                Value = SpellManagerMB.Degrees2Quaternion(circleRotPos)
            };
            ecbParallel.SetComponent(index, spawned, rotation);

            //set translation
            Translation translation = new Translation { };
            ecbParallel.SetComponent(index, spawned, translation);
            */
        }
    }
}