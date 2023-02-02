using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public class InitializerMB : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [SerializeField]
    internal GameObject playerPrefab, npcPrefab, playerBulletPrefab, despawnerPrefab0, despawnerPrefab1, despawnerPrefab2, despawnerPrefab3;

    static internal Entity playerEntityPrefab, npcEntityPrefab, playerBulletEntityPrefab, despawnerEntityPrefab0, despawnerEntityPrefab1, despawnerEntityPrefab2, despawnerEntityPrefab3;

    //spell 1 gameobject prefabs
    [SerializeField]
    internal GameObject b1S1Prefab, b2S1Prefab, pheonixS1Prefab, c1S1Prefab, mc1S1Prefab;

    //spell 1 entity prefabs
    static internal Entity b1S1EntityPrefab, b2S1EntityPrefab, pheonixS1EntityPrefab, c1S1EntityPrefab, mc1S1EntityPrefab;

    //spell 2 gameobject prefabs
    [SerializeField]
    internal GameObject c1S2Prefab, c2S2Prefab, pS2Prefab;

    //spell 2 entity prefabs
    static internal Entity c1S2EntityPrefab, c2S2EntityPrefab, pS2EntityPrefab;

    //spell 3 gameobject prefabs
    [SerializeField]
    internal GameObject c1S3Prefab, b1S3Prefab, fS3Prefab, eS3Prefab, tS3Prefab;

    //spell 3 entity prefabs
    static internal Entity c1S3EntityPrefab, b1S3EntityPrefab, fS3EntityPrefab, eS3EntityPrefab, tS3EntityPrefab;

    internal EntityManager entityManager;

    static internal bool finished = false;

    //despawners
    internal static Entity despawner0, despawner1, despawner2, despawner3;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //converts gameobject to entity prefab
        //player
        playerEntityPrefab = conversionSystem.GetPrimaryEntity(playerPrefab);
        //player bullet
        playerBulletEntityPrefab = conversionSystem.GetPrimaryEntity(playerBulletPrefab);
        //despawner
        despawnerEntityPrefab0 = conversionSystem.GetPrimaryEntity(despawnerPrefab0);
        despawnerEntityPrefab1 = conversionSystem.GetPrimaryEntity(despawnerPrefab1);
        despawnerEntityPrefab2 = conversionSystem.GetPrimaryEntity(despawnerPrefab2);
        despawnerEntityPrefab3 = conversionSystem.GetPrimaryEntity(despawnerPrefab3);
        //NPC
        npcEntityPrefab = conversionSystem.GetPrimaryEntity(npcPrefab);

        //spell 1
        b1S1EntityPrefab = conversionSystem.GetPrimaryEntity(b1S1Prefab);
        b2S1EntityPrefab = conversionSystem.GetPrimaryEntity(b2S1Prefab);
        mc1S1EntityPrefab = conversionSystem.GetPrimaryEntity(mc1S1Prefab);
        c1S1EntityPrefab = conversionSystem.GetPrimaryEntity(c1S1Prefab);


        //spell 2
        c1S2EntityPrefab = conversionSystem.GetPrimaryEntity(c1S2Prefab);
        c2S2EntityPrefab = conversionSystem.GetPrimaryEntity(c2S2Prefab);
        pS2EntityPrefab = conversionSystem.GetPrimaryEntity(pS2Prefab);

        //spell 3
        c1S3EntityPrefab = conversionSystem.GetPrimaryEntity(c1S3Prefab);
        b1S3EntityPrefab = conversionSystem.GetPrimaryEntity(b1S3Prefab);
        fS3EntityPrefab = conversionSystem.GetPrimaryEntity(fS3Prefab);
        eS3EntityPrefab = conversionSystem.GetPrimaryEntity(eS3Prefab);
        tS3EntityPrefab = conversionSystem.GetPrimaryEntity(tS3Prefab);


        //set ready here
        finished = true;
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        //refers to gameobject prefab
        //player
        referencedPrefabs.Add(playerPrefab);
        //player bullet
        referencedPrefabs.Add(playerBulletPrefab);
        //despawner
        referencedPrefabs.Add(despawnerPrefab0);
        referencedPrefabs.Add(despawnerPrefab1);
        referencedPrefabs.Add(despawnerPrefab2);
        referencedPrefabs.Add(despawnerPrefab3);
        //NPC
        referencedPrefabs.Add(npcPrefab);

        //spell 1
        referencedPrefabs.Add(b1S1Prefab);
        referencedPrefabs.Add(b2S1Prefab);
        referencedPrefabs.Add(mc1S1Prefab);
        referencedPrefabs.Add(c1S1Prefab);
        //pheonix here

        //spell 2
        referencedPrefabs.Add(c1S2Prefab);
        referencedPrefabs.Add(c2S2Prefab);
        referencedPrefabs.Add(pS2Prefab);

        //spell 3
        referencedPrefabs.Add(c1S3Prefab);
        referencedPrefabs.Add(b1S3Prefab);
        referencedPrefabs.Add(fS3Prefab);
        referencedPrefabs.Add(eS3Prefab);
        referencedPrefabs.Add(tS3Prefab);

    }
}
