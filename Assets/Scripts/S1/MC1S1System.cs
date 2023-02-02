using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Burst;

[AlwaysUpdateSystem, DisableAutoCreation]
public class MC1S1System : SystemBase
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

        //ecb parallel writer
        EntityCommandBuffer.ParallelWriter ecbParallel = S1SO.ecbS.CreateCommandBuffer().AsParallelWriter();

        //allocated entities for jobs
        NativeArray<Entity> allocatedEntities = new NativeArray<Entity>(new Entity[1] { S1SO.b1 }, Allocator.TempJob);

        //global magic circle spin
        S1SO.eulerRot = (S1SO.eulerRot + S1SO.rotSpeed * time) % 360;
        Rotation mc1Rotation = new Rotation
        {
            Value = SpellManagerMB.Degrees2Quaternion(S1SO.eulerRot)
        };

        //global magic circle recoil
        S1SO.mcRecoil -= time;
        bool canFire = S1SO.mcRecoil <= 0;

        //required
        Entity npcRead = S1SO.npc;
        float2 mcLimitRead = S1SO.magicCircleLimit;
        Entity mc1Read = S1SO.mc1;


        //spawn magic circle
        if (S1SO.fireMagicCircle)
        {
            for (int i = 0; i < 4; i++)
            {
                Dependency = new SpawnMC
                {
                    mc = mc1Read,
                    spawnTranslation = GetComponent<Translation>(npcRead),
                    ecbParallel = ecbParallel

                }.Schedule(4, 1, Dependency);
            }
            S1SO.fireMagicCircle = false;
        }

        //process magic circle
        Entities.WithDisposeOnCompletion(allocatedEntities).ForEach((Entity entity, int entityInQueryIndex, ref Rotation rotation, ref Translation translation, in MC1S1Data mc1S1Data) =>
        {
            //mark as read-only
            NativeArray<Entity>.ReadOnly allocated = allocatedEntities.AsReadOnly();

            //destroy if too far
            float3 absPos = math.abs(translation.Value);
            bool destroyCircle = (absPos.x >= mcLimitRead.x) || (absPos.y >= mcLimitRead.y);
            if (destroyCircle)
            {
                ecbParallel.DestroyEntity(entityInQueryIndex, entity);
            }

            //fire
            bool fire = canFire && SpellManagerMB.IsInBarrier(translation.Value);
            if (fire)
            {
                for (int i = 0; i < 4; i++)
                {
                    float bulletRotDegree = i * 90;

                    quaternion bulletQuat = SpellManagerMB.RotateByEuler(rotation.Value, bulletRotDegree);

                    Rotation bulletRot = new Rotation
                    {
                        Value = bulletQuat
                    };

                    Entity spawnedBullet = ecbParallel.Instantiate(entityInQueryIndex, allocated[0]);

                    ecbParallel.SetComponent(entityInQueryIndex, spawnedBullet, translation);
                    ecbParallel.SetComponent(entityInQueryIndex, spawnedBullet, bulletRot);
                }
            }

            //rotate magic circles
            rotation = mc1Rotation;

            //move magic circles
            float3 forwardVec = SpellManagerMB.CalculateForward(mc1S1Data.direction.Value);
            translation.Value += forwardVec * time * mc1S1Data.speed;
        }).ScheduleParallel();

        //resets global magic circle recoil
        S1SO.mcRecoil = canFire ? S1SO.mcMaxRecoil : S1SO.mcRecoil;
    }

    [BurstCompile]
    struct SpawnMC : IJobParallelFor
    {
        public Entity mc;
        public Translation spawnTranslation;
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        public void Execute(int index)
        {
            //spawn
            Entity spawned = ecbParallel.Instantiate(index, mc);

            //set translation
            ecbParallel.SetComponent(index, spawned, spawnTranslation);

            //set data
            MC1S1Data data = new MC1S1Data
            {
                direction = new Rotation
                {
                    Value = SpellManagerMB.Degrees2Quaternion(S1SO.mcAngle[index]),
                }, speed = 2f,
            };
            ecbParallel.SetComponent(index, spawned, data);
        }
    }
}
