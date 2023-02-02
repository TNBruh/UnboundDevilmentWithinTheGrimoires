using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundMB : MonoBehaviour
{
    [SerializeField]
    internal float speedX = -0.02f;
    [SerializeField]
    internal float speedY = -0.08f;

    RawImage rawImage;
    Rect uvRect;


    private void Start()
    {
        rawImage = gameObject.GetComponent(typeof(RawImage)) as RawImage;
        Vector3[] corners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(corners);
        foreach (Vector3 corner in corners)
        {
            Debug.Log(corner);
        }
    }

    private void FixedUpdate()
    {
        uvRect = rawImage.uvRect;

        float delta = Time.deltaTime;

        uvRect.x = uvRect.x + speedX * delta;
        uvRect.y = uvRect.y + speedY * delta;

        if (uvRect.x < -1 ) { uvRect.x += 1; }
        if (uvRect.y > -1) { uvRect.y -= 1; }

        rawImage.uvRect = uvRect;
    }
}
