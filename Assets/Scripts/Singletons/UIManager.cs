using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public GameUI gameUI;
    public GameOver gameOver;
    [SerializeField] private Image fade;
    [SerializeField] private Animator fadeAnimator;

    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        FadeShow();
    }

    public void GameManagerReady()
    {
        GameManager.Instance.stateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        GameManager.Instance.stateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameManager.State newState)
    {
        HideAllUI();
        switch (newState)
        {
            case GameManager.State.MainMenu:
                mainMenu.gameObject.SetActive(true);
                break;
            case GameManager.State.Playing:
                gameUI.gameObject.SetActive(true);
                break;
            case GameManager.State.Paused:
                break;
            case GameManager.State.Dead:
                StartCoroutine(DiedCoroutine());
                break;
        }
    }

    private void HideAllUI()
    {
        mainMenu.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(false);
    }

    public void FadeHide()
    {
        fadeAnimator.ResetTrigger("FadeFromBlack");
        fadeAnimator.SetTrigger("FadeToBlack");
    }
    
    public void FadeShow()
    {
        fadeAnimator.ResetTrigger("FadeToBlack");
        fadeAnimator.SetTrigger("FadeFromBlack");
    }

    private IEnumerator DiedCoroutine()
    {
        yield return new WaitForSeconds(0.8f);
        FadeHide();
        yield return new WaitForSeconds(1.0f);
        FadeShow();
        gameOver.Show();
    }
}
