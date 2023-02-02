using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

[AlwaysUpdateSystem, DisableAutoCreation]
public class NPCSystem : SystemBase
{
    GameObject healthRing;
    internal static Image healthRingImage;
    internal static Entity NPC;
    internal static RandomizerData npcRandomData;
    internal static float lerpSpeed = 1.2f;
    internal static float lerpProg = 0;
    internal static float3[] lerpPos = new float3[]
    {
        new float3(0, 7, 0),
        new float3(0, 7, 0),
    };
    internal static bool resetHealth = false;
    internal static bool switchOffInvuln = false;

    protected override void OnCreate()
    {
        //enables auto-update by inserting it into the player loop
        World.GetExistingSystem<SimulationSystemGroup>().AddSystemToUpdateList(this);
    }

    protected override void OnStartRunning()
    {
        //creates NPC
        NPC = EntityManager.Instantiate(InitializerMB.npcEntityPrefab);

        //reset lerpPos
        lerpPos = new float3[]
        {
            new float3(0, 7, 0),
            new float3(0, 7, 0),
        };

        //changes NPC position
        Translation translation = new Translation
        {
            Value = new float3(0, 7, 0)
        };
        EntityManager.SetComponentData(NPC, translation);

        healthRing = GameObject.FindGameObjectWithTag("HealthRing");
        healthRingImage = healthRing.GetComponent(typeof(Image)) as Image;

        healthRing.SetActive(true);

        npcRandomData = new RandomizerData
        {
            value = Random.CreateFromIndex(69)
        };
        EntityManager.SetComponentData(NPC, npcRandomData);
    }

    protected override void OnUpdate()
    {

        NPCData npcData = GetComponent<NPCData>(NPC);

        Translation translation = new Translation { Value = math.lerp(lerpPos[0], lerpPos[1], lerpProg) };
        SetComponent(NPC, translation);

        healthRing.transform.position = translation.Value;
        healthRingImage.fillAmount = (float)npcData.health / npcData.maxHeath;

        lerpProg = math.clamp(lerpProg + Time.DeltaTime * lerpSpeed, 0, 1);

        if (resetHealth)
        {
            //reset and invuln
            npcData.health = math.clamp(npcData.health+15, 0, npcData.maxHeath);
            npcData.invuln = true;
            SetComponent(NPC, npcData);

            if (npcData.health == npcData.maxHeath)
            {
                resetHealth = false;
            }
        } else if (switchOffInvuln)
        {
            npcData.invuln = false;
            SetComponent(NPC, npcData);

            switchOffInvuln = false;
        }
    }

    internal static float3[] SetMovement(float3 start, float3 end)
    {
        lerpProg = 0;
        lerpPos[0] = start;
        lerpPos[1] = end;
        return lerpPos;
    }

    internal static float3[] SetMovement(float3 end)
    {
        lerpProg = 0;
        lerpPos[0] = lerpPos[1];
        lerpPos[1] = end;
        return lerpPos;
    }

    internal static void ResetAndInvuln()
    {
        resetHealth = true;
    }

    internal static void SwitchOffInvuln()
    {
        switchOffInvuln = true;
    }

    internal void CleanUp()
    {
        //healthRing.SetActive(false);
        EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer();
        ecb.DestroyEntity(NPC);

        World.DestroySystem(this);

    }
}
