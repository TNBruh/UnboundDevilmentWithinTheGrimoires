using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

[GenerateAuthoringComponent]
public struct MC1S1Data : IComponentData
{
    public Rotation direction;
    public float speed;
}
