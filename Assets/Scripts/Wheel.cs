using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Wheel : MonoBehaviour
{
    // Expose the car rigid
    [SerializeField] private Rigidbody rbBody;
    [SerializeField] private GameObject mesh;

    public bool left = false;
    public bool front = false;

    // Temp, for colour debug
    private Material meshMaterial;

    void Start()
    {
        meshMaterial = mesh.GetComponent<MeshRenderer>().material;
    }

    void Update()
    {

    }


    private void FixedUpdate()
    {
        Vector3 suspensionForce = transform.up * CalculateSuspension();
        Vector3 antiSlipForce = transform.right * CalculateAntiSlip();

        ApplyWheelForce(suspensionForce);
        ApplyWheelForce(antiSlipForce);
    }

    public void ApplyGas(float gasStrength)
    {
        if (!IsGrounded())
        {
            meshMaterial.color = new Color(1, 0, 0);
            return;
        }

        //mesh.transform.Rotate(new Vector3(0, Vector3.Dot(rbBody.linearVelocity, transform.right), 0));
        meshMaterial.color = new Color(0, 1, 0);

        ApplyWheelForce(CalculateAcceleration(gasStrength));
    }
    
    // <0.0 is left, >0.0 is right
    public void Steer(float amount)
    {
        if(front)
        {
            transform.localEulerAngles = new Vector3(transform.rotation.x, amount, transform.rotation.z);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + transform.up * 0.1f, -rbBody.transform.up, 0.2f);
    }

    private void ApplyWheelForce(Vector3 force)
    {
        rbBody.AddForceAtPosition(force, transform.position + transform.up);
    }

    private float SideMult()
    {
        return left ? 1.0f : -1.0f;
    }

    private Vector3 CalculateAcceleration(float gasStrength)
    {
        Vector3 acceleration = transform.right * gasStrength;
        if(!front)
        {
            acceleration *= 0.5f;
        }
        return acceleration;
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
