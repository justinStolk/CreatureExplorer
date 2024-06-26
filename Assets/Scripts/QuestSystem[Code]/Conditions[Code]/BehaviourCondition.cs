using UnityEngine;

[CreateAssetMenu(fileName = "BehaviourCondition", menuName = "Titan Quests/New Behaviour Condition")]
public class BehaviourCondition : QuestCondition
{
    [SerializeField] private Action requiredBehaviour;

    public override bool Evaluate(PictureInfo pictureInfo)
    {
        foreach (QuestableObject questable in pictureInfo.PictureObjects)
        {
            if (questable.TryGetComponent(out Creature creature))
            {
                if (creature.enabled)
                {
                    if (creature.CurrentAction.GetType() == requiredBehaviour.GetType())
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
