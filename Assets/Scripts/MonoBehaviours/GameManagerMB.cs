using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerMB : MonoBehaviour
{
    //stage-related
    [SerializeField]
    internal GameObject stageManager;
    internal StageManagerMB stageManagerMB;
    //which scene we are in
    internal static Scene currentScene;
    
    //supposedly one and only game manager
    internal static GameManagerMB gameManagerMB;

    //scene-processing phase
    static Coroutine sceneProcessing;

    //main menu buttons
    GameObject startButtonObj;
    GameObject creditsButtonObj;
    GameObject exitButtonObj;
    Button startButton;
    Button creditsButton;
    Button exitButton;

    //credits screen
    GameObject creditsExitButtonObj;
    Button creditsExitButton;

    //audio
    AudioSource audioSource;
    [SerializeField] AudioClip startClip, loopClip;


    // Start is called before the first frame update
    void Start()
    {
        GameObject[] gameManagers = GameObject.FindGameObjectsWithTag("GameManager");
        if (gameManagers.Length > 1)
        {
            Destroy(gameObject);
        } else
        {
            DontDestroyOnLoad(gameObject);
            gameManagerMB = this;
            currentScene = SceneManager.GetActiveScene();

            audioSource = gameObject.GetComponent(typeof(AudioSource)) as AudioSource;
            StartCoroutine(PlaySong());

            StartCoroutine(ProcessScene(currentScene));
        }
    }

    internal Coroutine ChangeSceneAPI(int sceneInd)
    {
        return StartCoroutine(ChangeScene(sceneInd));
    }

    IEnumerator ProcessScene(Scene scene, Coroutine prevCoroutine = null)
    {
        yield return prevCoroutine;
        yield return new WaitUntil(() => scene.isLoaded);

        int sceneInd = scene.buildIndex;

        switch (sceneInd)
        {
            case 1: //main menu
                Debug.Log("processing main menu");
                startButtonObj = GameObject.FindGameObjectWithTag("StartButton");
                creditsButtonObj = GameObject.FindGameObjectWithTag("CreditsButton");
                exitButtonObj = GameObject.FindGameObjectWithTag("ExitButton");
                startButton = startButtonObj.GetComponent(typeof(Button)) as Button;
                creditsButton = creditsButtonObj.GetComponent(typeof(Button)) as Button;
                exitButton = exitButtonObj.GetComponent(typeof(Button)) as Button;

                startButton.onClick.AddListener(EnterStage);
                creditsButton.onClick.AddListener(EnterCredits);
                exitButton.onClick.AddListener(Sussy);

                break;
            case 2: //game
                GameObject stageManagerObj = Instantiate(stageManager);
                stageManagerMB = stageManagerObj.GetComponent(typeof(StageManagerMB)) as StageManagerMB;
                break;
            case 3: //credits
                Debug.Log("processing credits");
                exitButtonObj = GameObject.FindGameObjectWithTag("ExitButton");
                exitButton = exitButtonObj.GetComponent(typeof(Button)) as Button;

                exitButton.onClick.AddListener(EnterMain);
                break;
        }
    }
    IEnumerator CleanScene(Scene scene)
    {
        yield return new WaitUntil(() => scene.isLoaded);

        int sceneInd = scene.buildIndex;

        switch (sceneInd)
        {
            case 1: //main menu
                break;
            case 2: //game
                //clean up
                //order the stage manager to clean
                yield return stageManagerMB.CleanUpAPI();
                Debug.Log("cleaned stage");
                break;
            case 3: //credits
                break;
            case 4:
                break;
        }
    }

    IEnumerator ChangeScene(int sceneInd)
    {
        Scene previousScene = SceneManager.GetActiveScene();
        Coroutine cleanUp = StartCoroutine(CleanScene(previousScene));

        yield return cleanUp;

        AsyncOperation asyncLoadScene = SceneManager.LoadSceneAsync(sceneInd);
        yield return new WaitUntil(() => asyncLoadScene.isDone);
        Scene currentScene = SceneManager.GetActiveScene();

        yield return new WaitUntil(() => currentScene.isLoaded);
        StartCoroutine(ProcessScene(currentScene, cleanUp));
        Time.timeScale = 1;

    }

    void EnterStage()
    {
        ChangeSceneAPI(2);
    }

    void EnterCredits()
    {
        ChangeSceneAPI(3);
    }

    void EnterMain()
    {
        ChangeSceneAPI(1);
    }

    void Sussy()
    {
        Application.Quit();
    }

    IEnumerator PlaySong()
    {
        audioSource.clip = startClip;
        audioSource.Play();
        yield return new WaitWhile(() => audioSource.isPlaying);
        audioSource.clip = loopClip;
        audioSource.Play();
        audioSource.loop = true;

    }
}
