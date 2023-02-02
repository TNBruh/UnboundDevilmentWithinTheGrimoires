using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Burst;


[UpdateAfter(typeof(StepPhysicsWorld)), DisableAutoCreation, AlwaysUpdateSystem]
public class CollisionSystem : EndFramePhysicsSystem
{
    StepPhysicsWorld simulation;
    BuildPhysicsWorld buildPhysicsWorld;
    EndSimulationEntityCommandBufferSystem ecb;

    //the player
    PlayerData playerData;


    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<FixedStepSimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        simulation = World.GetOrCreateSystem<StepPhysicsWorld>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        //npc data
        NPCData npcData = GetComponent<NPCData>(NPCSystem.NPC);

        //bullets marked for destruction due to collision
        NativeList<Entity> toDestroy = new NativeList<Entity>(Allocator.TempJob);

        //invulnerability indicator
        uint npcIsInvuln = (uint)(npcData.invuln ? 1 : 0);

        //npc hit count, second element indicates invulnerability
        NativeArray<uint> hitCount = new NativeArray<uint>(new uint[] { 0, npcIsInvuln }, Allocator.TempJob); //[0, 0]

        //detects collision and writes to the natives
        JobHandle collisionJob = new GlobalCollision
        {
            playerDataPool = GetComponentDataFromEntity<PlayerData>(true),
            bulletPool = GetComponentDataFromEntity<BulletTag>(true),
            npcPool = GetComponentDataFromEntity<NPCData>(true),
            playerBulletPool = GetComponentDataFromEntity<PlayerBulletData>(true),
            despawnerPool = GetComponentDataFromEntity<DespawnerTag>(true),
            entitiesToDestroy = toDestroy,
            hitCount = hitCount,
            ecb = ecb.CreateCommandBuffer(), //queues up action

        }.Schedule(simulation.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);

        //this must finish
        collisionJob.Complete();

        //queues destruction
        JobHandle destroyJob = new DestroyMarkedEntities
        {
            entitiesToDestroy = toDestroy.AsParallelReader(),
            ecb = ecb.CreateCommandBuffer().AsParallelWriter()
        }.Schedule(toDestroy.Length, (int)math.floor(toDestroy.Length / 4));

        destroyJob.Complete();

        //sets npc health at end frame
        NPCData newNPCData = new NPCData
        {
            health = (uint)math.clamp(npcData.health - hitCount[0], 0, math.INFINITY),
            invuln = npcData.invuln,
            maxHeath = npcData.maxHeath,
        };
        SetComponent(NPCSystem.NPC, newNPCData);

        //cleans natives
        Dependency = toDestroy.Dispose(destroyJob);
        Dependency = hitCount.Dispose(Dependency);
    }

    [BurstCompile]
    struct GlobalCollision : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<PlayerData> playerDataPool; //player pool
        [ReadOnly] public ComponentDataFromEntity<BulletTag> bulletPool; //bullet pool
        [ReadOnly] public ComponentDataFromEntity<NPCData> npcPool; //npc pool
        [ReadOnly] public ComponentDataFromEntity<PlayerBulletData> playerBulletPool; //player bullet pool
        [ReadOnly] public ComponentDataFromEntity<DespawnerTag> despawnerPool; //despawner pool
        public NativeList<Entity> entitiesToDestroy;
        public EntityCommandBuffer ecb;
        public NativeArray<uint> hitCount;
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            //player collision
            if (playerDataPool.HasComponent(entityA) && (bulletPool.HasComponent(entityB) || npcPool.HasComponent(entityB)))
            {

                PlayerData playerData = playerDataPool[entityA];

                if (playerData.invulnTimer > 0) return;

                //resets the player
                playerData.isDead = true;
                playerData.invulnTimer = 2;
                ecb.SetComponent(entityA, playerData);

                //return if it's NPC
                if (npcPool.HasComponent(entityB)) return;

                //destroys bullet
                entitiesToDestroy.Add(entityB);
                return;
                //ecb.DestroyEntity(entityB);

            } else if (playerDataPool.HasComponent(entityB) && (bulletPool.HasComponent(entityA) || npcPool.HasComponent(entityA)))
            {
                PlayerData playerData = playerDataPool[entityB];

                if (playerData.invulnTimer > 0) return;

                //resets the player
                playerData.isDead = true;
                playerData.invulnTimer = 2;
                ecb.SetComponent(entityB, playerData);

                //return if it's NPC
                if (npcPool.HasComponent(entityA)) return;

                //destroys bullet
                entitiesToDestroy.Add(entityA);
                return;
                //ecb.DestroyEntity(entityA);
            }

            //despawner collision
            if (despawnerPool.HasComponent(entityA) && (bulletPool.HasComponent(entityB) || playerBulletPool.HasComponent(entityB)))
            {
                entitiesToDestroy.Add(entityB); return;
            }
            else if (despawnerPool.HasComponent(entityB) && (bulletPool.HasComponent(entityA) || playerBulletPool.HasComponent(entityA)))
            {
                entitiesToDestroy.Add(entityA); return;
            }

            //NPC collision
            //returns if npc is invuln
            if (hitCount[1] == 1) return;
            if (npcPool.HasComponent(entityA) && playerBulletPool.HasComponent(entityB))
            {
                hitCount[0] += 1;

                entitiesToDestroy.Add(entityB); return;
            } else if (npcPool.HasComponent(entityB) && playerBulletPool.HasComponent(entityA))
            {
                hitCount[0] += 1;

                entitiesToDestroy.Add(entityA); return;
            }
        }
    }

    [BurstCompile]
    struct DestroyMarkedEntities : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Entity>.ReadOnly entitiesToDestroy;
        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(int index)
        {
            ecb.DestroyEntity(index, entitiesToDestroy[index]);
        }
    }
}
