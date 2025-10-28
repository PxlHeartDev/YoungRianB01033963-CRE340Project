using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class CoinModel : MonoBehaviour
{
    [SerializeField] private Coin coin;
    [SerializeField] private VisualEffect sparkle;

    public void Collect()
    {
        GetComponent<MeshRenderer>().enabled = false;
        sparkle.SetBool("ShouldRender", true);
        StartCoroutine(Delete());
    }

    IEnumerator Delete()
    {
        yield return new WaitForSeconds(1.0f);
        Destroy(coin.gameObject);
    }
}
