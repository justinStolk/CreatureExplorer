using UnityEngine;

public class Edible : StatusEffect
{
    [SerializeField] private ParticleSystem particles;
    private Creature eatenBy;

    public void StartEating(Creature creature)
    {
        eatenBy = creature;
    }

    private void OnDestroy()
    {
        if (eatenBy != null)
        {
            TriggerStatusEffect(eatenBy);
            particles.Play();
        }
    }
}