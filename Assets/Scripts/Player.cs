using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Car
{
    [Header("Assets")]
    public InputActionAsset actions;                        // Input action asset to assign in the editor

    // Input actions
    private InputAction m_gasAction;
    private InputAction m_reverseAction;
    private InputAction m_steerAction;
    private InputAction m_jumpAction;

    private bool jumpHeld = false;                          // Whether the jump action is currently being held

    public int sequentialCollects { get; private set; } = 0;   // How many coins have been collected in this combo
    private float sequentialCollectCooldown = 0.0f;

    public System.Action<int> CoinComboEnded;               // Event for when the coin combo ends

    public bool inputLocked = true;

    void Start()
    {
        // Get the input actions
        m_gasAction = InputSystem.actions.FindAction("Gas");
        m_reverseAction = InputSystem.actions.FindAction("Brake");
        m_steerAction = InputSystem.actions.FindAction("Move");
        m_jumpAction = InputSystem.actions.FindAction("Jump");

        // Enable the action map
        actions.FindActionMap("Player").Enable();
        
        // Reset wheels
        Steer(0.0f);

        LockInputs(inputLocked);
    }

    void Update()
    {
        // Only process inputs if this flag is false
        if (!inputLocked)
            ProcessInputs();

        SequentialCoinLogic();
    }

    public void LockInputs(bool isLocked)
    {
        inputLocked = isLocked;
        LockWheels(isLocked);
        rb.linearDamping = isLocked ? 100.0f : 0.1f;
    }

    public override void ItemCollected(ICollectable item)
    {
        base.ItemCollected(item);
        if (item is Coin || item is PowerupInWorld)
        {
            sequentialCollects += 1;
            sequentialCollectCooldown = 2.0f;
        }
    }

    public float GetCoinPitch()
    {
        float pitch = 1.0f;

        // Calculate pitch
        if (sequentialCollects <= 20) pitch = 1.0f + 0.025f * sequentialCollects;
        else if (sequentialCollects <= 40) pitch = 1.5f;
        else if (sequentialCollects <= 60) pitch = 1.5f + 0.025f * (sequentialCollects - 40);
        else if (sequentialCollects <= 80) pitch = 2.0f;
        else if (sequentialCollects <= 100) pitch = 2.0f + 0.025f * (sequentialCollects - 80);
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
        if (sequentialCollectCooldown > 0.0f)
        {
            sequentialCollectCooldown -= Time.deltaTime;
        }
        else if (sequentialCollects > 0)
        {
            OnCoinComboEnded();
            sequentialCollects = 0;
        }
    }

    private void OnCoinComboEnded()
    {
        CoinComboEnded?.Invoke(sequentialCollects);

        // To-do Implement in UI for visual combo indicator

        PlayComboSound();
    }

    private void PlayComboSound()
    {
        AudioClip comboSFX = null;

        if (sequentialCollects < 5) return;
        else if (sequentialCollects < 20) comboSFX = Resources.Load("SFX/Combo/Combo1") as AudioClip;
        else if (sequentialCollects < 40) comboSFX = Resources.Load("SFX/Combo/Combo2") as AudioClip;
        else comboSFX = Resources.Load("SFX/Combo/Combo3") as AudioClip;

        AudioManager.Instance.PlaySFXNonPositional(AudioManager.Source.Combo, comboSFX, 1.0f);
    }

    public bool IsReversing()
    {
        return m_reverseAction.IsPressed();
    }
}
