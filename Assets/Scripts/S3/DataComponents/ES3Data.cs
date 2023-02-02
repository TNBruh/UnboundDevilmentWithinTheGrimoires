using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ES3Data : IComponentData
{
    public quaternion fallDirection;
    public float rotSpeed;
    public float speed;
}
