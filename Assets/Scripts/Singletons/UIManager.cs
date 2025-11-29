using UnityEngine;

public class UIManager : MonoBehaviour
{

    // Singleton
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("UIManager instance is null");
            }

            return _instance;
        }
    }

    public MainMenu mainMenu;


    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        
    }

    void Update()
    {
        
    }
}
