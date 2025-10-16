using UnityEngine;

// Debug util for showing the transform axis of an object

[ExecuteInEditMode]
public class TransformGizmos : MonoBehaviour
{
    public bool active = false;

    void Update()
    {
        if (active)
        {
            Debug.DrawRay(transform.position, transform.forward, Color.blue);
            Debug.DrawRay(transform.position, transform.right, Color.red);
            Debug.DrawRay(transform.position, transform.up, Color.green);
        }
    }
}
