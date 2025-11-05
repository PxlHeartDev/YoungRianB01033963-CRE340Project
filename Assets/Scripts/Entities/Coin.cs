using UnityEngine;

public class Coin : MonoBehaviour, ICollectable
{
    [SerializeField] private CollectableModel model;
    [SerializeField] private AudioClip collectSFX;

    public int scoreValue = 1;

    void Awake()
    {
        model.VFX.SetInt("ParticleCount", 16);
        model.VFX.SetVector3("Offset", new Vector3(0.0f, 2.0f, 0.0f));
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

    public void Collect(GameObject source)
    {
        // Calculate pitch
        float pitch = 1.0f + 0.05f * GameManager.Instance.sequentialCoins;
        pitch = Mathf.Clamp(pitch, 1.0f, 2.0f);

        // Play sound
        AudioManager.Instance?.PlaySFXAtPoint(collectSFX, transform.position, 0.3f, pitch);
        
        // Do collection logic
        GetComponent<Collider>().enabled = false;
        model.Collect();
        EventManager.Collected?.Invoke(this, source);
    }
}
