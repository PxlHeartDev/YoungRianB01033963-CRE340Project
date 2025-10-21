using UnityEngine;

public class Coin : MonoBehaviour, ICollectable
{
    public int scoreValue = 1;

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        Collect(collision.collider.gameObject);
    }

    public void Collect(GameObject source)
    {
        EventManager.Collected?.Invoke(this, source);
        Destroy(this);
    }
}
