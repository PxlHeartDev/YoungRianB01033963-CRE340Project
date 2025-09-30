using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Wheel[] wheels;

    public float gasStrength = 1.0f;


// Start is called once before the first execution of Update after the MonoBehaviour is created
void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Gas();
    }

    void Gas()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.ApplyGas();
        }
    }

    void Brake()
    {

    }

    void SteerLeft()
    {

    }

    void SteerRight()
    {

    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, 0.1f);
    }
}
