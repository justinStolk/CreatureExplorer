using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationToText : MonoBehaviour
{
    [SerializeField] private RectTransform trackedLocation;
    private TMPro.TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = $"pos: {trackedLocation.position}, \nrot: {trackedLocation.rotation.eulerAngles}";
    }
}
