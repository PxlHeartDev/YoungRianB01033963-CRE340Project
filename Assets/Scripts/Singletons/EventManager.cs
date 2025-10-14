using UnityEngine;

public class EventManager : MonoBehaviour
{
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

    public delegate void DamageEventHandler(int dmg, MonoBehaviour damageTarget, MonoBehaviour damageSource);
    public delegate void DeathEventHandler(MonoBehaviour target, MonoBehaviour source);

    public static DamageEventHandler TookDamage;
    public static DeathEventHandler Died;

    void Awake()
    {
        _instance = this;
    }
}
