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

    public Player player;
    public SunMoon sunMoon;

    public static float standardDelta = 0.0f;

    public int score = 0;
    public float time = 0.0f;


    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);

        //Application.targetFrameRate = 60;
    }
    
    void Update()
    {
        standardDelta = Mathf.Clamp01(Time.deltaTime) * 60.0f;
        time += Time.deltaTime;
    }

    private void OnEnable()
    {
        EventManager.Collected += ItemCollected;
        sunMoon.sunsetTrigger += player.TurnOnLights;
        sunMoon.sunriseTrigger += player.TurnOffLights;
    }

    private void OnDisable()
    {
        EventManager.Collected -= ItemCollected;
        sunMoon.sunsetTrigger -= player.TurnOnLights;
        sunMoon.sunriseTrigger -= player.TurnOffLights;
    }

    private void ItemCollected(ICollectable item, GameObject source)
    {
        if (source == null) return;

        Car car = source.GetComponent<Car>();

        car.ItemCollected(item);

        if (item is Coin)
        {
            Coin coin = item as Coin;
            score += coin.scoreValue;
            Debug.Log("Collected a coin worth " + coin.scoreValue + " / New score: " + score + " / Combo: " + (car as Player).sequentialCoins);
        }
        else if (item is Powerup)
        {
            Powerup powerup = item as Powerup;
            // Other powerup logic
        }
    }
}
