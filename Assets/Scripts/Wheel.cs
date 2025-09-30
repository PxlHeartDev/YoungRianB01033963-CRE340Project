using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Rigidbody rbBody;
    public float gasStrength = 1.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.forward, 0.5f);
    }

    public void ApplyGas()
    {
        if (!IsGrounded())
        {
            return;
        }
        Debug.Log("Driving");
        Vector3 acceleration;
        acceleration = -rbBody.transform.right * gasStrength;
        rbBody.AddForceAtPosition(acceleration, transform.position);
    }

    private float CalculateSuspension()
    {
        return 0.0f;
    }

    private float CalculateAntiSlip()
    {
        return 0.0f;
    }
}
