using UnityEngine;

public class FruitTree : Tree, IBreakable
{
    [field: SerializeField] private AudioClip breakingSound;
    private SoundPlayer soundPlayer;

    public void Break()
    {
        soundPlayer = GetComponentInParent<SoundPlayer>();
        if (soundPlayer != null)
        {
            soundPlayer.PlaySound(breakingSound, true);
        }

        foreach (Food berry in GetComponentsInChildren<Food>())
        {
            berry.ActivatePhysics();
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color originalGizmoColour = Gizmos.color;
        Gizmos.color = Color.red;

        if (Application.IsPlaying(gameObject))
        {
            return;
            /*
            LODGroup group = gameObject.GetComponentInChildren<LODGroup>();

            foreach (Renderer r in group.GetLODs()[1].renderers)
            {
                Gizmos.DrawWireMesh(r.GetComponent<MeshFilter>().mesh, r.transform.position, r.transform.rotation, r.transform.lossyScale * 1.1f);
            }*/
        } else {
            foreach (MeshFilter drawnMesh in GetComponentsInChildren<MeshFilter>())
            {
                Gizmos.DrawWireMesh(drawnMesh.sharedMesh, drawnMesh.transform.position, drawnMesh.transform.rotation, drawnMesh.transform.lossyScale * 1.1f);
            }
        }
        Gizmos.color = originalGizmoColour;
    }
#endif
}
