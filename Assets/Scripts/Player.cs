using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Car
{
    public InputActionAsset actions;

    private InputAction m_leftAction;
    private InputAction m_rightAction;
    private InputAction m_gasAction;
    private InputAction m_reverseAction;

    private InputAction m_steerAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_leftAction = InputSystem.actions.FindAction("Left");
        m_rightAction = InputSystem.actions.FindAction("Right");
        m_gasAction = InputSystem.actions.FindAction("Gas");
        m_reverseAction = InputSystem.actions.FindAction("Brake");

        m_steerAction = InputSystem.actions.FindAction("Move");

        actions.FindActionMap("Player").Enable();
    }

    // Update is called once per frame
    void Update()
    {
        Steer(m_steerAction.ReadValue<Vector2>().x);

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
