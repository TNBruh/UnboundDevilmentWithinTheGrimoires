using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Entities;

public class SpellManagerMB : MonoBehaviour
{
    readonly static float3 up = new float3(0, 1, 0);

    internal Coroutine Spell1API()
    {
        S1SO.allowCleanUp = false;
        return StartCoroutine(Spell1());
    }

    internal Coroutine Spell2API()
    {
        S2SO.allowCleanUp = false;
        return StartCoroutine(Spell2());
    }

    internal Coroutine Spell3API()
    {
        S3SO.allowCleanUp = false;
        return StartCoroutine(Spell3());
    }

    IEnumerator Spell1()
    {
        Coroutine magicCircleCycle = StartCoroutine(SpawnMagicCircleS1());
        if (!StageManagerMB.isTesting)
        {
            yield return new WaitUntil(() => NPCSystem.healthRingImage.fillAmount == 0);
        } else
        {
            yield return new WaitForSeconds(StageManagerMB.spellTestLength);
        }
        StopCoroutine(magicCircleCycle);
        yield return new WaitForEndOfFrame();
        StageManagerMB.b1S1System.Enabled = false;
        StageManagerMB.b2S1System.Enabled = false;
        StageManagerMB.mc1S1System.Enabled = false;
        yield return new WaitForEndOfFrame();
        S1SO.allowCleanUp = true;
        yield return new WaitWhile(() => S1SO.allowCleanUp);
        yield return new WaitForEndOfFrame();
        StageManagerMB.cleanUpS1System.Enabled = false;
    }

    IEnumerator SpawnMagicCircleS1()
    {
        while (true)
        {
            //randomizes the next movement
            float3 nextPos = RandomNPCPosition();
            NPCSystem.SetMovement(nextPos);

            //waits until lerp finishes
            yield return new WaitUntil(() => NPCSystem.lerpProg == 1);
            yield return new WaitForFixedUpdate();

            //enable fire
            S1SO.fireMagicCircle = true;
            

            //recoil
            yield return new WaitForSeconds(4f);
        }
    }

    IEnumerator Spell2()
    {
        Coroutine spellCycle = StartCoroutine(Spell2Cycle());
        if (!StageManagerMB.isTesting)
        {
            yield return new WaitUntil(() => NPCSystem.healthRingImage.fillAmount == 0);
        } else
        {
            yield return new WaitForSeconds(StageManagerMB.spellTestLength);
        }
        StopCoroutine(spellCycle);
        yield return new WaitForEndOfFrame();
        StageManagerMB.c1S2System.Enabled = false;
        StageManagerMB.c2S2System.Enabled = false;
        StageManagerMB.pS2System.Enabled = false;
        yield return new WaitForEndOfFrame();
        S2SO.allowCleanUp = true;
        yield return new WaitWhile(() => S2SO.allowCleanUp);
        yield return new WaitForEndOfFrame();

    }

    IEnumerator Spell2Cycle()
    {
        while (true)
        {
            yield return StartCoroutine(IceFireCycle());
            yield return new WaitForSeconds(S2SO.waitMelt);
            S2SO.melt = true;
            yield return new WaitWhile(() => S2SO.melt);
            yield return new WaitForSeconds(S2SO.waitFreeze);
            S2SO.freeze = true;
            yield return new WaitWhile(() => S2SO.freeze);
        }
    }

    IEnumerator IceFireCycle()
    {
        Coroutine blossomCycle = StartCoroutine(BlossomSpawnCycle());
        for (int i = 0; i < S2SO.iceFireCount; i++)
        {
            //randomizes next movement
            float3 nextPos = RandomNPCPosition(bottom: 1.4f);
            NPCSystem.SetMovement(nextPos);

            //waits until lerp finishes
            yield return new WaitUntil(() => NPCSystem.lerpProg == 1);
            yield return new WaitForFixedUpdate();

            //fire
            S2SO.c1Fire = true;
            //recoil
            yield return new WaitForSeconds(S2SO.waitIceCycle);
        }
        StopCoroutine(blossomCycle);
    }

    IEnumerator BlossomSpawnCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(S2SO.blossomRecoil);
            S2SO.c2Fire = true;
        }
    }

    IEnumerator Spell3()
    {
        Coroutine tulipCycle = StartCoroutine(B1S3Cycle());
        Coroutine petalCycle = StartCoroutine(FS3Cycle());
        Coroutine stormCycle = StartCoroutine(StormS3Cycle());
        yield return StartCoroutine(BeginIntro());
        if (!StageManagerMB.isTesting)
        {
            yield return new WaitUntil(() => NPCSystem.healthRingImage.fillAmount == 0);
        } else
        {
            yield return new WaitForSeconds(StageManagerMB.spellTestLength);
        }
        yield return new WaitForEndOfFrame();
        //disable systems
        StageManagerMB.b1S3System.Enabled = false;
        StageManagerMB.c1S3System.Enabled = false;
        StageManagerMB.eS3System.Enabled = false;
        StageManagerMB.fS3System.Enabled = false;
        StageManagerMB.tcS3System.Enabled = false;
        StageManagerMB.tS3System.Enabled = false;
        StageManagerMB.twS3System.Enabled = false;
        yield return new WaitForEndOfFrame();
        S3SO.allowCleanUp = true;
        yield return new WaitWhile(() => S3SO.allowCleanUp);
        yield return new WaitForEndOfFrame();
        StageManagerMB.cleanUpS3System.Enabled = false;
    }

    IEnumerator BeginIntro()
    {
        NPCSystem.SetMovement(new float3
        {
            x = 0,
            y = 2.5f,
            z = 0,
        });
        yield return new WaitUntil(() => NPCSystem.lerpProg == 1);
        S3SO.beginS3 = true;
    }

    IEnumerator B1S3Cycle()
    {
        while (true)
        {
            S3SO.tulipFire = true;
            yield return new WaitForSeconds(S3SO.tulipRecoil);
        }
    }

    IEnumerator FS3Cycle()
    {
        yield return new WaitWhile(() => S3SO.intro);
        while (true)
        {
            S3SO.petalFire = true;
            yield return new WaitForSeconds(S3SO.petalRecoil);
        }
    }

    IEnumerator StormS3Cycle()
    {
        yield return new WaitWhile(() => S3SO.intro);
        while (true)
        {
            S3SO.eFire = true;
            yield return new WaitForSeconds(S3SO.eRecoil);
        }
    }

    internal static bool IsInBarrier(float3 loc, float customX = 3.5f, float customY = 4.6f)
    {
        float absX = math.abs(loc.x);
        float absY = math.abs(loc.y);

        if (absX >= customX || absY >= customY)
        {
            return false;
        }
        return true;
    }

    internal static float3 CalculateForward(quaternion direction)
    {
        return math.mul(direction, up);
    }

    internal static quaternion Degrees2Quaternion(float degrees)
    {
        return quaternion.RotateZ(math.radians(degrees));
    }

    internal static quaternion RotateByEuler(quaternion rot, float degrees)
    {
        return math.mul(rot, Degrees2Quaternion(degrees));
    }

    internal static float3 RandomNPCPosition(float left = -2f, float right = 2f, float bottom = 0, float top = 3)
    {
        return new float3
        {
            x = UnityEngine.Random.Range(left, right),
            y = UnityEngine.Random.Range(bottom, top),
            z = 0
        };
    }
}
