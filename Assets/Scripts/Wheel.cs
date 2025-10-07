using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class WheelOld : MonoBehaviour
{
    //[SerializeField] private Rigidbody rbBody;
    //[SerializeField] private GameObject mesh;

    //[SerializeField] private float suspensionStrength = 100.0f;
    //[SerializeField] private float suspensionDampening = 15.0f;
    //[SerializeField] private float suspensionRestOffset = 0.3f;

    //public bool left = false;
    //public bool front = false;

    //private bool onGround = false;

    //private RaycastHit hit;

    //// Temp, for colour debug
    //private Material meshMaterial;

    //void Start()
    //{
    //    meshMaterial = mesh.GetComponent<MeshRenderer>().material;
    //}

    //void Update()
    //{

    //}


    //void FixedUpdate()
    //{
    //    ShootRay();
    //    if(hit.collider != null)
    //    {
    //        transform.position = hit.point;
    //    }
    //    onGround = IsGrounded();

    //    Vector3 suspensionForce = transform.up * CalculateSuspension();
    //    Vector3 antiSlipForce = transform.right * CalculateAntiSlip();

    //    if (onGround)
    //    {
    //        ApplyWheelForce(suspensionForce);
    //        ApplyWheelForce(antiSlipForce);
    //    }
    //}

    //public void ApplyGas(float gasStrength)
    //{
    //    if (!onGround)
    //    {
    //        meshMaterial.color = new Color(1, 0, 0);
    //        return;
    //    }

    //    //mesh.transform.Rotate(new Vector3(0, Vector3.Dot(rbBody.linearVelocity, transform.right), 0));
    //    meshMaterial.color = new Color(0, 1, 0);

    //    ApplyWheelForce(CalculateAcceleration(gasStrength));
    //}
    
    //// <0.0 is left, >0.0 is right
    //public void Steer(float degrees)
    //{
    //    if(front)
    //    {
    //        Vector3 rot = transform.localEulerAngles;
    //        rot.y = degrees;
    //        transform.localEulerAngles = rot;
    //    }
    //}

    //private void ApplyWheelForce(Vector3 force)
    //{
    //    rbBody.AddForceAtPosition(force, transform.position + transform.up);
    //}

    //private bool IsGrounded()
    //{
    //    return Physics.Raycast(transform.position + transform.up * 0.1f, -rbBody.transform.up, 0.3f);
    //}


    //private void ShootRay()
    //{
    //    Physics.Raycast(transform.position + transform.up, -rbBody.transform.up, out hit, 0.5f);
    //}

    //private float SideMult()
    //{
    //    return left ? 1.0f : -1.0f;
    //}

    //private Vector3 CalculateAcceleration(float gasStrength)
    //{
    //    Vector3 acceleration = transform.right * gasStrength;
    //    if(!front)
    //    {
    //        acceleration *= 0.5f;
    //    }
    //    return acceleration;
    //}

    //private float CalculateSuspension()
    //{
    //    Vector3 springAxis = transform.up;
    //    Vector3 wheelWorldVelocity = rbBody.GetPointVelocity(transform.position);
    //    float offset = suspensionRestOffset - hit.distance;


    //    float vel = Vector3.Dot(springAxis, wheelWorldVelocity);
    //    float force = (offset * suspensionStrength) - (vel * suspensionDampening);

    //    Debug.Log($"Force: {force}");

    //    return force;
    //}

    //private float CalculateAntiSlip()
    //{
    //    return 0.0f;
    //}
}
