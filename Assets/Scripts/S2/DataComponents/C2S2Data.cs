using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct C2S2Data : IComponentData
{
    public float speed;
    public bool frozen;
}
