using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI quip;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private Button restart;
    [SerializeField] private Button backToMenu;

    private Player player;

    public List<string> quipList = new()
    {
        "Did you win?",
        "I think you died",
        "World record! (Not)",
        "Wait do that again, I wasn't looking",
        "Oops",
        "You should see your face right now",
        "Pro tip: Don't die",
        "...",
        "Where'd you get your license?!",
        "Back to the lobby",
        "That's gonna leave a mark",
        "It'll buff right out",
    };

    void OnEnable()
    {
        restart.onClick.AddListener(OnRestartClicked);
        backToMenu.onClick.AddListener(OnMainMenuClicked);
    }

    void OnDisable()
    {
        restart.onClick.RemoveListener(OnRestartClicked);
        backToMenu.onClick.RemoveListener(OnMainMenuClicked);
    }
    public void Show()
    {
        gameObject.SetActive(true);
        score.text = $"Final Score: {GameManager.Instance?.score}";
        ShowQuip();
    }

    private void ShowQuip()
    {
        quip.text = quipList[Random.Range(0, quipList.Count)];
    }

    #region Buttons
    private void OnRestartClicked()
    {
        SceneManager.LoadScene(0);
        EventManager.GameStarted?.Invoke();
    }

    private void OnMainMenuClicked()
    {

    }
    #endregion
}