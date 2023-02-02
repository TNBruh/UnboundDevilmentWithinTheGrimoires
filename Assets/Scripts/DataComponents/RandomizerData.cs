using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

[GenerateAuthoringComponent]
public struct RandomizerData : IComponentData
{
    public Random value;
}
