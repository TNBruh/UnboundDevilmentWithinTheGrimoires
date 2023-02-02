using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct FS3Data : IComponentData
{
    public quaternion fallDirection;
    public float speed;
    public float rotSpeed;
}
