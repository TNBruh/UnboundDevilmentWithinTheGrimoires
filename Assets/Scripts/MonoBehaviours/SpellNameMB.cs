using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpellNameMB : MonoBehaviour
{
    TextMeshProUGUI textMeshProUGUI;
    [SerializeField] float waitTime = 0.05f;
    Coroutine currentProcess;

    private void OnEnable()
    {
        textMeshProUGUI = gameObject.GetComponent(typeof(TextMeshProUGUI)) as TextMeshProUGUI;
        textMeshProUGUI.text = "";
    }

    internal Coroutine SetText(string target)
    {
        if (currentProcess != null)
        {
            StopCoroutine(currentProcess);
        }
        currentProcess = StartCoroutine(CycleText(target));
        return currentProcess;
    }

    IEnumerator CycleText(string t)
    {
        for (int i = 0; i <= t.Length; i++)
        {
            textMeshProUGUI.text = t.Substring(0, i);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
