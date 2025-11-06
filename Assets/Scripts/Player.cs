using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Car
{
    // Input action asset to assign in the editor
    public InputActionAsset actions;

    // Input actions
    private InputAction m_gasAction;
    private InputAction m_reverseAction;
    private InputAction m_steerAction;
    private InputAction m_jumpAction;

    // Whether the jump action is currently being held
    private bool jumpHeld;

    public int sequentialCoins = 0;
    private float sequentialCoinCooldown = 0.0f;

    public System.Action<int> CoinComboEnded;

    void Start()
    {
        // Get the input actions
        m_gasAction = InputSystem.actions.FindAction("Gas");
        m_reverseAction = InputSystem.actions.FindAction("Brake");
        m_steerAction = InputSystem.actions.FindAction("Move");
        m_jumpAction = InputSystem.actions.FindAction("Jump");

        // Enable the action map
        actions.FindActionMap("Player").Enable();
    }

    void Update()
    {
        ProcessInputs();

        SequentialCoinLogic();
    }

    public override void ItemCollected(ICollectable item)
    {
        base.ItemCollected(item);
        if (item is Coin)
        {
            sequentialCoins += 1;
            sequentialCoinCooldown = 1.0f;
        }
    }

    public float GetCoinPitch()
    {
        float pitch = 1.0f;

        // Calculate pitch
        if (sequentialCoins <= 20) pitch = 1.0f + 0.025f * sequentialCoins;
        else if (sequentialCoins <= 40) pitch = 1.5f;
        else if (sequentialCoins <= 60) pitch = 1.5f + 0.025f * (sequentialCoins - 40);
        else if (sequentialCoins <= 80) pitch = 2.0f;
        else if (sequentialCoins <= 100) pitch = 2.0f + 0.025f * (sequentialCoins - 80);
        else pitch = 2.5f;

        return pitch;
    }

    void ProcessInputs()
    {
        // Steer the car left/right
        Steer(m_steerAction.ReadValue<Vector2>().x);

        // Accelerate
        if (m_gasAction.IsPressed())
        {
            Gas();
        }

        // Reverse
        if (m_reverseAction.IsPressed())
        {
            Reverse();
        }

        if (m_jumpAction.IsPressed())
        {
            jumpHeld = true;
            JumpHold();
        }
        else if (!m_jumpAction.IsPressed() && jumpHeld)
        {
            jumpHeld = false;
            JumpRelease();
        }
    }

    private void SequentialCoinLogic()
    {
        if (sequentialCoinCooldown > 0.0f)
        {
            sequentialCoinCooldown -= Time.deltaTime;
        }
        else if (sequentialCoins > 0)
        {
            OnCoinComboEnded();
            sequentialCoins = 0;
        }
    }

    private void OnCoinComboEnded()
    {
        Debug.Log("Combo ended with " + sequentialCoins + " coins");


        CoinComboEnded?.Invoke(sequentialCoins);

        // To-do Implement in UI for visual combo indicator

        PlayComboSound();
    }

    private void PlayComboSound()
    {
        AudioClip comboSFX = null;

        if (sequentialCoins < 5) return;
        else if (sequentialCoins <= 20) comboSFX = Resources.Load("SFX/Combo/Combo1") as AudioClip;
        else if (sequentialCoins <= 40) comboSFX = Resources.Load("SFX/Combo/Combo2") as AudioClip;
        else comboSFX = Resources.Load("SFX/Combo/Combo3") as AudioClip;

        AudioManager.Instance.PlaySFXNonPositional(AudioManager.Source.Combo, comboSFX, 0.1f);
    }
}
