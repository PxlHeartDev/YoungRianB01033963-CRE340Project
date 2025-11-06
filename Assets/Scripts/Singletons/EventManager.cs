using UnityEngine;

public class EventManager : MonoBehaviour
{
    // Singleton
    private static EventManager _instance;
    public static EventManager Instance {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("EventManager instance is null");
            }

            return _instance;
        }
    }

                      // dmg, damageTarget, damageSource
    public static System.Action<int, GameObject, GameObject> TookDamage;
                      // target,     source
    public static System.Action<GameObject, GameObject> Died;
                      // collected,    source
    public static System.Action<ICollectable, GameObject> Collected;

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }
}
