using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header ("References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Animator comboAnimator;
    [SerializeField] private TextMeshProUGUI comboIndicatorText;

    private Player player;

    void OnEnable()
    {
        player = GameManager.Instance.player;

        healthBar.minValue = 0;
        healthBar.maxValue = player.maxHealth;

        EventManager.TookDamage += CarTookDamage;
        EventManager.Died += CarDied;
        EventManager.ScoreUpdated += ScoreUpdated;
        EventManager.Collected += ItemCollected;
        GameManager.Instance.player.CoinComboEnded += ComboEnded;
    }

    void OnDisable()
    {
        EventManager.TookDamage -= CarTookDamage;
        EventManager.Died -= CarDied;
        EventManager.ScoreUpdated -= ScoreUpdated;
        EventManager.Collected -= ItemCollected;
        GameManager.Instance.player.CoinComboEnded -= ComboEnded;
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

    #region Combo

    private void ItemCollected(ICollectable item, GameObject source)
    {
        if (source.CompareTag("Player") && (item is Coin || item is PowerupInWorld))
        {
            ComboUpdated(GameManager.Instance.player.sequentialCollects);
        }
    }

    private void ComboUpdated(int newCombo)
    {
        switch (newCombo) {
            case 1:
                comboAnimator.SetTrigger("Appear");
                comboIndicatorText.faceColor = Color.white;
                break;
            case 5:
                comboIndicatorText.faceColor = new Color(0.529f, 0.890f, 1.0f);
                break;
            case 20:
                comboIndicatorText.faceColor = new Color(0.929f, 0.529f, 1.0f);
                break;
            case 40:
                comboIndicatorText.faceColor = new Color(1.0f, 0.361f, 0.690f);
                break;
        }
        comboIndicatorText.text = $"COMBO\nX{newCombo}";
    }

    private void ComboEnded(int comboAmount)
    {
        if (comboAmount < 5)
            comboAnimator.SetTrigger("Disappear1");
        else if (comboAmount < 20)
            comboAnimator.SetTrigger("Disappear2");
        else if (comboAmount < 40)
            comboAnimator.SetTrigger("Disappear3");
        else
            comboAnimator.SetTrigger("Disappear4");
    }

    #endregion
}
