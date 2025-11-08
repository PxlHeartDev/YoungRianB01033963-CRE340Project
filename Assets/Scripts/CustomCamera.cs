using UnityEngine;
using UnityEngine.Rendering;

public class CustomCamera : MonoBehaviour
{
    // References
    public Player player;
    public GameObject forwardTarget;
    public GameObject reverseTarget;

    // Parameters
    [SerializeField] private float lerpSpeed = 0.1f;
    [SerializeField] private float reverseFlipEpsilon = 0.1f;
    [SerializeField] private float lockingLeniency = 0.8f;

    // Post-proc
    public Volume globalVolume;
    public Volume damageVignetteVolume;
    

    // Reference to actual target transform
    private GameObject cameraTarget;

    private Vector3 savedPos;
    private Quaternion savedRot;

    // If the camera is currently reversed
    private bool flipped;

    // Matches only the y rotation of the cameraTarget
    private GameObject transformTracker;

    // Camera shake stuff
    private float shakeStrength;
    private float shakeDuration;
    private float shakeTimeElapsed;

    void Start()
    {
        transformTracker = new GameObject();
        cameraTarget = forwardTarget;
    }

    void OnEnable()
    {
        EventManager.TookDamage += CarTookDamage;
    }

    void OnDisable()
    {
        EventManager.TookDamage -= CarTookDamage;
    }

    void FixedUpdate()
    {
        // If the player has died, set the camera to the last remembered transform
        // This fixes the edge-case of the camera being placed wrong if the player dies mid-shake
        if (player == null)
        {
            transform.SetPositionAndRotation(savedPos, savedRot);
            return;
        }

        //AdjustTargetsToFallSpeed();

        MoveCamera();

        //LockRot();

        if (shakeTimeElapsed < shakeDuration) DoCameraShake();

        DoVignetteFade();

        // If the car is very slow, default to forward
        // Use squared speed to compare because it's cheaper
        float sqrSpeed = player.rb.linearVelocity.sqrMagnitude;
        if (sqrSpeed < 64.0f) // speed < 8.0f
        {
            flipped = false;
            cameraTarget = forwardTarget;
            return;
        }

        //float upsideDownness = Vector3.Dot(cameraTarget.transform.up, transformTracker.transform.up);

        //// If the car is flipped upside-down, don't do anything
        //if (upsideDownness < lockingLeniency)
        //{
        //    return;
        //}

        // If falling, don't do anything
        if (player.groundedWheels == 0)
        {
            flipped = false;
            cameraTarget = forwardTarget;
            return;
        }

        // Get the dot product of the current velocity of the car and the camera direction
        Vector3 movement = player.rb.linearVelocity;
        Vector3 cameraDir = transformTracker.transform.forward;
        float dot = Vector3.Dot(movement, cameraDir);

        // Switch the camera if needed
        if (dot < -reverseFlipEpsilon)
        {
            flipped = !flipped;
            cameraTarget = flipped ? reverseTarget : forwardTarget;
        }

        savedPos = cameraTarget.transform.position;
        savedRot = cameraTarget.transform.rotation;
    }

    // Rotate the targets slightly down towards the car to account for fall speed
    private void AdjustTargetsToFallSpeed()
    {
        float fallSpeed = player.rb.linearVelocity.y;

        Vector3 fRot = forwardTarget.transform.localEulerAngles;
        fRot.y = -113.0f - Mathf.Lerp(0.0f, 20.0f, fallSpeed / 500.0f);
        forwardTarget.transform.localEulerAngles = fRot;

        Vector3 rRot = reverseTarget.transform.localEulerAngles;
        rRot.y = -246.0f + Mathf.Lerp(0.0f, 20.0f, fallSpeed / 500.0f);
        reverseTarget.transform.localEulerAngles = rRot;
    }

    // Actually move the camera towards the target
    private void MoveCamera()
    {
        // Set the tracker
        transformTracker.transform.eulerAngles = new Vector3(0.0f, cameraTarget.transform.eulerAngles.y, 0.0f);

        // Move the camera towards the target
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, cameraTarget.transform.position, lerpSpeed), Quaternion.Lerp(transform.rotation, cameraTarget.transform.rotation, lerpSpeed));
    }

    // Lock the X and Z rotation of the camera
    private void LockRot()
    {
        // How well the camera's forward vector maps to down
        float downPointedness = Vector3.Dot(cameraTarget.transform.forward, -transformTracker.transform.up);

        Vector3 rot = transform.eulerAngles;

        // Don't lock rolling if the car is pointing almost straight down (as it stutters)
        if (downPointedness < lockingLeniency) rot.z = 0.0f; // Set z rotation to 0 (so there is no rolling)

        if (rot.x >= 60.0f && rot.x <= 70.0f) rot.x = 60.0f;
        if (rot.x > 70.0f && rot.x <= 80.0f) rot.x = 80.0f;

        if (rot.x >= 270.0f && rot.x <= 280.0f) rot.x = 270.0f;
        if (rot.x > 280.0f && rot.x <= 290.0f) rot.x = 290.0f;

        transform.eulerAngles = rot;

    }

    #region Events and Effects

    // Any car took damage
    private void CarTookDamage(int dmg, GameObject target, GameObject source)
    {
        // The car the camera is attached to took damage
        if (target == player.gameObject)
        {
            AudioManager.Instance.PlayerTookDamage();

            CameraShake(0.1f, 0.5f);

            float maxHP = target.GetComponent<Car>().maxHealth;

            damageVignetteVolume.weight = Mathf.Clamp01(2.0f * dmg/maxHP);
            globalVolume.weight = 0.0f;
        }
        else
        {
            float distanceToDamagedCar = Vector3.Distance(target.transform.position, source.transform.position);
            if (distanceToDamagedCar < 100.0f)
            {

                CameraShake(0.05f, 0.3f);
            }
        }
    }

    private void CameraShake(float strength, float duration)
    {
        shakeTimeElapsed = 0.0f;
        shakeStrength = strength;
        shakeDuration = duration;
    }

    private void DoCameraShake()
    {
        float x = Random.Range(-1.0f, 1.0f) * shakeStrength;
        float y = Random.Range(-1.0f, 1.0f) * shakeStrength;
        float z = Random.Range(-1.0f, 1.0f) * shakeStrength;

        transform.position += new Vector3(x, y, z);

        shakeTimeElapsed += Time.deltaTime;
    }

    private void DoVignetteFade()
    {
        damageVignetteVolume.weight = Mathf.Lerp(damageVignetteVolume.weight, 0.0f, 0.01f);
        globalVolume.weight = Mathf.Lerp(globalVolume.weight, 1.0f, 0.01f);
    }

    #endregion
}
