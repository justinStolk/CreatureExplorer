using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BugSpot : MonoBehaviour
{
    private ParticleSystem particleSystem;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        particleSystem.Play();
    }
}
