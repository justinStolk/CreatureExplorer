using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LookForObjects<T>
{
    public static List<T> CheckForObjects(Vector3 checkFromPosition, float checkingRange)
    {
        List<T> result = new List<T>();
        float distance = checkingRange;
        Collider nearest = null;

        foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        {
            if (c.gameObject.TryGetComponent(out T objectToCheckFor) && (c.transform.position - checkFromPosition).magnitude < distance)
            {
                result.Add(objectToCheckFor); 
                distance = (c.transform.position - checkFromPosition).magnitude;
                nearest = c;
            }
        }
        return result;
    }

    public static bool TryGetClosestObject(Vector3 checkFromPosition, float checkingRange, out T nearestObject)
    {
        nearestObject = default(T);
        float distance = checkingRange;
        Collider nearest = null;


        foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        {
            if (c.gameObject.TryGetComponent(out T objectToCheckFor) && (c.transform.position - checkFromPosition).magnitude < distance)
            {
                nearestObject = objectToCheckFor;
                distance = (c.transform.position - checkFromPosition).magnitude;
                nearest = c;
            }
        }

        return nearest!= null;
    }

    public static bool TryGetClosestObject(Vector3 checkFromPosition, Vector3 nearestToPosition, float checkingRange, out T nearestObject)
    {
        nearestObject = default(T);
        float distance = checkingRange;
        Collider nearest = null;


        foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        {
            if (c.gameObject.TryGetComponent(out T objectToCheckFor) && (c.transform.position - nearestToPosition).magnitude < distance)
            {
                nearestObject = objectToCheckFor;
                distance = (c.transform.position - nearestToPosition).magnitude;
                nearest = c;
            }
        }

        return nearest!= null;
    }

    public static bool TryGetClosestObject(T objectToCheckFor, Vector3 checkFromPosition, float checkingRange, out T nearestObject)
    {
        nearestObject = objectToCheckFor;
        float distance = checkingRange;
        Collider nearest = null;


        foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        {
            if (c.gameObject.TryGetComponent(out objectToCheckFor) && (c.transform.position - checkFromPosition).magnitude < distance)
            {
                nearestObject = objectToCheckFor;
                distance = (c.transform.position - checkFromPosition).magnitude;
                nearest = c;
            }
        }

        return nearest!= null;
    }

    /// <summary>
    /// Look for the closest object with same type as objectToCheckFor
    /// </summary>
    /// <param name="objectToCheckFor"></param>
    /// <param name="checkFromPosition"></param>
    /// <param name="checkingRange"></param>
    /// <param name="searcher">the object that initiated the search, excluded from results</param>
    /// <param name="nearestObject"></param>
    /// <returns></returns>
    public static bool TryGetClosestObject(T objectToCheckFor, Vector3 checkFromPosition, float checkingRange, GameObject searcher, out T nearestObject)
    {
        nearestObject = objectToCheckFor;
        float distance = checkingRange;
        Collider nearest = null;

        foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        {
            if (c.gameObject.TryGetComponent(out objectToCheckFor) && (c.transform.position - checkFromPosition).sqrMagnitude < distance && !c.gameObject.Equals(searcher))
            {
                nearestObject = objectToCheckFor;
                distance = (c.transform.position - checkFromPosition).magnitude;
                nearest = c;
            }
        }

        return nearest!= null;
    }

}
