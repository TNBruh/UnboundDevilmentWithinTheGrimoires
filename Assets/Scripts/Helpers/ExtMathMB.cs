using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

public class ExtMathMB : MonoBehaviour
{
    static internal float3 QuadBerzierCurve(float3 a, float3 b, float3 c, float prog)
    {
        //clamp
        prog = math.clamp(prog, 0.0f, 1.0f);
        float antiProg = 1 - prog;

        //P1 + (1-t)^2(P0-P1) + t^2(P2-P1)
        float3 result = b + math.pow(antiProg, 2) * (a - b) + math.pow(prog, 2) * (c - b);

        return result;
    }
}
