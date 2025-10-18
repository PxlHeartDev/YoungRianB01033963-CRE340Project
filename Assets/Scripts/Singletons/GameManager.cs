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


    void Awake()
    {
        _instance = this;
    }
    
    void Update()
    {
        time += Time.deltaTime;
    }
}
