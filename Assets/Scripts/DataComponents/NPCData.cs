using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct NPCData : IComponentData
{
    public uint health;
    public bool invuln;
    public uint maxHeath;
}
