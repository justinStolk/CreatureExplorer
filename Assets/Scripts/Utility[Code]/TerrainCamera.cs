using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainCamera : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(DeactivateCamera());
    }
    private IEnumerator DeactivateCamera()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
