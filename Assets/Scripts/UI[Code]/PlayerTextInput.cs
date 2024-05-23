using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerTextInput : MonoBehaviour
{
    [field:SerializeField] public TMP_InputField TextField { get; private set; }

    void Start()
    {
        TextField.onSelect.AddListener((string s) => Scrapbook.OnBeginType?.Invoke());
        TextField.onDeselect.AddListener((string s) => Scrapbook.OnEndType?.Invoke());
    }

    private void OnDestroy()
    {
        TextField.onSelect.RemoveAllListeners();
        TextField.onDeselect.RemoveAllListeners();
    }
}
