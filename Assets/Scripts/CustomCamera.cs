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

    //
    private float rememberedZ = 0.0f;

    void Start()
    {
        transformTracker = new GameObject();
        cameraTarget = forwardTarget;
        ResetEffects();
    }

    void OnEnable()
    {
        EventManager.TookDamage += CarTookDamage;
        EventManager.Died += CarDied;
    }

    void OnDisable()
    {
        EventManager.TookDamage -= CarTookDamage;
        EventManager.Died -= CarDied;
    }

    void FixedUpdate()
    {
        // If the player has died, set the camera to the last remembered transform and skip the rest of the logic
        // This fixes the edge-case of the camera being placed wrong if the player dies mid-shake
        if (player == null || !player.active)
        {
            MoveCamera(true);
            return;
        }

        TransformGizmos.DrawTransformGizmo(forwardTarget.transform);
        TransformGizmos.DrawTransformGizmo(reverseTarget.transform);

        MoveCamera(false);

        DoVignetteFade();

        // If airbourne,
        if (player.groundedWheels == 0)
        {
            // Don't do flipping logic, keep it as is

            // Check the player isn't falling with high speed
            if (player.rb.linearVelocity.y > -30)
            {
                // Lock cam
                LockRot();
            }
        }
        else
        {
            // Switch the camera if needed
            flipped = player.IsReversing() && Vector3.Dot(player.rb.linearVelocity.normalized, player.rb.transform.right) < -0.1f;
            cameraTarget = flipped ? reverseTarget : forwardTarget;

            // Update tracker
            rememberedZ = transform.eulerAngles.z;
        }

        savedPos = cameraTarget.transform.position;
        savedRot = cameraTarget.transform.rotation;
    }

    // Actually move the camera towards the target
    private void MoveCamera(bool toSaved)
    {
        if (toSaved)
        {
            // Set the tracker
            transformTracker.transform.eulerAngles = new Vector3(0.0f, savedRot.eulerAngles.y, 0.0f);

            // Move the camera towards the target
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, savedPos, lerpSpeed), Quaternion.Lerp(transform.rotation, savedRot, lerpSpeed));
        }
        else
        {
            // Set the tracker
            transformTracker.transform.eulerAngles = new Vector3(0.0f, cameraTarget.transform.eulerAngles.y, 0.0f);

            // Move the camera towards the target
            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, cameraTarget.transform.position, lerpSpeed), Quaternion.Lerp(transform.rotation, cameraTarget.transform.rotation, lerpSpeed));
        }
    }

    // Lock the Z rotation of the camera
    private void LockRot()
    {
        Vector3 rot = transform.eulerAngles;
        rot.z = rememberedZ; // Set z rotation to the remembered value (so there is no rolling)

        transform.rotation = Quaternion.Euler(rot.x, rot.y, rot.z);
    }

    #region Events

    // Any car took damage
    private void CarTookDamage(int dmg, GameObject target, GameObject source)
    {
        // The car the camera is attached to took damage
        if (target == player.gameObject)
        {
            AudioManager.Instance.PlayerTookDamage();

            CameraShake(0.1f, 0.5f);

            float maxHP = target.GetComponent<Car>().maxHealth;

            damageVignetteVolume.weight = Mathf.Clamp01(5.0f * dmg/maxHP);
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

    private void CarDied(GameObject target, GameObject source)
    {
        if (target == player.gameObject)
        {
            CameraShake(1.0f, 1.0f);
            damageVignetteVolume.weight = 0.8f;
            globalVolume.weight = 0.2f;
        }
    }
    #endregion

    #region Effects

    private void ResetEffects()
    {
        ResetDamageVignette();
    }

    private void ResetDamageVignette()
    {
        damageVignetteVolume.weight = 0.0f;
        globalVolume.weight = 1.0f;
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
