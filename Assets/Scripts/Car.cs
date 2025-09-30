using UnityEngine;

public class Car : MonoBehaviour
{
    public Rigidbody rb;
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
        if (!IsGrounded())
        {
            return;
        }
        Vector3 acceleration;
        acceleration = transform.right * gasStrength;
        rb.AddForce(acceleration);
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
