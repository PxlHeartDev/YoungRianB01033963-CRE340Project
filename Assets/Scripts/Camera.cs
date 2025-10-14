using UnityEngine;

public class Camera : MonoBehaviour
{
    public Player player;

    void Start()
    {
        EventManager.TookDamage += CarTookDamage;
    }

    void Update()
    {
        
    }

    private void CarTookDamage(int dmg, MonoBehaviour target, MonoBehaviour source)
    {
        if(target == player)
        {
            // Camera effects
        }
    }
}
