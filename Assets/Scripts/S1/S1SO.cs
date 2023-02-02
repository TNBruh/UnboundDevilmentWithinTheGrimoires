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


public class S1SO : ScriptableObject
{
    static internal Entity npc;
    static internal Entity b1, b2, c1, mc1, p;
    static internal float eulerRot = 0;
    static internal float mcRecoil = 0.24f;
    readonly static internal float mcMaxRecoil = 0.24f;
    static internal readonly int[] mcAngle = { 65, 115, -65, -115 };
    static internal readonly float rotSpeed = 50f;
    static internal bool fireMagicCircle = false;
    static internal float2 magicCircleLimit = new float2
    {
        x = 4.5f,
        y = 6f
    };
    static internal bool allowCleanUp = false;

    static internal StepPhysicsWorld simulation;
    static internal BuildPhysicsWorld buildPhysicsWorld;
    static internal EndSimulationEntityCommandBufferSystem ecbS;
}
