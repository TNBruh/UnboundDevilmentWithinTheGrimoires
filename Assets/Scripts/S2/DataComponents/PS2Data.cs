using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PS2Data : IComponentData
{
    public float phaseDegree;
    public float speed;
    public float turnSpeed;
    public float originalRot;
}
