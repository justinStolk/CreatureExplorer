using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WindRotation : MonoBehaviour
{
    //public Transform WindController;
    public Transform child;
    float Rotation;
    float Movement;
    public float WindStrength = 1f;
    Transform WindController;

    void Start()
    {
        Vector3 worldPosition = transform.position;
        child = this.gameObject.transform.GetChild(0);
    }

    void Update()
    {
        Transform WindController = GameObject.FindWithTag("WindController").transform;
        Rotation = WindController.eulerAngles.y;
        Movement = (Mathf.Cos(Time.time)+2) * WindStrength;
        transform.rotation = Quaternion.Euler(Movement, Rotation, 0);
        child.transform.rotation = Quaternion.Euler((Movement*1.5f), Rotation, 0);

    }
}
