using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFader : MonoBehaviour
{
    [SerializeField] private Image imageToFade;
    [SerializeField] private bool fadeFromColour;
    [SerializeField] private Color fadeColour;
    [SerializeField] private float fadeTime = 1;

    private void Start()
    {
        if (!fadeFromColour)
        {
            imageToFade.gameObject.SetActive(false);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        imageToFade.gameObject.SetActive(true);
        imageToFade.enabled = true;
        Color startColour = imageToFade.color;

        float timer = fadeTime;

        if (fadeFromColour)
        {
            while (timer > 0)
            {
                imageToFade.color =  Color.Lerp(startColour, fadeColour, timer / fadeTime);
                timer -= Time.deltaTime;
                yield return null;
            }

            imageToFade.color = startColour;
            enabled = false;
        }
        else
        {
            while (timer > 0)
            {
                imageToFade.color = new Color(startColour.r, startColour.g, startColour.b, timer / fadeTime);
                timer -= Time.deltaTime;
                yield return null;
            }

            //imageToFade.gameObject.SetActive(false);
            enabled = false;
        }
    }
}
