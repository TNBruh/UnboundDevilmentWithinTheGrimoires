using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class IntroMB : MonoBehaviour
{
    [SerializeField] GameObject characterNameObj;
    [SerializeField] GameObject dialogueObj;
    TextMeshProUGUI characterName;
    TextMeshProUGUI dialogue;
    static internal int alphaMult = -1;
    static internal float alphaLevel = 0;
    static readonly internal float[] rgbLevel = new float[3] {1, 1, 1};
    

    private void Start()
    {
        //retrieves text component
        characterName = characterNameObj.GetComponent<TextMeshProUGUI>();
        dialogue = dialogueObj.GetComponent<TextMeshProUGUI>();

        //set alpha, through setting colour
        SetColour();

        //start coroutine
        StartCoroutine(DialogueProcess());
    }

    private void Update()
    {
        //changing alpha level
        alphaLevel = math.clamp(alphaLevel + IntroSO.alphaChangeSpeed * alphaMult * Time.deltaTime, 0, 1);

        //applying alpha level
        SetColour();

        if (Input.GetButtonDown("Cancel"))
        {
            SceneManager.LoadScene(1);
        }
    }

    private void SetColour()
    {
        characterName.color = new Color(rgbLevel[0], rgbLevel[1], rgbLevel[2], alphaLevel);
        dialogue.color = new Color(rgbLevel[0], rgbLevel[1], rgbLevel[2], alphaLevel);
    }

    IEnumerator DialogueProcess()
    {
        for (int i = 0; i < IntroSO.dialogues.GetLength(0); i++)
        {
            string n = IntroSO.dialogues[i,0];
            string d = IntroSO.dialogues[i,1];

            yield return StartCoroutine(DialogueAppear(n, d));
        }
        SceneManager.LoadScene(1);
    }

    IEnumerator DialogueAppear(string inpCharName, string inpDialogue)
    {
        //set name and dialogue
        characterName.text = inpCharName;
        dialogue.text = inpDialogue;
        //set signal to appear
        alphaMult = 1;
        yield return new WaitUntil(() => alphaLevel == 1);
        //let readers read
        yield return new WaitForSeconds(IntroSO.readTime);
        //set signal to disappear
        alphaMult = -1;
        yield return new WaitUntil(() => alphaLevel == 0);
    }
}
