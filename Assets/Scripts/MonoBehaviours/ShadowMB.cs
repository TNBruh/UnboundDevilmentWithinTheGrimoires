using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UI;


public class ShadowMB : MonoBehaviour
{
    [SerializeField] float3[] berzierPath;
    [SerializeField] float progSpeed = 1.2f;
    float maxProg = 0.5f;
    internal float prog = 0;

    private void OnEnable()
    {
        //movement
        gameObject.transform.position = berzierPath[0];
        maxProg = 0.5f;
        prog = 0;

        //transparency
    }

    private void FixedUpdate()
    {
        //movement
        prog = math.clamp(prog + progSpeed * Time.deltaTime, 0, maxProg);
        gameObject.transform.position = ExtMathMB.QuadBerzierCurve(berzierPath[0], berzierPath[1], berzierPath[2], prog);

        //transparency
    }

    internal void NextPath()
    {
        maxProg = 1;
    }

    private void OnDisable()
    {
        //movement
        gameObject.transform.position = berzierPath[0];
        maxProg = 0.5f;
        prog = 0;
    }
}
