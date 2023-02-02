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


public class S3SO : ScriptableObject
{
    static internal Entity npc;
    static internal Entity c1, b1, f, e, t;
    static internal Entity tEntity;
    static internal readonly float circleInitPhase = math.PI / 2;


    //intro
    static internal bool intro = true;

    //throne
    static internal readonly Translation throneInitPos = new Translation
    {
        Value = new float3(0, 9, 0)
    };
    static internal float entranceProg = 0;
    static internal float entranceSpeed = 1f;

    //tulip and tulip wing
    static internal bool tulipFire = false;
    static internal bool tulipIntro = false; //false = entrance subphase, true = open fire subphase
    static internal bool tulipCanFire = false;
    static internal float wingProg = 0;
    static internal float wingProgSpeed = 1;
    static internal readonly float waveDeviation = 3.8f;
    static internal readonly float tulipRecoil = 0.04f;

    //sakura petal
    static internal readonly float3[] petalSpawnPos = new float3[2]
    {
        new float3(-3.6f, 5f, 0),
        new float3(3.6f, 6f, 0)
    };
    static internal readonly uint petalSpawnerCount = 6;
    static internal Entity[] petalPrefabs;
    static internal bool petalFire = false;
    static internal readonly float[] petalSpeedRange = new float[2] { 1, 2.1f };
    static internal readonly float[] petalRotSpeedRange = new float[2] { 1, 40 };
    static internal readonly float petalRecoil = 0.45f;

    //s3 begins
    static internal bool beginS3 = false;

    //storm's eye
    static internal readonly float c1TimeToFloat = 4f;
    static internal readonly uint stormWorkerCount = 72;
    static internal readonly uint totalCirclePerShot = 32;
    static internal readonly uint stormCount = 4;
    static internal readonly float circlePerStorm = totalCirclePerShot / stormCount;
    static internal readonly float circleRotDeviation = 360 / totalCirclePerShot;
    static internal readonly float stormFallDeviation = 20;
    static internal readonly float[] stormFallSpeed = new float[2] { 1f, 2f };
    static internal readonly float[] stormRotSpeed = new float[2] { 20f, 60f };
    static internal bool eFire = false;
    static internal readonly float eRecoil = 6.8f;
    static internal Entity[] stormCirclePrefabs;
    static internal readonly int randomSeed = 69420;

    static internal bool allowCleanUp = false;

    static internal StepPhysicsWorld simulation;
    static internal BuildPhysicsWorld buildPhysicsWorld;
    static internal EndSimulationEntityCommandBufferSystem ecbS;
}
