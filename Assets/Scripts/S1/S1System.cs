
/*
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

[AlwaysUpdateSystem, DisableAutoCreation]
public class S1System : SystemBase
{
    Entity npc;
    Entity b1, b2, c1, mc1; //remember add pheonix
    float eulerRot = 0;
    float rotSpeed = 20f;

    float mcRecoil = 0.1f;

    readonly int[] shootAngle = { 65, 115, -65, -115 };

    static internal bool fireMagicCircle = false;

    StepPhysicsWorld simulation;
    BuildPhysicsWorld buildPhysicsWorld;
    EndSimulationEntityCommandBufferSystem ecbS;

    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        npc = NPCSystem.NPC;
        b1 = InitializerMB.b1S1EntityPrefab;
        b2 = InitializerMB.b2S1EntityPrefab;
        c1 = InitializerMB.c1S1EntityPrefab;
        mc1 = InitializerMB.mc1S1EntityPrefab;
        //pheonix here

        simulation = World.GetOrCreateSystem<StepPhysicsWorld>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        ecbS = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        //delta time
        float time = Time.DeltaTime;

        //ecb parallel writer
        EntityCommandBuffer.ParallelWriter ecb = ecbS.CreateCommandBuffer().AsParallelWriter();

        //ecb single writer
        EntityCommandBuffer ecbSingle = ecbS.CreateCommandBuffer();

        //native arrays
        NativeArray<Entity> prefabEntities = new NativeArray<Entity>(new Entity[5] { b1, b2, c1, mc1, b1 }, Allocator.TempJob);
        NativeArray<Entity> prefabEntities1 = new NativeArray<Entity>(new Entity[5] { b1, b2, c1, mc1, b1 }, Allocator.TempJob);

        //global magic circle spin
        eulerRot = (eulerRot + rotSpeed * Time.DeltaTime) % 360;
        float currentEulerRot = eulerRot;
        Rotation mc1Rotation = new Rotation
        {
            Value = quaternion.RotateZ(math.radians(eulerRot))
        };

        //global magic circle recoil
        mcRecoil -= time;
        bool mcCanFire = mcRecoil <= 0;

        //processing magic circle
        Entities.WithDisposeOnCompletion(prefabEntities).ForEach((Entity entity, int entityInQueryIndex, ref Rotation rotation, ref Translation translation, in MC1S1Data mc1S1Data) => {

            //destroy if too far
            float absX = math.abs(translation.Value.x);
            float absY = math.abs(translation.Value.y);
            if (absX >= 4.5 || absY >= 6)
            {
                ecb.DestroyEntity(entityInQueryIndex, entity);
            }

            //fire
            bool canFire = SpellManagerMB.IsInBarrier(translation.Value) && mcCanFire;
            if (canFire)
            {

                for (int i = 0; i < 4; i++)
                {
                    float bulletRotDegree = i * 90;

                    quaternion bulletQuat = SpellManagerMB.RotateByEuler(rotation.Value, bulletRotDegree);

                    Rotation bulletRot = new Rotation
                    {
                        Value = bulletQuat
                    };

                    Entity spawnedBullet = ecb.Instantiate(entityInQueryIndex, prefabEntities[0]);
                    ecb.SetComponent(entityInQueryIndex, spawnedBullet, translation);
                    ecb.SetComponent(entityInQueryIndex, spawnedBullet, bulletRot);
                }
            }

            //rotating the magic circles
            rotation.Value = SpellManagerMB.Degrees2Quaternion(currentEulerRot);

            //moving the magic circles, ignoring its current real rotation
            //calculates the path vector
            float3 forwardVec = SpellManagerMB.CalculateForward(mc1S1Data.direction.Value);

            //moves magic circle
            translation.Value = forwardVec * time * mc1S1Data.speed;

        }).ScheduleParallel();

        //processing bullets
        //red, bullet 1
        Entities.WithDisposeOnCompletion(prefabEntities1).ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in Rotation rotation, in B1S1Data b1S1Data) => {
            //calculates the path vector
            float3 forwardVec = SpellManagerMB.CalculateForward(rotation.Value);

            //moves bullet
            translation.Value = forwardVec * time * b1S1Data.speed;

            //sets for destruction
            float absX = math.abs(translation.Value.x); //3.5
            float absY = math.abs(translation.Value.y); //4.6

            //bounce
            if (absX >= 3.5 || absY >= 4.6)
            {
                //destroy the bullet
                ecb.DestroyEntity(entityInQueryIndex, entity);

                //instantiate the bounced bullet
                Entity bounced = ecb.Instantiate(entityInQueryIndex, prefabEntities1[1]);

                //create a flipped rotation
                Rotation bouncedRot = new Rotation
                {
                    Value = math.mul(rotation.Value, quaternion.RotateZ(math.radians(180)))
                };

                //sets rotation and translation respectively
                ecb.SetComponent(entityInQueryIndex, bounced, bouncedRot);
                ecb.SetComponent(entityInQueryIndex, bounced, translation);
            }
        }).ScheduleParallel();
        //blue, bullet 2
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, in Rotation rotation, in B2S1Data b2S1Data) => {
            //calculates the path vector
            float3 forwardVec = SpellManagerMB.CalculateForward(rotation.Value);

            //moves bullet
            translation.Value = forwardVec * time * b2S1Data.speed;
        }).ScheduleParallel();

        //processing pheonix

        //processing firing calls
        if (fireMagicCircle)
        {
            Translation npcTranslation = GetComponent<Translation>(npc);

            for (int i = 0; i < 4; i++)
            {
                //queues to spawn
                Entity spawnedMC = ecbSingle.Instantiate(mc1);

                //sets translation
                ecbSingle.SetComponent(spawnedMC, npcTranslation);

                //sets direction
                MC1S1Data mc1S1Data = GetComponent<MC1S1Data>(spawnedMC);
                mc1S1Data.direction = new Rotation
                {
                    Value = SpellManagerMB.Degrees2Quaternion(shootAngle[i])
                };
                ecbSingle.SetComponent(spawnedMC, mc1S1Data);

                //sets rotation
                Rotation rotation = new Rotation
                {
                    Value = SpellManagerMB.Degrees2Quaternion(currentEulerRot)
                };
                ecbSingle.SetComponent(spawnedMC, rotation);

            }
            fireMagicCircle = false;
        }

        //resets magic circle recoil
        if (mcRecoil <= 0)
        {
            mcRecoil = 0.1f;
        }

    }
}
*/