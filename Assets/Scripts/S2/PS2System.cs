using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

[AlwaysUpdateSystem, DisableAutoCreation]
public class PS2System : SystemBase
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

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation, ref PS2Data pS2Data) =>
        {
            //rotate
            //process lerp
            pS2Data.phaseDegree = (pS2Data.phaseDegree + time * pS2Data.turnSpeed) % (2 * math.PI);
            float lerpProg = (math.sin(pS2Data.phaseDegree) + 1) / 2;
            float lerpRotEuler = math.lerp(S2SO.lerpBound[0], S2SO.lerpBound[1], lerpProg);
            rotation.Value = SpellManagerMB.Degrees2Quaternion(lerpRotEuler + pS2Data.originalRot);

            //move
            float3 fowardVec = SpellManagerMB.CalculateForward(rotation.Value);
            translation.Value += fowardVec * pS2Data.speed * time;

        }).ScheduleParallel();
    }
}
