using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Random = Unity.Mathematics.Random;

[AlwaysUpdateSystem, DisableAutoCreation]
public class PlayerSystem : SystemBase
{
    //init and post-death entrance speed
    internal float transitionSpeed = 1.2f, invulnCountdownSpeed = 0.8f, recoil = 0f;

    //ecb for bullet-spawning
    internal BeginInitializationEntityCommandBufferSystem ecbSystem;

    //bullet spread

    //who else? lmao
    static internal Entity player;

    //bullet
    static internal Entity playerBullet;

    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        //instantiates player
        Entity prefab = InitializerMB.playerEntityPrefab;
        player = EntityManager.Instantiate(prefab);

        //assigns player bullet
        playerBullet = InitializerMB.playerBulletEntityPrefab;

        //creates a new translation data
        Translation newTranslation = new Translation { Value = new float3(0, -5.6f, 0) };

        //displaces player by setting component with new data
        EntityManager.SetComponentData(player, newTranslation);

        //initiates random data on bat
        Entities.ForEach((int entityInQueryIndex, ref BatData batData) => {
            batData.value = Random.CreateFromIndex((uint)entityInQueryIndex);
        }).ScheduleParallel();

        //get or create ecb initialization system
        ecbSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    internal void SetTest()
    {
        Translation newTranslation = new Translation { Value = new float3(0, -18f, 0) };
        EntityManager.SetComponentData(player, newTranslation);
    }

    protected override void OnUpdate()
    {

        //retrieves necessary component
        Translation playerTranslation = EntityManager.GetComponentData<Translation>(player);
        PlayerData playerData = EntityManager.GetComponentData<PlayerData>(player);
        PhysicsCollider physicsCollider = EntityManager.GetComponentData<PhysicsCollider>(player);

        //movement, including death movement
        if (playerData.isDead)
        {
            playerData.revivalTransition = math.clamp(playerData.revivalTransition + Time.DeltaTime * transitionSpeed, 0, 1);
            float2 floatTransition = math.lerp(playerData.respawnPos, playerData.entrancePos, playerData.revivalTransition);

            playerTranslation.Value = new float3 { x = floatTransition.x, y = floatTransition.y, z = 0 };

            if (playerData.revivalTransition >= 1)
            {
                playerData.isDead = false;
                playerData.revivalTransition = 0;
            }

        } else
        {
            float shift = Input.GetButton("Shift") ? playerData.shiftPercentage : 1;

            float x = Input.GetAxisRaw("Horizontal") * playerData.speed * shift * Time.DeltaTime;
            float y = Input.GetAxisRaw("Vertical") * playerData.speed * shift * Time.DeltaTime;
            playerTranslation.Value.x += x;
            playerTranslation.Value.y += y;

            if (!StageManagerMB.isTesting)
            {
                playerTranslation.Value.y = math.clamp(playerTranslation.Value.y, -4.65f, 4.65f);
                playerTranslation.Value.x = math.clamp(playerTranslation.Value.x, -3.45f, 3.45f);
            } else
            {
                playerTranslation.Value.y = math.clamp(playerTranslation.Value.y, -12f, -12f);
                playerTranslation.Value.x = math.clamp(playerTranslation.Value.x, 0f, 0f);
            }


        }

        //counts down invuln
        playerData.invulnTimer = math.clamp(playerData.invulnTimer - invulnCountdownSpeed * Time.DeltaTime, 0, 2);

        //sets data
        EntityManager.SetComponentData(player, playerData);
        EntityManager.SetComponentData(player, playerTranslation);

        //fire!
        bool fired = false;
        if (Input.GetButton("Fire1") && recoil == 0)
        {
            fired = true;

            //creates ecb instance
            EntityCommandBuffer.ParallelWriter ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();

            NativeArray<Entity> passedBulletAndPlayer = new NativeArray<Entity>(new Entity[] { playerBullet, player }, Allocator.TempJob);

            Entities.WithDisposeOnCompletion(passedBulletAndPlayer).WithAny<PlayerData, BatData>().ForEach((Entity entity, int entityInQueryIndex, in Translation translation) =>
            {
                //strictly tells the compiler that injected variables are read-only by creating a read-only native array
                NativeArray<Entity>.ReadOnly readBulletAndPlayer = passedBulletAndPlayer.AsReadOnly();

                //instantiates bullet
                Entity bullet = ecb.Instantiate(entityInQueryIndex, readBulletAndPlayer[0]);

                //retrieves player position
                Translation playerTranslation = GetComponent<Translation>(readBulletAndPlayer[1]);

                //calculates appropriate position
                float3 pos = playerTranslation.Value;

                if (readBulletAndPlayer[1]!=entity) //if spawns on bat
                {
                    //adjusts the position accordingly
                    LocalToParent localToParent = GetComponent<LocalToParent>(entity);
                    pos += localToParent.Position;

                    //retrieves bat data
                    BatData batData = GetComponent<BatData>(entity);

                    //generates random data
                    float direction = batData.value.NextFloat(-20, 20);

                    //creates rotation
                    Rotation rotation = new Rotation
                    {
                        Value = quaternion.RotateZ(math.radians(direction))
                    };

                    //sets the rotation of bullet
                    ecb.SetComponent(entityInQueryIndex, bullet, rotation);
                }

                //sets the position
                Translation spawnPoint = new Translation { Value = pos };
                ecb.SetComponent(entityInQueryIndex, bullet, spawnPoint);

            }).ScheduleParallel();

            //randomizes bat data if fired
            if (fired)
            {
                Entities.ForEach((ref BatData batData) => {
                    batData.value.NextBool();
                }).ScheduleParallel();

            }

            //recoils
            recoil = 0.06f;
        }

        recoil = math.clamp(recoil - Time.DeltaTime, 0, math.INFINITY);
    }

}
