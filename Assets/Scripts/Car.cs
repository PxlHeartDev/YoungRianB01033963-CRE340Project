using Unity.VisualScripting;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] protected Rigidbody rb;

    public float gasStrength = 1.0f;

    private float steerDegrees = 90.0f;

    public GameObject wheelPrefab;
    public Vector2 wheelDistance = new Vector2(2, 2);

    Wheel[] wheels = new Wheel[4];
    float rpm = 0;

    float maxSuspensionLength = 1.3f;

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            wheels[i] = new Wheel();
            wheels[i].rbCar = rb;
            wheels[i].position = transform.position;
            wheels[i].restPosition = wheels[i].position;

            wheels[i].transform = transform;

            if (i == 0 || i == 1) wheels[i].front = true;
            wheels[i].prefab = Instantiate(wheelPrefab, wheels[i].position, Quaternion.identity);
            wheels[i].prefab.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        }
    }

    void FixedUpdate()
    {
        wheels[0].position = transform.right * wheelDistance.x + transform.forward * wheelDistance.y; //front right
        wheels[1].position = transform.right * -wheelDistance.x + transform.forward * wheelDistance.y; //front left
        wheels[2].position = transform.right * wheelDistance.x + transform.forward * -wheelDistance.y; //back right
        wheels[3].position = transform.right * -wheelDistance.x + transform.forward * -wheelDistance.y; //back left

        for (int i = 0; i < 4; i++)
        {
            wheels[i].Update();

            rpm += ((rpm * Mathf.PI) + -transform.InverseTransformDirection(rb.linearVelocity).z) * Time.deltaTime * 0.1f;

            wheels[i].isOnGround = wheels[i].ShootRay();
            RaycastHit hit = wheels[i].rayHit;
            if (hit.collider != null)
            {
                wheels[i].prefab.transform.position = hit.point;

                wheels[i].prefab.transform.GetChild(0).Rotate(0, -rpm, 0, Space.Self);
            }
            else
            {
                wheels[i].prefab.transform.position = transform.position + wheels[i].position - transform.up * (maxSuspensionLength);
            }
        }
    }

    protected void Gas()
    {
        for (int i = 0; i < 4; i++)
        {
            rpm -= 1.0f * Time.deltaTime;
            if (wheels[i].isOnGround) 
            {
                wheels[i].ApplyGas(gasStrength);
            }
        }
    }

    protected void Reverse()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.ApplyGas(-gasStrength * 0.75f);
        }
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
}


class Wheel
{
    public Transform transform;
    public Vector3 position;
    public Vector3 restPosition;
    public GameObject prefab;
    public RaycastHit rayHit;
    public Rigidbody rbCar;

    public bool isOnGround;

    public bool front;

    float maxSuspensionLength = 1.3f;
    float suspensionMultiplier = 50.0f;
    float suspensionDamping = 5.0f;
    float maxTraction = 240.0f;
    float traction = 120.0f;

    [SerializeField] private float suspensionStrength = 100.0f;
    [SerializeField] private float suspensionDampening = 15.0f;
    [SerializeField] private float suspensionLength = 1.0f;

    Vector3 suspensionForce;
    Vector3 tractionForce;

    float rpm = 0.0f;

    public void Update()
    {
        transform.position = position;
        if (isOnGround)
        {
            ApplyWheelForce(transform.up * CalculateSuspension());

            //float vel = Vector3.Dot(rbCar.transform.up, rbCar.GetPointVelocity(position));
            //suspensionForce = Vector3.ClampMagnitude(rbCar.transform.up * ((Mathf.Clamp(maxSuspensionLength - rayHit.distance, 0, 3) * suspensionMultiplier) - (vel * suspensionDamping)), 100.0f);
            //ApplyWheelForce(suspensionForce);
            //Debug.Log(suspensionForce);
        }
        if (position != null)
        {
            prefab.transform.position = position;
        }
    }

    private float CalculateSuspension()
    {
        Vector3 springAxis = transform.up;
        Vector3 wheelWorldVelocity = rbCar.GetPointVelocity(transform.position);
        float offset = suspensionLength - rayHit.distance;


        float vel = Vector3.Dot(springAxis, wheelWorldVelocity);
        float force = (offset * suspensionStrength) - (vel * suspensionDampening);

        return force;
    }

    public void ApplyGas(float gasStrength)
    {
        if (front && isOnGround)
        {
            tractionForce = Vector3.ClampMagnitude(traction * (prefab.transform.right * -prefab.transform.InverseTransformDirection(rbCar.linearVelocity).x + prefab.transform.forward * ((rpm * Mathf.PI) + -prefab.transform.InverseTransformDirection(rbCar.linearVelocity).z)), maxTraction);
            tractionForce = Quaternion.AngleAxis(prefab.transform.eulerAngles.y, Vector3.up) * tractionForce;

            ApplyWheelForce(tractionForce);
        }
    }

    public void Steer(float degrees)
    {
        if (front)
        {
            prefab.transform.eulerAngles = new Vector3(prefab.transform.eulerAngles.x, prefab.transform.eulerAngles.y + degrees, prefab.transform.eulerAngles.z);
        }
        else
        { 
            prefab.transform.rotation = prefab.transform.rotation; 
        }
    }

    private void ApplyWheelForce(Vector3 force)
    {
        rbCar.AddForceAtPosition(force, rayHit.point);
    }


    public bool ShootRay()
    {
        return Physics.Raycast(restPosition + rbCar.transform.up * 0.1f, -rbCar.transform.up, out rayHit, maxSuspensionLength);
    }
}