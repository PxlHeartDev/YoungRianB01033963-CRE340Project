using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Car
{
    public InputActionAsset actions;

    private InputAction m_leftAction;
    private InputAction m_rightAction;
    private InputAction m_gasAction;
    private InputAction m_reverseAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_leftAction = InputSystem.actions.FindAction("Left");
        m_rightAction = InputSystem.actions.FindAction("Right");
        m_gasAction = InputSystem.actions.FindAction("Gas");
        m_reverseAction = InputSystem.actions.FindAction("Brake");

        actions.FindActionMap("Player").Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_leftAction.WasPressedThisFrame())
        {
            SteerLeft();
        }
        else if (m_rightAction.WasPressedThisFrame())
        {
            SteerRight();
        }
        else if (m_leftAction.WasReleasedThisFrame() || m_rightAction.WasReleasedThisFrame())
        {
            SteerNone();
        }

        if (m_gasAction.IsPressed())
        {
            Gas();
        }

        if (m_reverseAction.IsPressed())
        {
            Reverse();
        }
    }
}
