using UnityEngine;

public class SegmentBounds : MonoBehaviour
{
    public int segmentIndex;

    public System.Action<int> playerCollideEnter;
    public System.Action<int> playerCollideExit;

    public void Setup(Vector3 pos, Vector3 size, int _segmentIndex)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gameObject.AddComponent<BoxCollider>().isTrigger = true;
        gameObject.AddComponent<MeshFilter>().sharedMesh = cube.GetComponent<MeshFilter>().sharedMesh;

        Destroy(cube);
        transform.position = pos;
        transform.localScale = size;

        gameObject.isStatic = true;

        segmentIndex = _segmentIndex;

        name = "Seg" + segmentIndex + "/Bounds";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerCollideEnter?.Invoke(segmentIndex);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerCollideExit?.Invoke(segmentIndex);
    }
}
