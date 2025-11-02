using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameManager instance is null");
            }

            return _instance;
        }
    }

    public int score = 0;
    public float time = 0.0f;

    public int sequentialCoins = 0;
    private float sequentialCoinCooldown = 0.0f; 


    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }
    
    void Update()
    {
        time += Time.deltaTime;
        SequentialCoinLogic();
    }

    private void OnEnable()
    {
        EventManager.Collected += ItemCollected;
    }

    private void OnDisable()
    {
        EventManager.Collected -= ItemCollected;
    }

    private void ItemCollected(ICollectable item, GameObject source)
    {
        if (item is Coin)
        {
            Coin coin = item as Coin;
            score += coin.scoreValue;
            Debug.Log("Collected a coin worth " + coin.scoreValue + " / New score: " + score);

            sequentialCoins++;
            sequentialCoinCooldown = 1.0f;
        }
        else if (item is Powerup)
        {
            Powerup powerup = item as Powerup;
            // Other powerup logic
        }
    }

    private void SequentialCoinLogic()
    {
        if (sequentialCoinCooldown > 0.0f)
            sequentialCoinCooldown -= Time.deltaTime;
        else
            sequentialCoins = 0;
    }
}
