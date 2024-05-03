using System.Collections;
using UnityEngine;

public abstract class Spawner : MonoBehaviour
{
    [Header("Gizmos")]
    [SerializeField] protected Color gizmoColour;

    [Header("Spawning")]
    [SerializeField] protected LayerMask canspawnOn;
    [SerializeField] protected GameObject spawnedObject;

    [SerializeField] protected float spawnRange;
    [SerializeField] protected float spawnDelay;
    [SerializeField] protected int maxSpawnAmount = 10;
    [SerializeField] protected bool continous;

    private bool isFull = false;

    private void Start()
    {
        Spawn();
        StartCoroutine(SpawnTimer());
    }

    private void OnTransformChildrenChanged()
    {
        if (continous && isFull)
        {
            isFull = transform.childCount > maxSpawnAmount;

            if (!isFull)
                StartCoroutine(SpawnTimer());
        }
    }

    protected abstract void Spawn();

    private IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(spawnDelay);

        Spawn();

        if (continous)
        {
            isFull = transform.childCount > maxSpawnAmount;
            
            if (!isFull)
                StartCoroutine(SpawnTimer());
        }
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        GizmoDrawer.DrawPrimitive(transform.position, Vector3.one * (spawnRange+1), GizmoType.WireSphere, gizmoColour);
    }
#endif
}
