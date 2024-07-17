using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LookForObjects<T>
{
    public static readonly Collider[] overlapBuffer = new Collider[100];

    public static List<T> CheckForObjects(Vector3 checkFromPosition, float checkingRange)
    {
        List<T> result = new List<T>();
        float distance = checkingRange;
        
        Physics.OverlapSphereNonAlloc(checkFromPosition, checkingRange, overlapBuffer);
       
        //foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        foreach (Collider c in overlapBuffer)
        {
            if (c != null && c.gameObject.TryGetComponent(out T objectToCheckFor))
            {
                result.Add(objectToCheckFor);
            }
        }
        return result;
    }

    public static List<T> CheckForObjects(Vector3 checkFromPosition, float checkingRange, LayerMask mask)
    {
        List<T> result = new List<T>();
        float distance = checkingRange;
        
        Physics.OverlapSphereNonAlloc(checkFromPosition, checkingRange, overlapBuffer, mask);
       
        //foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        foreach (Collider c in overlapBuffer)
        {
            if (c != null && c.gameObject.TryGetComponent(out T objectToCheckFor))
            {
                result.Add(objectToCheckFor); 
            }
        }

#if UNITY_EDITOR
        GizmoDrawer.DrawPrimitiveDelayed(checkFromPosition, Vector3.one*checkingRange, GizmoType.Sphere, new Color(1, 0.92f, 0.016f, 0.3f));
#endif
        return result;
    }

    public static bool TryGetClosestObject(Vector3 checkFromPosition, float checkingRange, out T nearestObject)
    {
        nearestObject = default(T);
        float distance = checkingRange;
        Collider nearest = null;

        Physics.OverlapSphereNonAlloc(checkFromPosition, checkingRange, overlapBuffer);

        //foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        foreach (Collider c in overlapBuffer)
        {
            if (c != null && c.gameObject.TryGetComponent(out T objectToCheckFor) && (c.transform.position - checkFromPosition).magnitude < distance)
            {
                nearestObject = objectToCheckFor;
                distance = (c.transform.position - checkFromPosition).magnitude;
                nearest = c;
            }
        }

        return nearest!= null;
    }

    public static bool TryGetClosestObject(Vector3 checkFromPosition, float checkingRange, LayerMask objLayer, out T nearestObject)
    {
        nearestObject = default(T);
        float distance = checkingRange;
        Collider nearest = null;

        Physics.OverlapSphereNonAlloc(checkFromPosition, checkingRange, overlapBuffer, objLayer);

        //foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        foreach (Collider c in overlapBuffer)
        {
            if (c != null && c.gameObject.TryGetComponent(out T objectToCheckFor) && (c.transform.position - checkFromPosition).magnitude < distance)
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

        Physics.OverlapSphereNonAlloc(checkFromPosition, checkingRange, overlapBuffer);

        //foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        foreach (Collider c in overlapBuffer)
        {
            if (c != null && c.gameObject.TryGetComponent(out T objectToCheckFor) && (c.transform.position - nearestToPosition).magnitude < distance)
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


        Physics.OverlapSphereNonAlloc(checkFromPosition, checkingRange, overlapBuffer);

        //foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        foreach (Collider c in overlapBuffer)
        {
            if (c != null && c.gameObject.TryGetComponent(out objectToCheckFor) && (c.transform.position - checkFromPosition).magnitude < distance)
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

        Physics.OverlapSphereNonAlloc(checkFromPosition, checkingRange, overlapBuffer);

        //foreach (Collider c in Physics.OverlapSphere(checkFromPosition, checkingRange))
        foreach (Collider c in overlapBuffer)
        {
            if (c != null && c.gameObject.TryGetComponent(out objectToCheckFor) && (c.transform.position - checkFromPosition).sqrMagnitude < distance && !c.gameObject.Equals(searcher))
            {
                nearestObject = objectToCheckFor;
                distance = (c.transform.position - checkFromPosition).magnitude;
                nearest = c;
            }
        }

#if UNITY_EDITOR
        GizmoDrawer.DrawPrimitiveDelayed(checkFromPosition, Vector3.one * checkingRange, GizmoType.Sphere, new Color(1, 0.92f, 0.016f, 0.3f));
#endif

        return nearest!= null;
    }

}
