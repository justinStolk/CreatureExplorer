using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BugSpot : MonoBehaviour
{
    private ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        particles.Play();
    }
}
