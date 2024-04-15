using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button), typeof(Collider))]
public class VRButton : MonoBehaviour
{
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out VRHandController hand))
        {
            button.onClick.Invoke();
        }
    }
}
