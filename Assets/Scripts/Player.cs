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

    private bool jumpHeld;

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
}
