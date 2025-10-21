using UnityEngine;

public class PowerupInWorld : MonoBehaviour, ICollectable
{
    private Mesh mesh;

    public Powerup powerup;

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
