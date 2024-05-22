using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SliderValueText : MonoBehaviour
{
    [SerializeField] private float valueMultiplier = 1;
    [SerializeField] private bool setAsInt = true;
    private TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void SetValueText(float value)
    {
        if (setAsInt)
            textMesh.text = ((int)(value*valueMultiplier)).ToString();
        else
            textMesh.text = (value*valueMultiplier).ToString("0.000");
    }
}
