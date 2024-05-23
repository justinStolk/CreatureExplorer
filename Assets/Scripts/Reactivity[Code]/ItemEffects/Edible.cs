using UnityEngine;

public class Edible : StatusEffect
{
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
        }
    }
}