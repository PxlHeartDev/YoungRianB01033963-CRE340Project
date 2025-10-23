using UnityEngine;
using UnityEngine.VFX;

public class CoinModel : MonoBehaviour
{
    [SerializeField] private Coin coin;
    [SerializeField] private VisualEffect sparkle;

    private void AnimateEnd()
    {
        GetComponent<MeshRenderer>().enabled = false;
        sparkle.SetBool("ShouldRender", true);
    }

    private void Delete()
    {
        Destroy(coin.gameObject);
    }
}
