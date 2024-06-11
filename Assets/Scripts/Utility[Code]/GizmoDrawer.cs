using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoDrawer : MonoBehaviour
{
    private static Matrix4x4 originalMatrix;

    public static void DrawPrimitive(Vector3 origin, Vector3 size, GizmoType drawType, Color drawColour)
    {
        Color originalColour = Gizmos.color;
        Gizmos.color = drawColour;

        switch (drawType)
        {
            case (GizmoType.Cube):
                Gizmos.DrawCube(origin, size);
                break;
            case (GizmoType.Line):
                Gizmos.DrawLine(origin, origin+size);
                break;
            case (GizmoType.Ray):
                Gizmos.DrawRay(origin, size);
                break;
            case (GizmoType.Sphere):
                originalMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.Scale(size);

                origin.Scale(new Vector3(1 / size.x, 1 / size.y, 1 / size.z));

                Gizmos.DrawSphere(origin, 1);

                Gizmos.matrix = originalMatrix;
                break;
            case (GizmoType.WireCube):
                Gizmos.DrawWireCube(origin, size);
                break;
            case (GizmoType.WireSphere):
                originalMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.Scale(size);

                origin.Scale(new Vector3(1 / size.x, 1 / size.y, 1 / size.z));

                Gizmos.DrawWireSphere(origin, 1);

                Gizmos.matrix = originalMatrix;
                break;
        }

        Gizmos.color = originalColour;
    }
}

public enum GizmoType
{
    Cube,
    Line,
    Ray,
    Sphere,
    WireSphere,
    WireCube
}
