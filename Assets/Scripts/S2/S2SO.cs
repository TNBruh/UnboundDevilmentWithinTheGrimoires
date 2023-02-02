using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;

public class S2SO : ScriptableObject
{
    static internal Entity npc;
    static internal Entity c1, c2, p;
    static internal bool c2Fire = false;
    static internal bool c1Fire = false;
    static internal bool freeze = false;
    static internal bool melt = false;
    static internal Entity[] icePrefabs;
    readonly static internal float[] lerpBound = { -60, 60 };
    readonly static internal float c2SpawnDist = 0.5f;
    readonly static internal uint iceCount = 8;
    readonly static internal uint iceFireCount = 1;
    readonly static internal float blossomRecoil = 0.12f;
    readonly static internal uint blossomFireCount = 2;
    readonly static internal float waitMelt = 1f;
    readonly static internal float waitFreeze = 7.2f;
    readonly static internal float waitIceCycle = 4f;
    readonly static internal float[] blossomClamp = { 3.61f, 4.81f };
    readonly static internal uint randomSeed = 69420;

    static internal bool allowCleanUp = false;

    static internal StepPhysicsWorld simulation;
    static internal BuildPhysicsWorld buildPhysicsWorld;
    static internal EndSimulationEntityCommandBufferSystem ecbS;
}
