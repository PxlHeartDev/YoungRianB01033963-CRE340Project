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

    public delegate void DamageEventHandler(int dmg, GameObject damageTarget, GameObject damageSource);
    public delegate void DeathEventHandler(GameObject target, GameObject source);
    public delegate void CollectEventHandler(ICollectable collected, GameObject source);

    public static DamageEventHandler TookDamage;
    public static DeathEventHandler Died;
    public static CollectEventHandler Collected;

    void Awake()
    {
        _instance = this;
    }
}
