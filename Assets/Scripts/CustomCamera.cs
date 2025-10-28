using System.Collections;
using UnityEngine;

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
        // Set the music source to this camera's audio source
        AudioManager.Instance.SetMusicSource(GetComponent<AudioSource>());

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
        if (player == null)
        {
            transform.SetPositionAndRotation(savedPos, savedRot);
            return;
        }

        AdjustTargetsToFallSpeed();

        MoveCamera();

        LockZRot();

        if (shakeTimeElapsed < shakeDuration) DoCameraShake();

        // If the car is very slow, default to forward
        // Use squared speed to compare because it's cheaper
        float sqrSpeed = player.rb.linearVelocity.sqrMagnitude;
        if (sqrSpeed < 64.0f) // speed < 8.0f
        {
            flipped = false;
            cameraTarget = forwardTarget;
            return;
        }

        float upsideDownness = Vector3.Dot(cameraTarget.transform.up, transformTracker.transform.up);

        // If the car is flipped upside-down, don't do anything
        if (upsideDownness < lockingLeniency)
        {
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

    // Lock the Z rotation of the camera
    private void LockZRot()
    {
        // How well the camera's forward vector maps to down
        float downPointedness = Vector3.Dot(cameraTarget.transform.forward, -transformTracker.transform.up);

        // Don't lock rolling if the car is pointing almost straight down (as it stutters)
        if (downPointedness < lockingLeniency)
        {
            // Set z rotation to 0 (so there is no rolling)
            Vector3 rot = transform.eulerAngles;
            rot.z = 0.0f;
            transform.eulerAngles = rot;
        }
    }

    // Any car took damage
    private void CarTookDamage(int dmg, GameObject target, GameObject source)
    {
        // The car the camera is attached to took damage
        if (target == player.gameObject)
        {
            AudioManager.Instance.PlayerTookDamage();

            CameraShake(0.1f, 0.5f);
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
}
