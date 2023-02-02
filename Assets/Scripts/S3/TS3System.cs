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
using Random = Unity.Mathematics.Random;

[AlwaysUpdateSystem, DisableAutoCreation]
public class TS3System : SystemBase
{


    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        //reset into phase signal into true
        //instantiate throne and assign it to SO
        S3SO.tEntity = EntityManager.Instantiate(S3SO.t);

        //set it off screen
        SetComponent(S3SO.tEntity, S3SO.throneInitPos);

        //reset intro data
        S3SO.intro = true;
        S3SO.entranceProg = 0;
        S3SO.beginS3 = false;

        //get ts3 data for init config
        TS3Data tS3Data = GetComponent<TS3Data>(S3SO.tEntity);

        //prepare rand seeds
        Random rand0 = new Random { state = 69420 };
        Random rand1 = new Random { state = 80084 };

        //set rand seeds
        tS3Data.rand0 = rand0;
        tS3Data.rand1 = rand1;

        //set init config
        SetComponent(S3SO.tEntity, tS3Data);
    }

    protected override void OnUpdate()
    {
        if (!S3SO.beginS3) return;

        //delta time
        float time = Time.DeltaTime;

        //parallel buffer
        EntityCommandBuffer.ParallelWriter ecbParallel = S3SO.ecbS.CreateCommandBuffer().AsParallelWriter();

        //intro signal
        bool intro = S3SO.intro;

        //throne
        Entity tEntity = S3SO.tEntity;

        //npc
        Entity npc = NPCSystem.NPC;

        if (intro)
        {
            if (S3SO.entranceProg != 1)
            {
                //original spawn pos
                Translation spawnTranslation = S3SO.throneInitPos;

                //npc's current pos
                Translation npcTranslation = GetComponent<Translation>(npc);

                //continue entrance prog
                S3SO.entranceProg = math.clamp(S3SO.entranceProg + time * S3SO.entranceSpeed, 0, 1);

                //entrance pos
                Translation entranceTranslation = new Translation
                {
                    Value = math.lerp(spawnTranslation.Value, npcTranslation.Value, S3SO.entranceProg),
                };

                //set component
                SetComponent(tEntity, entranceTranslation);

            } else
            {
                //open bullet fire intro
                S3SO.tulipIntro = true; //process to next subphase
                S3SO.tulipCanFire = true; //enable bullet barrage
            }

        }

        

    }

    
}