using UnityEngine;

public class Coin : MonoBehaviour, ICollectable, IPoolable
{
    private ObjectPool pool;

    [SerializeField] private CollectableModel model;
    [SerializeField] private AudioClip collectSFX;

    private Collider coinCollider;

    public int scoreValue = 1;

    void Awake()
    {
        model.VFX.SetInt("ParticleCount", 16);
        model.VFX.SetVector3("Offset", new Vector3(0.0f, 2.0f, 0.0f));

        coinCollider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        // If the player hit the coin
        if (other.CompareTag("Player"))
        {
            Car car = other.transform.parent.gameObject.GetComponent<Car>();

            //car.Damage(1, gameObject);

            Collect(car.gameObject);
        }
    }

    #region ICollectable
    public void Collect(GameObject source)
    {
        if (source == null) return;

        Car car = source.GetComponent<Car>();
        if (car.CompareTag("Player"))
        {
            Player player = car as Player;

            // Play sound
            AudioManager.Instance?.PlaySFXAtPoint(AudioManager.Source.Collectable, collectSFX, transform.position, 1.0f, player.GetCoinPitch());

            // Do collection logic
            coinCollider.enabled = false;
            model.Collect();
            EventManager.Collected?.Invoke(this, source);
        }
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
        coinCollider.enabled = true;
    }

    public void Release()
    {
        pool.ReturnToPool(this);
    }
    #endregion
}
