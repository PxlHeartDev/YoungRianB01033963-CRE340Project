using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header ("References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI scoreText;

    private Player player;

    void OnEnable()
    {
        player = GameManager.Instance.player;

        healthBar.minValue = 0;
        healthBar.maxValue = player.maxHealth;

        EventManager.TookDamage += CarTookDamage;
        EventManager.Died += CarDied;
        EventManager.ScoreUpdated += ScoreUpdated;
    }

    void OnDisable()
    {
        EventManager.TookDamage -= CarTookDamage;
        EventManager.Died -= CarDied;
        EventManager.ScoreUpdated -= ScoreUpdated;
    }

    #region Health
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
    #endregion

    #region Score
    private void ScoreUpdated(int newScore)
    {
        scoreText.text = $"Score: {newScore.ToString()}";
    }
    
    #endregion
}
