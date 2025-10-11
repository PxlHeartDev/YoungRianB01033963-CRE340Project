using Unity.VisualScripting;
using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] protected Rigidbody rb;
    [SerializeField] private GameObject wheelPrefab;
    [SerializeField] private Vector2 wheelDistance = new Vector2(2, 2);

    public bool debug = false;

    Wheel[] wheels = new Wheel[4];

    public float gasStrength = 40.0f;
    private float rpm = 0.0f;

    void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            wheels[i] = new Wheel();
            wheels[i].rbCar = rb;
            wheels[i].wheelTransform = new GameObject();
            wheels[i].wheelTransform.transform.position = transform.position;
            wheels[i].debug = debug;

            switch (i) {
                case (0):
                    wheels[0].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * wheelDistance.y;//front right
                    break;
                case (1):
                    wheels[1].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * wheelDistance.y;//front left
                    break;
                case (2):
                    wheels[2].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * -wheelDistance.y;//back right
                    break;
                case (3):
                    wheels[3].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * -wheelDistance.y;//back left
                    break;
            }
            wheels[i].restPosition = wheels[i].wheelTransform.transform.position;

            if (i == 0 || i == 2) wheels[i].front = true;
            wheels[i].prefab = Instantiate(wheelPrefab, wheels[i].wheelTransform.transform.position, Quaternion.identity, transform);
            wheels[i].prefab.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);

            wheels[i].PostInitialize();
        }
    }

    void FixedUpdate()
    {
        wheels[0].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * wheelDistance.y; //front right
        wheels[1].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * wheelDistance.y; //front left
        wheels[2].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * -wheelDistance.y; //back right
        wheels[3].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * -wheelDistance.y; //back left


        rpm -= ((rpm * Mathf.PI) + -transform.InverseTransformDirection(rb.linearVelocity).z) * Time.deltaTime * 0.1f;

        for (int i = 0; i < 4; i++)
        {
            wheels[i].wheelTransform.transform.rotation = transform.rotation;
            if (debug)
            {
                Debug.DrawRay(wheels[i].wheelTransform.transform.position, wheels[i].wheelTransform.transform.forward, Color.blue);
                Debug.DrawRay(wheels[i].wheelTransform.transform.position, wheels[i].wheelTransform.transform.right, Color.red);
                Debug.DrawRay(wheels[i].wheelTransform.transform.position, wheels[i].wheelTransform.transform.up, Color.green);
            }
            wheels[i].rpm = rpm;
            wheels[i].Update();
        }
    }

    protected void Gas()
    {
        for (int i = 0; i < 4; i++)
        {
            rpm += 1.0f * Time.deltaTime;
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

    protected void Steer(float ratio)
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.Steer(ratio);
        }
    }
}


class Wheel
{
    public GameObject wheelTransform;
    public Vector3 restPosition;
    public GameObject prefab;
    public RaycastHit rayHit;
    public Rigidbody rbCar;

    public bool front;

    public bool debug = false;

    private float wheelGrip = 0.0f;

    private readonly float maxTraction = 240.0f;
    private readonly float frictionCoefficient = 1.2f;
    private readonly float frontGrip = 30.0f;
    private readonly float rearGrip = 10.0f;

    private readonly float maxSteerDegrees = 45.0f;
    private readonly float maxSuspensionLength = 2.0f;
    private readonly float suspensionStrength = 200.0f;
    private readonly float suspensionDampening = 10.0f;
    private readonly float suspensionRestLength = 1.3f;

    public bool isOnGround;

    public float rpm = 0.0f;
    private float currentSteer = 0.0f;

    private float defaultPrefabRotation = 0.0f;

    public void PostInitialize()
    {
        defaultPrefabRotation = prefab.transform.localEulerAngles.y;
        wheelGrip = front ? frontGrip : rearGrip;
    }

    public void Update()
    {
        isOnGround = ShootRay();

        wheelTransform.transform.Rotate(new Vector3(0.0f, currentSteer, 0.0f));

        if (isOnGround)
        {
            ApplyWheelForce(CalculateSuspension());
            ApplyWheelForce(CalculateFriction());
            ApplyWheelForce(CalculateAntiSlip());
        }



        if (rayHit.collider != null)
        {
            prefab.transform.position = rayHit.point;
        }
        else
        {
            prefab.transform.position = Vector3.Lerp(prefab.transform.position, (wheelTransform.transform.position - rbCar.transform.up * 0.3f) - rbCar.transform.up * (maxSuspensionLength), 0.2f);
        }


        prefab.transform.GetChild(0).Rotate(0, rpm, 0, Space.Self);
    }

    public void ApplyGas(float gasStrength)
    {
        if (front && isOnGround)
        {
            Vector3 accelerationForce = Vector3.ClampMagnitude(gasStrength * wheelTransform.transform.right, maxTraction);

            ApplyWheelForce(accelerationForce);
        }
    }

    public void Steer(float ratio)
    {
        if (front)
        {
            float degrees = ratio * maxSteerDegrees;
            currentSteer = degrees;
            prefab.transform.eulerAngles = new Vector3(prefab.transform.eulerAngles.x, defaultPrefabRotation + degrees, prefab.transform.eulerAngles.z);
        }
        else
        { 
            prefab.transform.rotation = prefab.transform.rotation; 
        }
    }

    public bool ShootRay()
    {
        if (debug) {
            Debug.DrawRay(wheelTransform.transform.position - rbCar.transform.up * 0.3f, -rbCar.transform.up, Color.green);
            Debug.DrawRay(rayHit.point, rayHit.normal, Color.purple);
        }
        return Physics.Raycast(wheelTransform.transform.position - rbCar.transform.up * 0.3f, -rbCar.transform.up, out rayHit, maxSuspensionLength);
    }

    private void ApplyWheelForce(Vector3 force)
    {
        rbCar.AddForceAtPosition(force, wheelTransform.transform.position);
    }

    private Vector3 CalculateSuspension()
    {
        Vector3 springAxis = wheelTransform.transform.up;
        Vector3 wheelWorldVelocity = rbCar.GetPointVelocity(wheelTransform.transform.position);
        float offset = suspensionRestLength - rayHit.distance;


        float vel = Vector3.Dot(springAxis, wheelWorldVelocity);
        float force = (offset * suspensionStrength) - (vel * suspensionDampening);
        Vector3 suspension = wheelTransform.transform.up * force;

        return suspension;
    }

    private Vector3 CalculateFriction()
    {
        Vector3 vel = rbCar.GetPointVelocity(wheelTransform.transform.position);

        Vector3 friction = -vel * frictionCoefficient;

        return friction;
    }

    private Vector3 CalculateAntiSlip()
    {
        float steerVelDot = Vector3.Dot(wheelTransform.transform.forward, rbCar.GetPointVelocity(wheelTransform.transform.position).normalized);

        float negationForce = -steerVelDot * wheelGrip;

        Vector3 antiSlip = wheelTransform.transform.forward * negationForce;

        return antiSlip;
    }
}