using System.Collections;
using Unity.VisualScripting;
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
    [SerializeField] private Image fade;
    [SerializeField] private Animator fadeAnimator;

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        FadeShow();
    }

    void Update()
    {

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
}
