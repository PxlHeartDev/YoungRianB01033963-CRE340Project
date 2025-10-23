using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour, ICollectable
{
    [SerializeField] private GameObject model;
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
        EventManager.Collected?.Invoke(this, source);
        anim.SetTrigger("Collected");
    }
}
