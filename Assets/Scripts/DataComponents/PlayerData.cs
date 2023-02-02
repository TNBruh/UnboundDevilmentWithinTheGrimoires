using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerData : IComponentData
{
    public bool isDead;// = true;
    public float revivalTransition;// = 0;
    public float2 respawnPos;// = new float2(0, -5.6f);
    public float2 entrancePos;// = new float2(0, -3.5f);
    public float invulnTimer;// = 2f;
    public float speed;// = 20f;
    public float shiftPercentage;// = 0.36f;
}
