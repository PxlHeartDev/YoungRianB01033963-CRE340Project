using UnityEngine;

public class Coin : MonoBehaviour, ICollectable
{
    [SerializeField] private GameObject model;
    private Animation anim;

    public int scoreValue = 1;

    void Awake()
    {
        anim = model.GetComponent<Animation>();
        //anim.Play();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.ToString());
        if (other.gameObject.tag == "Player")
        {
            Collect(other.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.ToString()); 
        if (collision.collider.gameObject.tag == "Player")
        {
            Collect(collision.collider.gameObject);
        }
    }

    public void Collect(GameObject source)
    {
        EventManager.Collected?.Invoke(this, source);
        Destroy(this);
    }
}
