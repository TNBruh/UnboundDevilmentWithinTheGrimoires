using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Collections;

[UpdateAfter(typeof(StepPhysicsWorld)), DisableAutoCreation]
public class DespawnerCSystem : EndFramePhysicsSystem
{
    StepPhysicsWorld simulation;
    BuildPhysicsWorld buildPhysicsWorld;
    EndSimulationEntityCommandBufferSystem ecb;

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

        //creates barriers
        EntityManager.Instantiate(InitializerMB.despawnerEntityPrefab0);
        EntityManager.Instantiate(InitializerMB.despawnerEntityPrefab1);
        EntityManager.Instantiate(InitializerMB.despawnerEntityPrefab2);
        EntityManager.Instantiate(InitializerMB.despawnerEntityPrefab3);
    }

    protected override void OnUpdate()
    {
        //bullets to destroy
        NativeList<Entity> toDestroy = new NativeList<Entity>(Allocator.TempJob);

        //detects collision
        JobHandle collisionJob = new DespawnerCollision
        {
            despawnerPool = GetComponentDataFromEntity<DespawnerTag>(true),
            bulletPool = GetComponentDataFromEntity<BulletTag>(true),
            playerBulletPool = GetComponentDataFromEntity<PlayerBulletData>(true),
            entitiesToDestroy = toDestroy
        }.Schedule(simulation.Simulation, ref buildPhysicsWorld.PhysicsWorld, Dependency);

        //finishes collision detection
        collisionJob.Complete();

        //destroys marked bullets
        JobHandle destroyJob = new DestroyMarkedEntities
        {
            entitiesToDestroy = toDestroy.AsParallelReader(),
            ecb = ecb.CreateCommandBuffer().AsParallelWriter(),
        }.Schedule(toDestroy.Length, 16);

        destroyJob.Complete();

        //cleans native array
        Dependency = toDestroy.Dispose(destroyJob);
    }

    struct DespawnerCollision : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<DespawnerTag> despawnerPool;
        [ReadOnly] public ComponentDataFromEntity<BulletTag> bulletPool;
        [ReadOnly] public ComponentDataFromEntity<PlayerBulletData> playerBulletPool;
        public NativeList<Entity> entitiesToDestroy;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            if (despawnerPool.HasComponent(entityA) && (bulletPool.HasComponent(entityB) || playerBulletPool.HasComponent(entityB)))
            {
                entitiesToDestroy.Add(entityB);
            } else if (despawnerPool.HasComponent(entityB) && (bulletPool.HasComponent(entityA) || playerBulletPool.HasComponent(entityA)))
            {
                entitiesToDestroy.Add(entityA);
            }
        }
    }

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
