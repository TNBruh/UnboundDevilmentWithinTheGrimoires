using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent, System.Serializable]
public struct PlayerBulletData : IComponentData
{
    public float speed;
}
