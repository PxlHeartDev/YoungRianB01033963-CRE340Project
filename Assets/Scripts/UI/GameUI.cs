using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Slider healthBar;

    private Player player;

    void OnEnable()
    {
        player = GameManager.Instance.player;

        healthBar.minValue = 0;
        healthBar.maxValue = player.maxHealth;

        EventManager.TookDamage += CarTookDamage;
        EventManager.Died += CarDied;
    }

    void OnDisable()
    {
        EventManager.TookDamage -= CarTookDamage;
        EventManager.Died -= CarDied;
    }

    private void CarTookDamage(int amount, GameObject target, GameObject source)
    {
        if (target == player.gameObject)
        {
            healthBar.value = player.health;
        }
    }
    private void CarDied(GameObject target, GameObject source)
    {
        if (target == player.gameObject)
        {
            healthBar.value = 0;
        }
    }
}
