using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.UI;
using System;
using Unity.Mathematics;
using Unity.Physics.Systems;

public class StageManagerMB : MonoBehaviour
{
    [SerializeField]
    internal GameObject initializer;
    static internal int spellPhase = 0;

    internal static readonly bool isTesting = true;
    public static readonly float spellTestLength = 30;


    //loading layout
    GameObject loadingLayout;

    //pause canvas
    [SerializeField]
    GameObject pauseCanvasPrefab;
    GameObject pauseCanvas;

    //game manager
    GameManagerMB gameManagerMB;

    //continue button
    Button continueButton;

    //quit button
    Button quitButton;

    //spell manager
    [SerializeField]
    GameObject spellManagerPrefab;
    GameObject spellManager;
    SpellManagerMB spellManagerMB;

    //character shadows
    GameObject mokouS, cirnoS, byakurenS;
    ShadowMB mokouSMB, cirnoSMB, byakurenSMB;

    //systems
    static internal PlayerSystem playerSystem;
    static internal PlayerBulletSystem playerBulletSystem;
    static internal NPCSystem npcSystem;
    static internal CollisionSystem collisionSystem;

    //S1
    static internal MC1S1System mc1S1System;
    static internal B1S1System b1S1System;
    static internal B2S1System b2S1System;
    static internal CleanUpS1System cleanUpS1System;

    //S2
    static internal PS2System pS2System;
    static internal C1S2System c1S2System;
    static internal C2S2System c2S2System;
    static internal CleanUpS2System cleanUpS2System;

    //S3
    static internal TS3System tS3System;
    static internal TWS3System twS3System;
    static internal TCS3System tcS3System;
    static internal B1S3System b1S3System;
    static internal FS3System fS3System;
    static internal ES3System eS3System;
    static internal C1S3System c1S3System;
    static internal CleanUpS3System cleanUpS3System;

    //despawner entities
    Entity[] despawners = new Entity[4];

    //entity manager
    EntityManager entityManager;

    //spell names
    static internal readonly string[] spellNames = new string[3]
    {
        "Transcribe \"Fury Beyond the Samsara of Life and Death\"",
        "Transcribe \"Brilliant Blizzard Blossom\"",
        "Transcribe \"Absent Desire to Welcome the Eight Sufferings\"",
    };

    //spell name obj
    static internal GameObject spellNameObj;
    static internal SpellNameMB spellNameMB;

    //dialogue obj
    static internal GameObject dialogueObj;
    static internal DialogueMB dialogueMB;

    // Start is called before the first frame update
    void Start()
    {
        spellPhase = 0;
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        //self-managed UIs
        //loading layout
        loadingLayout = GameObject.FindGameObjectWithTag("LoadingLayout");
        loadingLayout.SetActive(true);

        //dialogue-related
        dialogueObj = GameObject.FindGameObjectWithTag("Dialogue");
        dialogueMB = dialogueObj.GetComponent<DialogueMB>();
        dialogueMB.DoAppear(false);

        //pause canvas
        pauseCanvas = Instantiate(pauseCanvasPrefab);

        continueButton = GameObject.FindGameObjectWithTag("ContinueButton").GetComponent(typeof(Button)) as Button;
        quitButton = GameObject.FindGameObjectWithTag("QuitButton").GetComponent(typeof(Button)) as Button;
        continueButton.onClick.AddListener(Pause);
        quitButton.onClick.AddListener(Exit);

        pauseCanvas.SetActive(false);

        //find shadows
        mokouS = GameObject.FindGameObjectWithTag("mokouS");
        mokouS.SetActive(false);
        mokouSMB = mokouS.GetComponent(typeof(ShadowMB)) as ShadowMB;
        cirnoS = GameObject.FindGameObjectWithTag("cirnoS");
        cirnoS.SetActive(false);
        cirnoSMB = cirnoS.GetComponent(typeof(ShadowMB)) as ShadowMB;
        byakurenS = GameObject.FindGameObjectWithTag("byakurenS");
        byakurenS.SetActive(false);
        byakurenSMB = byakurenS.GetComponent(typeof(ShadowMB)) as ShadowMB;

        //find spell name display
        spellNameObj = GameObject.FindGameObjectWithTag("SpellName");
        spellNameMB = spellNameObj.GetComponent(typeof(SpellNameMB)) as SpellNameMB;
        spellNameMB.SetText("");

        //waits until game manager is ready
        yield return new WaitForFixedUpdate();
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        gameManagerMB = gameManager.GetComponent(typeof(GameManagerMB)) as GameManagerMB;

        //initializes initializer
        Instantiate(initializer);
        yield return new WaitUntil(() => InitializerMB.finished);

        //instantiates and references despawner entities
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        despawners[0] = entityManager.Instantiate(InitializerMB.despawnerEntityPrefab0);
        despawners[1] = entityManager.Instantiate(InitializerMB.despawnerEntityPrefab1);
        despawners[2] = entityManager.Instantiate(InitializerMB.despawnerEntityPrefab2);
        despawners[3] = entityManager.Instantiate(InitializerMB.despawnerEntityPrefab3);

        //non-physics systems
        playerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerSystem>();
        playerSystem.Enabled = true;
        playerBulletSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerBulletSystem>();
        playerBulletSystem.Enabled = true;
        npcSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<NPCSystem>();
        npcSystem.Enabled = true;

        //NPC System adjustments
        NPCSystem.resetHealth = false;
        NPCSystem.switchOffInvuln = false;

        yield return new WaitForFixedUpdate();

        //collision system
        collisionSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CollisionSystem>();
        collisionSystem.Enabled = true;

        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();

        //spell systems

        //spell manager
        spellManager = Instantiate(spellManagerPrefab);
        spellManagerMB = spellManager.GetComponent(typeof(SpellManagerMB)) as SpellManagerMB;


        //S1 (Transcribe "Fury Beyond the Samsara of Life and Death")
        //prepares scriptable object
        S1SO.npc = NPCSystem.NPC;
        S1SO.b1 = InitializerMB.b1S1EntityPrefab;
        S1SO.b2 = InitializerMB.b2S1EntityPrefab;
        S1SO.c1 = InitializerMB.c1S1EntityPrefab;
        S1SO.mc1 = InitializerMB.mc1S1EntityPrefab;

        S1SO.simulation = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<StepPhysicsWorld>();
        S1SO.buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
        S1SO.ecbS = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        yield return new WaitForEndOfFrame();

        //initiates systems
        mc1S1System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MC1S1System>();
        mc1S1System.Enabled = true;
        b1S1System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<B1S1System>();
        b1S1System.Enabled = true;
        b2S1System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<B2S1System>();
        b2S1System.Enabled = true;
        cleanUpS1System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CleanUpS1System>();
        cleanUpS1System.Enabled = true;
        yield return new WaitForEndOfFrame();

        //S2 (Transcribe "Brilliant Blizzard Blossom")
        //prepares scriptable object
        S2SO.npc = NPCSystem.NPC;
        S2SO.c1 = InitializerMB.c1S2EntityPrefab;
        S2SO.c2 = InitializerMB.c2S2EntityPrefab;
        S2SO.p = InitializerMB.pS2EntityPrefab;

        S2SO.simulation = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<StepPhysicsWorld>();
        S2SO.buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
        S2SO.ecbS = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        yield return new WaitForEndOfFrame();

        //initiates systems
        pS2System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PS2System>();
        pS2System.Enabled = true;
        c1S2System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<C1S2System>();
        c1S2System.Enabled = true;
        c2S2System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<C2S2System>();
        c2S2System.Enabled = true;
        cleanUpS2System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CleanUpS2System>();
        cleanUpS2System.Enabled = true;
        yield return new WaitForEndOfFrame();

        //S3 (Transcribe "Absent Desire to Welcome the Eight Sufferings")
        //prepares scriptable object
        S3SO.npc = NPCSystem.NPC;
        S3SO.c1 = InitializerMB.c1S3EntityPrefab;
        S3SO.b1 = InitializerMB.b1S3EntityPrefab;
        S3SO.f = InitializerMB.fS3EntityPrefab;
        S3SO.e = InitializerMB.eS3EntityPrefab;
        S3SO.t = InitializerMB.tS3EntityPrefab;

        S3SO.simulation = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<StepPhysicsWorld>();
        S3SO.buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
        S3SO.ecbS = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        yield return new WaitForEndOfFrame();

        //initiates systems
        tS3System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TS3System>();
        tS3System.Enabled = true;
        tcS3System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TCS3System>();
        tcS3System.Enabled = true;
        twS3System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TWS3System>();
        twS3System.Enabled = true;
        b1S3System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<B1S3System>();
        b1S3System.Enabled = true;
        fS3System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<FS3System>();
        fS3System.Enabled = true;
        eS3System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ES3System>();
        eS3System.Enabled = true;
        c1S3System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<C1S3System>();
        c1S3System.Enabled = true;
        cleanUpS3System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CleanUpS3System>();
        cleanUpS3System.Enabled = true;
        yield return new WaitForEndOfFrame();


        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();


        if (!isTesting)
        {
            //opens the screen
            loadingLayout.SetActive(false);

            dialogueMB.BeginDialogue(DialogueSO.startDialogue);
            yield return new WaitWhile(() => dialogueMB.onDialogue);

            Coroutine name1 = spellNameMB.SetText(spellNames[0]);
            yield return StartCoroutine(ResetHealth());
            mokouS.SetActive(true);
            yield return name1;
            yield return StartCoroutine(TurnOffNPCInvuln());
            yield return StartCoroutine(Spell1());
            mokouSMB.NextPath();
            yield return new WaitUntil(() => mokouSMB.prog == 1);

            Coroutine name2 = spellNameMB.SetText(spellNames[1]);
            yield return StartCoroutine(ResetHealth());
            cirnoS.SetActive(true);
            yield return name2;
            yield return new WaitForSeconds(0.4f);
            yield return StartCoroutine(TurnOffNPCInvuln());
            yield return StartCoroutine(Spell2());
            cirnoSMB.NextPath();
            yield return new WaitUntil(() => cirnoSMB.prog == 1);

            Coroutine name3 = spellNameMB.SetText(spellNames[2]);
            yield return StartCoroutine(ResetHealth());
            byakurenS.SetActive(true);
            yield return name3;
            yield return new WaitForSeconds(0.4f);
            yield return StartCoroutine(TurnOffNPCInvuln());
            yield return StartCoroutine(Spell3());
            byakurenSMB.NextPath();
            yield return new WaitUntil(() => byakurenSMB.prog == 1);

            Coroutine clearname = spellNameMB.SetText("");
            yield return clearname;

            dialogueMB.BeginDialogue(DialogueSO.endDialogue);
            yield return new WaitWhile(() => dialogueMB.onDialogue);
        } else
        {
            //opens the screen
            loadingLayout.SetActive(false);
            yield return new WaitForSeconds(2);

            Coroutine name1 = spellNameMB.SetText(spellNames[0]);
            yield return StartCoroutine(ResetHealth());
            mokouS.SetActive(true);
            yield return name1;
            spellPhase = 1;
            yield return StartCoroutine(Spell1());
            NPCSystem.healthRingImage.fillAmount = 0;
            yield return new WaitForFixedUpdate();
            mokouSMB.NextPath();
            yield return new WaitUntil(() => mokouSMB.prog == 1);

            Coroutine name2 = spellNameMB.SetText(spellNames[1]);
            yield return StartCoroutine(ResetHealth());
            cirnoS.SetActive(true);
            yield return name2;
            spellPhase = 2;
            yield return new WaitForSeconds(0.4f);
            yield return StartCoroutine(Spell2());
            NPCSystem.healthRingImage.fillAmount = 0;
            yield return new WaitForFixedUpdate();
            cirnoSMB.NextPath();
            yield return new WaitUntil(() => cirnoSMB.prog == 1);

            Coroutine name3 = spellNameMB.SetText(spellNames[2]);
            yield return StartCoroutine(ResetHealth());
            byakurenS.SetActive(true);
            yield return name3;
            spellPhase = 3;
            yield return new WaitForSeconds(0.4f);
            yield return StartCoroutine(Spell3());
            NPCSystem.healthRingImage.fillAmount = 0;
            yield return new WaitForFixedUpdate();
            byakurenSMB.NextPath();
            yield return new WaitUntil(() => byakurenSMB.prog == 1);

            Coroutine clearname = spellNameMB.SetText("692077616e7420746f20646965");
            yield return clearname;

            BenchMB.SendStats();

            yield return new WaitForSeconds(2);
        }



        Exit();
    }

    IEnumerator BeginStage()
    {
        yield return null;
    }

    IEnumerator ResetHealth()
    {
        NPCSystem.ResetAndInvuln();
        yield return new WaitWhile(() => NPCSystem.resetHealth);
    }

    IEnumerator TurnOffNPCInvuln()
    {
        NPCSystem.SwitchOffInvuln();
        yield return new WaitWhile(() => NPCSystem.switchOffInvuln);
    }

    IEnumerator Spell1() //mokou
    {
        yield return new WaitForEndOfFrame();
        yield return spellManagerMB.Spell1API();
    }

    IEnumerator Spell2() //cirno
    {
        yield return new WaitForEndOfFrame();
        yield return spellManagerMB.Spell2API();
    }

    IEnumerator Spell3() //hijiri
    {
        yield return new WaitForEndOfFrame();
        yield return spellManagerMB.Spell3API();
    }

    //to-do later
    IEnumerator CleanUp()
    {
        //resets initializer's ready variable
        InitializerMB.finished = false;
        Debug.Log("cleaning scene");

        //"closes the curtain"
        loadingLayout.SetActive(true);
        pauseCanvas.SetActive(false);
        Debug.Log("set UIs");

        //disable all systems
        collisionSystem.Enabled = false;
        Debug.Log("disabled collision system");
        npcSystem.Enabled = false;
        Debug.Log("disabled npc systems");
        playerBulletSystem.Enabled = false;
        Debug.Log("disabled player bullet system");
        playerSystem.Enabled = false;
        Debug.Log("disabled player system");

        yield return new WaitForEndOfFrame();
        Debug.Log("systems disabled");

        //disable all spell systems

        //disable spell 1
        b1S1System.Enabled = false;
        b2S1System.Enabled = false;
        mc1S1System.Enabled = false;
        cleanUpS1System.Enabled = false;

        //disable spell 2
        pS2System.Enabled = false;
        c1S2System.Enabled = false;
        c2S2System.Enabled = false;
        cleanUpS2System.Enabled = false;

        //disable spell 3
        twS3System.Enabled = false;
        eS3System.Enabled = false;
        tS3System.Enabled = false;
        tS3System.Enabled = false;
        b1S3System.Enabled = false;
        fS3System.Enabled = false;
        c1S3System.Enabled = false;
        cleanUpS3System.Enabled = false;

        yield return new WaitForEndOfFrame();


        //destroy all entities
        entityManager.DestroyEntity(entityManager.GetAllEntities());
        yield return new WaitForEndOfFrame();
        Debug.Log("entities destroyed");

        //destroy all prefab entities
        //already happened at the line above

        //destroy connected gameobjects
        Destroy(pauseCanvas);
        Debug.Log("canvas destroyed");

        //turn off shadows
        Debug.Log("shadows turned off");

        Debug.Log("committing sudoku");
        //commit sudoku lmao
        Destroy(gameObject);
    }

    public Coroutine CleanUpAPI()
    {
        return StartCoroutine(CleanUp());
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Pause();
        }
    }

    internal void Pause()
    {
        Time.timeScale = math.abs(Time.timeScale - 1);
        pauseCanvas.SetActive(!pauseCanvas.activeSelf);
    }

    internal void Exit()
    {
        gameManagerMB.ChangeSceneAPI(1);
    }
}
