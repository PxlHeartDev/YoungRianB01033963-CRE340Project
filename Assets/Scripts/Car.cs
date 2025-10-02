using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Wheel[] wheels;

    public float gasStrength = 1.0f;

    private float steerDegrees = 90.0f;


// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    protected void Gas()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.ApplyGas(gasStrength);
        }
    }

    protected void Brake()
    {

    }

    protected void SteerLeft()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.Steer(-steerDegrees);
        }
    }

    protected void SteerRight()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.Steer(steerDegrees);
        }
    }

    protected void SteerNone()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.Steer(0.0f);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, 0.1f);
    }
}
