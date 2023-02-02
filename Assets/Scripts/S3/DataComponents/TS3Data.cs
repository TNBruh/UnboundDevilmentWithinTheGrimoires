using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;
using Unity.Collections;

[GenerateAuthoringComponent]
public struct TS3Data : IComponentData
{
    public float3 bulletSpawn0;
    public float3 bulletSpawn1;
    public float3 bulletSpawn2;
    public float3 bulletSpawn3;

    public Random rand0;
    public Random rand1;

    public Entity npc;
}
