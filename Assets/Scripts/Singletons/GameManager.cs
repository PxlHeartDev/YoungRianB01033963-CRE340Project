using System.Collections;
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

    private GameObject playerStartTransform;


    public enum State
    {
        MainMenu,
        Playing,
        Paused,
        Dead,
    }

    public State state = State.MainMenu;

    public System.Action<State> stateChanged;

    void Awake()
    {
        _instance = this;

        playerStartTransform = new GameObject();
        playerStartTransform.transform.SetPositionAndRotation(player.transform.position, player.transform.rotation);

        //Application.targetFrameRate = 60;
    }

    private void Start()
    {
        EventManager.Collected += ItemCollected;
        EventManager.TookDamage += CarTookDamage;
        EventManager.Died += CarDied;
        sunMoon.sunsetTrigger += player.TurnOnLights;
        sunMoon.sunriseTrigger += player.TurnOffLights;

        EventManager.GameStarted += StartGame;
        EventManager.GameQuit += QuitGame;

        UIManager.Instance.GameManagerReady();
        AudioManager.Instance.GameManagerReady();
    }

    void Update()
    {
        standardDelta = Mathf.Clamp01(Time.deltaTime) * 60.0f;
        time += Time.deltaTime;
    }

    private void OnDestroy()
    {
        EventManager.Collected -= ItemCollected;
        EventManager.TookDamage -= CarTookDamage;
        EventManager.Died -= CarDied;
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
        else if (item is PowerupInWorld)
        {
            PowerupInWorld powerup = item as PowerupInWorld;
            SetScore(score + powerup.powerup.scoreValue);
            // Other powerup logic
        }
    }

    // Any car took damage
    private void CarTookDamage(int dmg, GameObject target, GameObject source)
    {
        // The player took damage
        if (target == player.gameObject)
        {

        }
    }

    private void CarDied(GameObject target, GameObject source)
    {
        if (target == player.gameObject)
        {
            state = State.Dead;
            stateChanged?.Invoke(state);
            Time.timeScale = 0.2f;

            StartCoroutine(DiedCoroutine());
        }
    }

    private void SetScore(int value)
    {
        score = value;
        EventManager.ScoreUpdated?.Invoke(score);
    }

    private void StartGame()
    {
        SetScore(0);
        cutsceneCamera.gameObject.SetActive(false);
        player.LockInputs(false);
        if (state == State.Dead)
        {
            AudioManager.Instance?.SetUpMusic();
        }
        UpdateState(State.Playing);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private IEnumerator DiedCoroutine()
    {
        for (int i = 4; i < 20; i++)
        {
            Time.timeScale = i * 0.05f;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
}
