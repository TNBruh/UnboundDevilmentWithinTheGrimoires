using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct TWS3Data : IComponentData
{
    public float initRot;
    public float endRot;

}
