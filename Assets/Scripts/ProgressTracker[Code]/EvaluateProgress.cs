using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvaluateProgress : MonoBehaviour
{
    // TODO: make async
    public static bool EvaluatePictureProgress(PictureInfo pictureInfo)
    {
        foreach (QuestableObject questable in pictureInfo.PictureObjects)
        {
            if (questable.TryGetComponent(out Creature creature))
            {
                if (creature.enabled)
                {
                    foreach (ProgressCategory progress in ProgressUIHandler.TrackedCategories)
                    {
                        if (progress.IsCategory(questable.QuestObjectID, out ProgressCategory rightCategory))
                        {
                            if (rightCategory.HasID(creature.CurrentAction.GetType().ToString(), out ProgressObject rightProgress))
                                UpdateTrackedProgress(rightProgress);
                        }
                    }
                }
            }
        }
        return false;
    }

    public static void UpdateTrackedProgress(string progressID)
    {
        foreach (ProgressCategory progress in ProgressUIHandler.TrackedCategories)
        {
            if (progress.HasID(progressID, out ProgressObject rightProgress))
            {
                UpdateTrackedProgress(rightProgress);
            }
        }
    }

    public static void UpdateTrackedProgress(ProgressObject progress)
    {
        progress.AddProgress();

        ProgressUIHandler.UpdateTrackedProgress(progress);
    }
}
