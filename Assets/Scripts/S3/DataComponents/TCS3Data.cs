using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct TCS3Data : IComponentData
{
    public float initRot;
    public float wavePhase;
    public float initWavePhase;
    public float waveSpeed;
}
