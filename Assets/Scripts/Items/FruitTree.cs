using UnityEngine;

public class FruitTree : Tree, IBreakable
{

    public void Break()
    {
        foreach (Food berry in GetComponentsInChildren<Food>())
        {
            berry.ActivatePhysics();
        }
    }

    private void OnDrawGizmos()
    {
        Color originalGizmoColour = Gizmos.color;
        Gizmos.color = Color.red;

        foreach (MeshFilter drawnMesh in GetComponentsInChildren<MeshFilter>())
        {
            Gizmos.DrawWireMesh(drawnMesh.sharedMesh, drawnMesh.transform.position, drawnMesh.transform.rotation, drawnMesh.transform.lossyScale * 1.1f);
        }

        Gizmos.color = originalGizmoColour;
    }
}