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

    [Header ("GameObjects")]
    public Player player;
    public SunMoon sunMoon;
    public Camera cutsceneCamera;

    [HideInInspector] public static float standardDelta = 0.0f;

    [HideInInspector] public int score = 0;
    [HideInInspector] public float time = 0.0f;


    public enum State
    {
        MainMenu,
        Playing,
        Paused,
        Dead,
    }

    private State state = State.MainMenu;

    public System.Action<State> stateChanged;

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

    private void Start()
    {
        EventManager.Collected += ItemCollected;
        sunMoon.sunsetTrigger += player.TurnOnLights;
        sunMoon.sunriseTrigger += player.TurnOffLights;

        EventManager.GameStarted += StartGame;
        EventManager.GameQuit += QuitGame;

        UIManager.Instance.GameManagerReady();
        AudioManager.Instance.GameManagerReady();
    }

    private void OnDestroy()
    {
        EventManager.Collected -= ItemCollected;
        sunMoon.sunsetTrigger -= player.TurnOnLights;
        sunMoon.sunriseTrigger -= player.TurnOffLights;

        EventManager.GameStarted -= StartGame;
        EventManager.GameQuit -= QuitGame;
    }

    private void UpdateState(State newState)
    {
        state = newState;
        stateChanged?.Invoke(state);
    }

    private void ItemCollected(ICollectable item, GameObject source)
    {
        if (source == null) return;

        Car car = source.GetComponent<Car>();

        car.ItemCollected(item);

        if (item is Coin)
        {
            Coin coin = item as Coin;
            SetScore(score + coin.scoreValue);
        }
        else if (item is Powerup)
        {
            Powerup powerup = item as Powerup;
            // Other powerup logic
        }
    }

    private void SetScore(int value)
    {
        score = value;
        EventManager.ScoreUpdated?.Invoke(score);
    }

    private void StartGame()
    {
        cutsceneCamera.gameObject.SetActive(false);
        player.LockInputs(false);
        UpdateState(State.Playing);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game");
    }
    
}
