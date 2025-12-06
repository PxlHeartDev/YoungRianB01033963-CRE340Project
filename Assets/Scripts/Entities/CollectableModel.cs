using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class CollectableModel : MonoBehaviour
{
    [SerializeField] private GameObject collectable;
    public VisualEffect VFX;

    public MeshRenderer meshRenderer;

    public void Collect()
    {
        meshRenderer.enabled = false;
        VFX.SetBool("ShouldRender", true);
        StartCoroutine(Delete());
    }

    IEnumerator Delete()
    {
        yield return new WaitForSeconds(0.1f);
        VFX.SetBool("ShouldRender", false);
        yield return new WaitForSeconds(0.9f);
        collectable.GetComponent<IPoolable>().Release();
    }
}
