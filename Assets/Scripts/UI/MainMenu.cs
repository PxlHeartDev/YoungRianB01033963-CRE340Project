using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header ("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    [Header ("Other")]
    [SerializeField] private Animator animator;

    public System.Action OnPlay;
    public System.Action OnQuit;

    public float thing;

    void Start()
    {
        playButton.image.alphaHitTestMinimumThreshold = 0.5f;
        exitButton.image.alphaHitTestMinimumThreshold = 0.5f;
    }

    void OnEnable()
    {
        // Connect events
        playButton.onClick.AddListener(OnPlayClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    void OnDisable()
    {
        // Disconnect events
        playButton.onClick.RemoveListener(OnPlayClicked);
        exitButton.onClick.RemoveListener(OnExitClicked);
    }

    private void OnPlayClicked()
    {
        DisableAllButtons();
        animator.SetTrigger("StartGame");
        UIManager.Instance?.FadeHide();
    }

    private void OnExitClicked()
    {
        DisableAllButtons();
        OnQuit?.Invoke();
        EventManager.GameQuit?.Invoke();
    }

    private void DisableAllButtons()
    {
        playButton.interactable = false;
        exitButton.interactable = false;
    }

    private void AnimationComplete()
    {
        gameObject.SetActive(false);
        UIManager.Instance?.FadeShow();
        OnPlay?.Invoke();
        EventManager.GameStarted?.Invoke();
    }
}
