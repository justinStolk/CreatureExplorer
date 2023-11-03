using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private LayerMask canspawnOn;
    [SerializeField] private GameObject spawnedObject;

    [SerializeField] private float spawnrange;
    [SerializeField] private float spawnDelay;
    [SerializeField] private int maxSpawnAmount = 10;
    [SerializeField] private bool continous;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColour;

    // Start is called before the first frame update
    void Awake()
    {
        Spawn();
        StartCoroutine(SpawnTimer());
    }

    private void Spawn()
    {
        Vector3 spawnpos = transform.position + new Vector3(Random.Range(-spawnrange, spawnrange), 0, Random.Range(-spawnrange, spawnrange));

        if (Physics.Raycast(spawnpos + Vector3.up * 200, Vector3.down, out RaycastHit hit, 500, canspawnOn))
        {
            Instantiate(spawnedObject, hit.point + new Vector3(0, spawnedObject.transform.lossyScale.y * 0.5f, 0), transform.rotation, transform);
        }
    }

    private IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(spawnDelay);

        while (transform.childCount > maxSpawnAmount)
        {
            yield return null;
        }

        Spawn();

        if (continous)
            StartCoroutine(SpawnTimer());
    }

    private void OnDrawGizmos()
    {
        Color originalColour = Gizmos.color;
        Gizmos.color = gizmoColour;
        Gizmos.DrawWireSphere(transform.position, (spawnrange+1)); 
        Gizmos.color = originalColour;

    }
}
