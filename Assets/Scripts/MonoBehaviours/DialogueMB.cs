using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Mathematics;

public class DialogueMB : MonoBehaviour
{
    [SerializeField] float timeToCancel = 0.2f;
    [SerializeField] GameObject p;
    [SerializeField] GameObject d;
    internal bool nextSignal = false;
    float timer = 0;
    TextMeshProUGUI textMeshProUGUI;
    internal Image panel;
    internal Coroutine dialogueCoroutine;
    internal Coroutine cancelCoroutine;
    internal bool onDialogue = false;

    private void OnEnable()
    {
        textMeshProUGUI = d.GetComponent<TextMeshProUGUI>();
        panel = p.GetComponent<Image>();
        DoAppear(false);

    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (timer > 0 && dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
                DoAppear(false);
            }
            timer = timeToCancel;
            nextSignal = true;
        }
        timer = math.clamp(timer - Time.deltaTime, 0f, 1f);
    }

    internal void DoAppear(bool inp)
    {
        if (inp)
        {
            onDialogue = true;
            textMeshProUGUI.color = new Color(1f, 1f, 1f, 1);
            panel.color = new Color(0f, 0f, 0f, 0.69f);
        } else
        {
            onDialogue= false;
            textMeshProUGUI.color = new Color(1f, 1f, 1f, 0);
            panel.color = new Color(0f, 0f, 0f, 0);
        }
    }

    IEnumerator CycleDialogue(string[,] dialogues)
    {
        DoAppear(true);
        for (int i = 0; i < dialogues.GetLength(0); i++)
        {
            textMeshProUGUI.text = dialogues[i,0]+'\n'+dialogues[i,1];
            yield return new WaitUntil(() => nextSignal);
            nextSignal = false;
        }
        DoAppear(false);
    }

    internal void BeginDialogue(string[,] dialogues)
    {
        dialogueCoroutine = StartCoroutine(CycleDialogue(dialogues));
    }
}
