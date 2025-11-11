using UnityEngine;

// Debug util for showing the transform axis of an object

[ExecuteInEditMode]
public struct TransformGizmos
{
    public static void DrawTransformGizmo(Transform transform)
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        Debug.DrawRay(transform.position, transform.right, Color.red);
        Debug.DrawRay(transform.position, transform.up, Color.green);
    }
}
