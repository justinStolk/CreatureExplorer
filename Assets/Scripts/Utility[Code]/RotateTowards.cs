using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowards : MonoBehaviour
{
    [SerializeField] private Transform rotationTarget;

    private RectTransform rectTransform;

    private void Start()
    {
        if (rotationTarget == null)
        {
            rotationTarget = Camera.main.transform;
        }

        rectTransform = GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        rectTransform?.LookAt(rotationTarget.position, Vector3.up);
        transform.Rotate(new Vector3(0, 180, 0));

        //transform.rotation.SetLookRotation(transform.position - rotationTarget.position);
    }
}
