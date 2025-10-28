using UnityEngine;
using UnityEngine.VFX;

public class Coin : MonoBehaviour, ICollectable
{
    [SerializeField] private CoinModel model;
    private Animator anim;

    public int scoreValue = 1;

    void Awake()
    {
        anim = model.GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Collect(other.gameObject);
        }
    }

    public void Collect(GameObject source)
    {
        GetComponent<Collider>().enabled = false;
        model.Collect();
        EventManager.Collected?.Invoke(this, source);
    }
}
