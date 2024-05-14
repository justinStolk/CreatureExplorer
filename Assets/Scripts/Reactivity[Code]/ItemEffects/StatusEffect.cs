using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;

    [SerializeField] protected string StatusEffectName;
    [SerializeField] protected bool oneShot;
    [SerializeField] protected bool destroyAfterTrigger = true;
    [SerializeField] protected float effectDuration = 0;

    [SerializeField] private CreatureState statusEffect;

    //private Creature affectedCreature;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!oneShot && effectDuration == 0)
        {
            Debug.LogError($"Effect of {gameObject.name} is set to have an effect over time, but the duration is set to 0");
        }
    }
#endif
    protected virtual void TriggerStatusEffect(Creature toAffect)
    {
        particles.Play();
        if (oneShot)
        {
            toAffect.UpdateValues(statusEffect);
            if (destroyAfterTrigger && gameObject.activeInHierarchy)
                DestroyImmediate(this.gameObject);
        }
        else
            StartCoroutine(TriggerEffect(toAffect));
    }

    public void TriggerStatusEffect(Creature toAffect, float duration = -1, bool instantanious = false)
    {
        effectDuration = duration < 0? effectDuration: duration;

        if (oneShot || instantanious)
        {
            toAffect.UpdateValues(statusEffect);

            if (destroyAfterTrigger)
                DestroyImmediate(this.gameObject);
        }
        else
            StartCoroutine(TriggerEffect(toAffect));
    }

    protected virtual void TriggerStatusEffect(List<Creature> toAffect)
    {
        foreach (Creature c in toAffect)
        {
            TriggerStatusEffect(c);
        }
    }

    private IEnumerator TriggerEffect(Creature affectedCreature)
    {
        float timer = 0;

        do
        {
            affectedCreature.UpdateValues(statusEffect);
            timer += Time.deltaTime;

            yield return new WaitForFixedUpdate();
        } while (timer < effectDuration);

        if (destroyAfterTrigger)
            DestroyImmediate(this.gameObject);
    }
}
