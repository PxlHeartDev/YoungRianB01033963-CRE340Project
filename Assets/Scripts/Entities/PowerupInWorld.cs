using UnityEngine;

public class PowerupInWorld : MonoBehaviour, ICollectable, IPoolable
{
    private ObjectPool pool;

    [SerializeField] private CollectableModel model;
    [SerializeField] private AudioClip collectSFX;

    private Collider powerupCollider;

    public Powerup powerup;

    void Awake()
    {
        powerupCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        // If the player hit the coin
        if (other.CompareTag("Player"))
        {
            Car car = other.transform.parent.gameObject.GetComponent<Car>();

            Collect(car.gameObject);
        }
    }

    #region ICollectable
    public void Collect(GameObject source)
    {
        // Play sound
        AudioManager.Instance?.PlaySFXAtPoint(AudioManager.Source.Collectable, collectSFX, transform.position, 0.3f);

        // Do collection logic
        powerupCollider.enabled = false;
        model.Collect();
        EventManager.Collected?.Invoke(this, source);
    }
    #endregion

    #region IPoolable
    public void SetPool(ObjectPool _pool)
    {
        pool = _pool;
    }

    public GameObject GetObj()
    {
        return gameObject;
    }

    public void Reuse()
    {
        powerupCollider.enabled = true;
    }

    public void Release()
    {
        pool.ReturnToPool(this);
    }
    #endregion
}
