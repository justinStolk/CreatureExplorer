using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestableObject : MonoBehaviour//, IIdentifiable
{
    // The QuestObjectID should be shared between all objects that are to be seen as the same. Therefore, place this on prefabs, so that all instances share the ID
    [field: SerializeField] public string QuestObjectID { get; private set; }
    //[field: SerializeField] public string GivenName { get; private set; }
    //[field: SerializeField] public string GivenDescription { get; private set; }
}
