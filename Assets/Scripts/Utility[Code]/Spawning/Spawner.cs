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
        InvokeRepeating("Spawn", 0, spawnDelay);
    }

    private void OnTransformChildrenChanged()
    {
        if (continous && isFull)
        {
            isFull = transform.childCount >= maxSpawnAmount;

            if (!isFull)
                InvokeRepeating("Spawn", spawnDelay, spawnDelay);
        } else if (continous)
        {
            isFull = transform.childCount >= maxSpawnAmount;

            if (isFull)
                CancelInvoke("Spawn");
        }
    }

    protected abstract void Spawn();

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        GizmoDrawer.DrawPrimitive(transform.position, Vector3.one * (spawnRange+1), GizmoType.WireSphere, gizmoColour);
    }
#endif
}
