using UnityEngine.VFX;
using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour, IDamageable
{
    // Editor things
    [SerializeField] public Rigidbody rb;
    [SerializeField] private GameObject wheelPrefab;
    [SerializeField] private VisualEffect wheelRubbleVFXPrefab;
    [SerializeField] private VisualEffect driftLinesVFXPrefab;
    [SerializeField] private Vector2 wheelDistance = new Vector2(1.85f, 0.95f);

    // Enables debug gizmos
    public bool debug = false;

    // Stores references to the wheels
    private Wheel[] wheels = new Wheel[4];

    // Forward acceleration strength
    public float gasStrength = 40.0f;

    // Coefficient for how strong reversing is relative to gasStrength
    public float reversePercentage = 0.75f;

    // Controls the visual wheel spin
    private float rpm = 0.0f;

    // Tracker for how many wheels are grounded
    public int groundedWheels { get; private set; } = 0;

    // Health
    public int maxHealth { get; private set; } = 10;
    public int health { get; private set; } = 10;

    void Awake()
    {
        // Generate the wheels
        for (int i = 0; i < 4; i++)
        {
            // Create the objects and assign some values
            wheels[i] = new Wheel();
            wheels[i].rbCar = rb;
            wheels[i].wheelTransform = new GameObject($"Wheel {i}");
            //wheels[i].wheelTransform.transform.SetParent(transform);
            wheels[i].debug = debug;

            // Set the initial transforms
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

            // Set front wheels
            if (i == 0 || i == 2) wheels[i].front = true;

            // Instantiate the visual mesh
            wheels[i].prefab = Instantiate(wheelPrefab, wheels[i].wheelTransform.transform.position, Quaternion.identity, wheels[i].wheelTransform.transform);
            wheels[i].prefab.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);

            wheels[i].wheelRubbleVFX = Instantiate(wheelRubbleVFXPrefab, wheels[i].prefab.transform);
            wheels[i].driftLinesVFX = Instantiate(driftLinesVFXPrefab, wheels[i].prefab.transform);
            wheels[i].driftLinesVFX.transform.position += (wheels[i].wheelTransform.transform.up * 0.1f);// + (wheels[i].wheelTransform.transform.forward * -0.3f);

            // Trigger the post-init function
            wheels[i].PostInitialize();
        }
    }

    void FixedUpdate()
    {
        // Update the transforms every frame to keep them relative to the body's transform
        wheels[0].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * wheelDistance.y; //front right
        wheels[1].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * wheelDistance.y; //front left
        wheels[2].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * -wheelDistance.y; //back right
        wheels[3].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * -wheelDistance.y; //back left

        // Dampen rpm
        rpm -= ((rpm * Mathf.PI) + -transform.InverseTransformDirection(rb.linearVelocity).z) * Time.deltaTime * 0.1f;

        groundedWheels = 0;
        // Update every wheel
        foreach (Wheel wheel in wheels)
        {
            // Sync the transforms' rotation
            wheel.wheelTransform.transform.rotation = transform.rotation;
            // Draw debug gizmos
            if (debug)
            {
                Debug.DrawRay(wheel.wheelTransform.transform.position, wheel.wheelTransform.transform.forward, Color.blue);
                Debug.DrawRay(wheel.wheelTransform.transform.position, wheel.wheelTransform.transform.right, Color.red);
                Debug.DrawRay(wheel.wheelTransform.transform.position, wheel.wheelTransform.transform.up, Color.green);
            }
            // Sync spin
            wheel.rpm = rpm;

            // Call the update function on each wheel
            wheel.UpdatePhysics();

            // Updated tracker
            if (wheel.isOnGround) groundedWheels++;
        }
    }

    // Accelerate the car
    protected void Gas()
    {
        foreach (Wheel wheel in wheels)
        {
            rpm += 1.0f * Time.deltaTime;
            if (wheel.isOnGround) 
            {
                wheel.ApplyGas(gasStrength);
            }
        }
    }

    // Reverse the car
    protected void Reverse()
    {
        if (groundedWheels == 0)
        {
            rb.angularVelocity = rb.angularVelocity *= 0.99f;
            return;
        }

        foreach (Wheel wheel in wheels)
        {
            wheel.ApplyGas(-gasStrength * reversePercentage);
        }
    }
    

    // Steer the car left/right as a percentage (and direction (-1 < r < 0 for left, 0 < r < 1 for right))
    // of the max steer degrees (stored in the Wheel class)
    protected void Steer(float ratio)
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.Steer(ratio);
        }
    }

    protected void JumpHold()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.JumpHold();
        }
    }

    protected void JumpRelease()
    {
        foreach (Wheel wheel in wheels)
        {
            StartCoroutine(wheel.JumpRelease());
        }
    }

    //
    // IDamageable
    //

    // Instantly kill the car
    public void Kill(GameObject source)
    {
        Damage(maxHealth, source);
    }

    public void Damage(int dmg, GameObject source)
    {
        EventManager.TookDamage?.Invoke(dmg, gameObject, source);
        health -= dmg;

        //Debug.Log("Took " + dmg + " damage from " + source.ToString() + ", now at " + health);

        if (health <= 0)
        {
            health = 0;
            Died(source);
        }
    }

    // Triggered when it actually dies
    private void Died(GameObject source)
    {
        Debug.Log("Ouchi, I die");
        EventManager.Died?.Invoke(gameObject, source);
        Destroy(gameObject);
        foreach(Wheel wheel in wheels)
        {
            Destroy(wheel.wheelTransform);
        }
    }
}

// Wheel class for storing information and doing physics calculations with
class Wheel
{
    // References
    public GameObject wheelTransform;
    public GameObject prefab;
    public Rigidbody rbCar;

    // Is one of the front wheels?
    public bool front;

    // Enables debug gizmos
    public bool debug = false;

    // Stores data on the physics raycast query
    private RaycastHit rayHit;

    // Actual grip of the wheel
    private float wheelGrip = 0.0f;
    
    //
    // Wheel parameters
    //
    // Grip and acceleration
    private readonly float maxTraction = 240.0f;
    private readonly float frictionCoefficient = 1.5f;
    private readonly float frontGrip = 80.0f;
    private readonly float rearGrip = 40.0f;
    // Steering
    private readonly float maxSteerDegrees = 15.0f;
    // Suspension
    private readonly float maxSuspensionLength = 2.5f;
    private readonly float suspensionStrength = 230.0f;
    private readonly float suspensionDampening = 10.0f;
    private readonly float suspensionRestLength = 1.8f;
    private float suspensionLengthAddition = 0.0f;

    // Does the wheel touch the ground
    public bool isOnGround;

    // Spin of the wheel
    public float rpm = 0.0f;
    // Steer of the wheel
    private float currentSteer = 0.0f;
    // Is the car in a drift
    private bool isDrifting = false;

    //
    // VFX
    //
    public VisualEffect wheelRubbleVFX;
    public VisualEffect driftLinesVFX;

    private void SetIsOnGround(bool _isOnGround)
    {
        isOnGround = _isOnGround;
        driftLinesVFX.SetBool("ShouldRender", isOnGround && isDrifting);
    }

    private void SetIsDrifting(bool _isDrifting)
    {
        isDrifting = _isDrifting;
        driftLinesVFX.SetBool("ShouldRender", isOnGround && isDrifting);
    }

    // Called by Car, assigns some things at the very start
    public void PostInitialize()
    {
        // Set the actual grip
        wheelGrip = front ? frontGrip : rearGrip;
        // Rotate the back wheels correctly
        if (!front) prefab.transform.Rotate(new Vector3(0.0f, -90.0f, 0.0f));
    }

    // Called by Car every physics frame
    public void UpdatePhysics()
    {
        // Update the grounded state and hit query
        ShootRay();

        // Rotate the transform according to the steer
        wheelTransform.transform.Rotate(new Vector3(0.0f, currentSteer, 0.0f));

        // If grounded, do all the independent physics calculations
        // (The ones that aren't acceleration)
        if (isOnGround)
        {
            ApplyWheelForce(CalculateSuspension());
            ApplyWheelForce(CalculateFriction());
            ApplyWheelForce(CalculateAntiSlip());
            DoVFX();
        }
        else
        {
            wheelRubbleVFX.SendEvent("OnStop");
        }

        if (rayHit.collider != null)
        {
            // Move the mesh visual to the point on the ground that the ray hit
            prefab.transform.position = rayHit.point;
        }
        else
        {
            // Move the mesh visual to the max range of the suspension
            prefab.transform.position = Vector3.Lerp(prefab.transform.position, (wheelTransform.transform.position - rbCar.transform.up * 0.3f) - rbCar.transform.up * (maxSuspensionLength), 0.2f);
        }

        // Spin the mesh visual
        prefab.transform.GetChild(0).Rotate(0, rpm, 0, Space.Self);
    }

    // Accelerate the car from the position of the wheel
    public void ApplyGas(float gasStrength)
    {
        // Front wheel drive, also only if grounded
        if (front && isOnGround)
        {
            // Calculate the acceleration as a measure of the 
            Vector3 accelerationForce = Vector3.ClampMagnitude(gasStrength * wheelTransform.transform.right, maxTraction);

            ApplyWheelForce(accelerationForce);
        }
    }

    // Steer the car
    public void Steer(float ratio)
    {
        // Front wheel steering
        if (front)
        {
            float degrees = ratio * maxSteerDegrees;
            currentSteer = degrees;
            prefab.transform.localEulerAngles = new Vector3(prefab.transform.localEulerAngles.x, degrees, prefab.transform.localEulerAngles.z);
        }
    }

    public void JumpHold()
    {
        suspensionLengthAddition = -0.2f;
    }

    public IEnumerator JumpRelease()
    {
        rbCar.angularDamping = 2.0f;
        suspensionLengthAddition = 1.0f;
        yield return new WaitForSeconds(0.2f);
        rbCar.angularDamping = 0.2f;
        suspensionLengthAddition = 0.0f;
    }

    // Suspension ray check
    public void ShootRay()
    {
        // Draw gizmos
        if (debug) {
            Debug.DrawRay(wheelTransform.transform.position - rbCar.transform.up * 0.3f, -rbCar.transform.up, Color.green);
            Debug.DrawRay(rayHit.point, rayHit.normal, Color.purple);
        }
        // Do the physics query
        bool didHit = Physics.Raycast(wheelTransform.transform.position - rbCar.transform.up * 0.3f, -rbCar.transform.up, out rayHit, maxSuspensionLength);
        SetIsOnGround(didHit);
    }

    // Apply a force to the car from the wheel position
    private void ApplyWheelForce(Vector3 force)
    {
        rbCar.AddForceAtPosition(force, wheelTransform.transform.position);
    }

    // Wheel spring
    private Vector3 CalculateSuspension()
    {
        // Get the unit vector that the suspension acts in
        Vector3 springAxis = wheelTransform.transform.up;
        // Get the current velocity of the car at the point of the wheel
        Vector3 wheelWorldVelocity = rbCar.GetPointVelocity(wheelTransform.transform.position);
        // Get how far the suspension is from its rest length
        float offset = suspensionRestLength + suspensionLengthAddition - rayHit.distance;

        // Get the dot product of the axis and the velocity
        float vel = Vector3.Dot(springAxis, wheelWorldVelocity);
        // Calculate the suspension force
        float force = (offset * suspensionStrength) - (vel * suspensionDampening);
        // Force in the suspension direction
        Vector3 suspension = wheelTransform.transform.up * force;

        return suspension;
    }

    // Calculate how the wheel should slow the car via friction
    private Vector3 CalculateFriction()
    {
        // Get the current velocity of the car at the point of the wheel
        Vector3 vel = rbCar.GetPointVelocity(wheelTransform.transform.position);
        
        // Negate the velocity by some coefficient of the velocity
        Vector3 friction = -vel * frictionCoefficient;

        return friction;
    }

    // Reduce the perpendicular slippiness of the wheels
    private Vector3 CalculateAntiSlip()
    {
        if (rbCar.linearVelocity.sqrMagnitude <= 1.0f) return Vector3.zero;

        // Get the normalized dot product of the side axis and the velocity
        float steerVelDot = Vector3.Dot(wheelTransform.transform.forward, rbCar.GetPointVelocity(wheelTransform.transform.position).normalized);
        
        // Force to negate the sliding according to the grip
        float negationForce = -steerVelDot * wheelGrip;

        // Antislip force in the correct direction
        Vector3 antiSlip = wheelTransform.transform.forward * negationForce;

        SetIsDrifting(antiSlip.magnitude > 20.0f);

        return antiSlip;
    }

    private void DoVFX()
    {
        wheelRubbleVFX.SendEvent("OnPlay");
        wheelRubbleVFX.SetFloat("Speed", rbCar.linearVelocity.magnitude * 2.0f);

        driftLinesVFX.transform.rotation = Quaternion.identity;
        driftLinesVFX.SetVector3("CollisionNormal", rayHit.normal);
    }
}