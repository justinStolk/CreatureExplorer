using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalWindCotroller : MonoBehaviour
{
    public Transform Controller;
    public float WindStrength = 1f;
    float Rotation;
    public Material Grass;

    void Update()
    {
        Rotation = Controller.eulerAngles.y;
        Grass.SetFloat("_WindDirection", Rotation);
    }
}
