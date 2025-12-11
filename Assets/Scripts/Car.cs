using UnityEngine.VFX;
using UnityEngine;
using System.Collections;

public class Car : MonoBehaviour, IDamageable
{
    // Editor things

    [Header ("References")]
    [SerializeField] public Rigidbody rb;                                           // The car's rigid body
    [SerializeField] private Light[] lightList;                                     // Stores references to the lights
    private Wheel[] wheels = new Wheel[4];                                          // Stores references to the wheels
    [SerializeField] VisualEffect explosionVFX;                                     // The explosion VFX object
    [SerializeField] private GameObject body;                                       // GameObject of the body mesh
    [SerializeField] private GameObject lights;                                     // GameObject of the rally lights

    [Header ("Prefabs")]
    [SerializeField] private GameObject wheelPrefab;                                // Prefab GameObject for the wheel visual
    [SerializeField] private VisualEffect wheelRubbleVFXPrefab;                     // Prefab for the rubble VFX
    [SerializeField] private VisualEffect driftLinesVFXPrefab;                      // Prefab for the drift lines VFX

    [Header ("Parameters")]
    [SerializeField] private Vector2 wheelDistance = new Vector2(1.85f, 0.95f);     // How far away from the centre the wheels should generate
    [SerializeField] private float lightIntensity = 600.0f;                         // Intensity for the rally lights
    public bool debug = false;                                                      // Enables debug gizmos
    public float gasStrength = 40.0f;                                               // Forward acceleration strength
    public float reversePercentage = 0.75f;                                         // Coefficient for how strong reversing is relative to gasStrength

    [Header ("Health")]
    public int maxHealth { get; private set; } = 10;
    public int health { get; private set; } = 10;

    private float rpm = 0.0f;                                                       // Controls the visual wheel spin

    private int driftingWheels = 0;                                                 // Number of drifting wheels
    private bool isDrifting = false;                                                // Is the car counted as in drift
    private float driftTime = 0.0f;                                                 // Time spent in continuous drift
    private Vector3 wheelYOffset;                                                   // Y offset for the wheels

    public bool active = true;                                                      // If the car is active

    [HideInInspector] public int groundedWheels { get; private set; } = 0;          // Tracker for how many wheels are grounded

    void Awake()
    {
        wheelYOffset = transform.up * 1.3f;

        TurnOffLights();
        CreateWheels();
    }

    void FixedUpdate()
    {
        // Update the transforms every frame to keep them relative to the body's transform
        wheels[0].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * wheelDistance.y + wheelYOffset; //front right
        wheels[1].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * wheelDistance.y + wheelYOffset; //front left
        wheels[2].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * -wheelDistance.y + wheelYOffset; //back right
        wheels[3].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * -wheelDistance.y + wheelYOffset; //back left

        // Dampen rpm
        rpm -= ((rpm * Mathf.PI) + -transform.InverseTransformDirection(rb.linearVelocity).z) * Time.deltaTime * 0.1f;

        groundedWheels = 0;
        driftingWheels = 0;
        // Update every wheel
        foreach (Wheel wheel in wheels)
        {
            // Sync the transforms' rotation
            wheel.wheelTransform.transform.rotation = transform.rotation;
            // Draw debug gizmos
            if (debug) TransformGizmos.DrawTransformGizmo(wheel.wheelTransform.transform);
            // Sync spin
            wheel.rpm = rpm;

            // Call the update function on each wheel
            wheel.UpdatePhysics();
            
            // Update tracker
            if (wheel.isOnGround) groundedWheels++;
            if (wheel.isDrifting) driftingWheels++;
        }

        isDrifting = driftingWheels >= 2;
        if (isDrifting) driftTime += Time.fixedDeltaTime;
        else driftTime = 0.0f;

        foreach(Wheel wheel in wheels) wheel.steerMult = 1.0f - Mathf.Clamp(driftTime / 10.0f, 0.0f, 0.5f);
    }

    private void CreateWheels()
    {
        for (int i = 0; i < 4; i++)
        {
            // Create the objects and assign some values
            wheels[i] = new Wheel();
            wheels[i].rbCar = rb;
            wheels[i].wheelTransform = new GameObject($"Wheel {i}");
            //wheels[i].wheelTransform.transform.SetParent(transform);
            wheels[i].debug = debug;

            // Set the initial transforms
            switch (i)
            {
                case (0):
                    wheels[0].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * wheelDistance.y + wheelYOffset;// Front right
                    break;
                case (1):
                    wheels[1].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * wheelDistance.y + wheelYOffset;// Front left
                    break;
                case (2):
                    wheels[2].wheelTransform.transform.position = transform.position + transform.right * wheelDistance.x + transform.forward * -wheelDistance.y + wheelYOffset;// Back right
                    break;
                case (3):
                    wheels[3].wheelTransform.transform.position = transform.position + transform.right * -wheelDistance.x + transform.forward * -wheelDistance.y + wheelYOffset;// Back left
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

    protected void LockWheels(bool isLocked)
    {
        foreach (Wheel wheel in wheels)
            wheel.isLocked = isLocked;
    }

    #region Movement

    // Accelerate the car
    protected void Gas()
    {
        foreach (Wheel wheel in wheels)
        {
            rpm += 1.0f * Time.deltaTime;
            if (wheel.isOnGround) 
            {
                wheel.ApplyGas(gasStrength * GameManager.standardDelta);
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
            wheel.ApplyGas(-gasStrength * reversePercentage * GameManager.standardDelta);
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

    // Jump stuff
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

    #endregion

    public virtual void ItemCollected(ICollectable item)
    {

    }

    #region IDamageable

    // Instantly kill the car
    public void Kill(GameObject source)
    {
        Damage(maxHealth, source);
    }

    public void Damage(int dmg, GameObject source)
    {
        health -= dmg;
        EventManager.TookDamage?.Invoke(dmg, gameObject, source);

        //Debug.Log("Took " + dmg + " damage from " + source.ToString() + ", now at " + health);

        if (dmg > 0)
        {
            rb.linearDamping = 1.0f;
            StartCoroutine(FadeLinearDamping());
        }

        if (health <= 0)
        {
            health = 0;
            Died(source);
        }
    }

    // Triggered when it actually dies
    private void Died(GameObject source)
    {
        EventManager.Died?.Invoke(gameObject, source);

        explosionVFX.SetBool("ShouldRender", true);
        StartCoroutine(HideExplodeVFX());

        AudioManager.Instance?.PlayExplodeSFX(transform.position);

        SetActive(false);

        //Destroy(gameObject);
    }

    #endregion

    #region Other damage things
    public void SetActive(bool _active)
    {
        active = _active;

        body.SetActive(active);
        lights.SetActive(active);
        foreach (Wheel wheel in wheels)
            wheel.SetActive(active);

        if (active)
            rb.constraints = RigidbodyConstraints.None;
        else
            rb.constraints = RigidbodyConstraints.FreezeAll;

    }

    private IEnumerator HideExplodeVFX()
    {
        yield return new WaitForSeconds(0.1f);
        explosionVFX.SetBool("ShouldRender", false);
    }

    private IEnumerator FadeLinearDamping()
    {
        for (int i = 10; i >= 1; i--)
        {
            rb.linearDamping = i * 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion

    #region Lights

    public void TurnOnLights()
    {
        lightList[0].intensity = lightIntensity;
        lightList[1].intensity = lightIntensity;
    }

    public void TurnOffLights()
    {
        lightList[0].intensity = 0.0f;
        lightList[1].intensity = 0.0f;
    }

    #endregion
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

    // If the car is locked
    public bool isLocked = false;
    
    //
    // Wheel parameters
    //
    // Grip and acceleration
    private readonly float maxTraction = 240.0f;
    private readonly float frictionCoefficient = 1.0f;
    private readonly float frontGrip = 120.0f;
    private readonly float rearGrip = 60.0f;
    // Steering
    private readonly float maxSteerDegrees = 15.0f;
    public float steerMult = 1.0f;
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
    public bool isDrifting = false;

    // Is the wheel active
    private bool isActive = true;

    //
    // VFX
    //
    public VisualEffect wheelRubbleVFX;
    public VisualEffect driftLinesVFX;

    public void SetActive(bool active)
    {
        isActive = active;
        wheelTransform.SetActive(isActive);
    }

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
        // Don't do the calculations if inactive
        if (!isActive)
            return;

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
            float degrees = ratio * maxSteerDegrees * steerMult;
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
        bool didHit = Physics.Raycast(wheelTransform.transform.position - rbCar.transform.up * 0.3f, -rbCar.transform.up, out rayHit, maxSuspensionLength, 1 << 7);
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
        Vector3 friction = -vel * (isLocked ? 100.0f : frictionCoefficient);

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

        SetIsDrifting(rbCar.linearVelocity.sqrMagnitude > 100.0f && antiSlip.magnitude > 20.0f);

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