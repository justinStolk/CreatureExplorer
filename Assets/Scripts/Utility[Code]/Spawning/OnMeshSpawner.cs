using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class OnMeshSpawner : Spawner
{
    [SerializeField] private MeshFilter spawnOn;
    [SerializeField] private float distanceFromMesh = 0;
    [SerializeField] private bool rotateWithMesh;

    private MeshCollider spawnCollider;

    private void Awake()
    {
        if (spawnCollider == null)
            spawnCollider = GetComponent<MeshCollider>();

        spawnCollider.convex = true;
    }

    private void OnValidate()
    {
        if (spawnOn != null) 
        {
            spawnCollider = GetComponent<MeshCollider>();
            spawnCollider.convex = true;
            spawnCollider.isTrigger = true;
            spawnCollider.sharedMesh = spawnOn.sharedMesh;
            transform.position = spawnOn.transform.position;
        }
    }

    protected override void Spawn()
    {
        // TODO: figure out better way to have objects spawn on mesh (not inside)
        Vector3 spawnpos = transform.position + new Vector3(Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange))*10;
        Vector3 spawnPoint = spawnCollider.ClosestPoint(spawnpos);
        
        Quaternion spawnRotation = transform.rotation;

        if (rotateWithMesh)
        {
            spawnRotation.SetLookRotation(Vector3.forward, spawnPoint - transform.position);
        }
        
        spawnPoint = transform.position + (spawnPoint - transform.position) * (1+distanceFromMesh);


        Instantiate(spawnedObject, spawnPoint,spawnRotation, transform);
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        Color originalColour = Gizmos.color;
        Gizmos.color = gizmoColour;
        /*
        LODGroup group = gameObject.GetComponentInChildren<LODGroup>();

            Gizmos.color = Color.black;
        foreach (Renderer r in group.GetLODs()[0].renderers)
        {
            Gizmos.DrawWireMesh(r.GetComponent<MeshFilter>().mesh, r.transform.position, r.transform.rotation, r.transform.lossyScale * 1.1f);

        }
            Gizmos.color = gizmoColour;
        */
        //Gizmos.DrawWireMesh(spawnOn.mesh, Vector3.zero, Quaternion.identity, Vector3.one + transform.lossyScale * distanceFromMesh);
        Gizmos.color = originalColour;

        Gizmos.matrix = originalMatrix;
    }
#endif
}
