using Unity.VisualScripting;
using UnityEngine;

public class CustomCamera : MonoBehaviour
{
    public Player player;
    public GameObject forwardTarget;
    public GameObject reverseTarget;

    private GameObject cameraTarget;

    [SerializeField] private float lerpSpeed = 0.1f;
    [SerializeField] private float reverseFlipEpsilon = 0.1f;
    [SerializeField] private float lockingLeniency = 0.35f;

    private bool flipped;

    // Matches only the y rotation of the cameraTarget
    private GameObject transformTracker;



    void Start()
    {
        // Connect the damage event
        EventManager.TookDamage += CarTookDamage;
        transformTracker = new GameObject();
        cameraTarget = forwardTarget;
    }

    void FixedUpdate()
    {
        // Set the tracker
        transformTracker.transform.eulerAngles = new Vector3(0.0f, cameraTarget.transform.eulerAngles.y, 0.0f);

        // Move the camera towards the target
        transform.SetPositionAndRotation(Vector3.Lerp(transform.position, cameraTarget.transform.position, lerpSpeed), Quaternion.Lerp(transform.rotation, cameraTarget.transform.rotation, lerpSpeed));

        float downPointedness = Vector3.Dot(cameraTarget.transform.forward, -transformTracker.transform.up);

        // If the car front is not pointing straight down or up
        if (downPointedness < -lockingLeniency || downPointedness > lockingLeniency)
        {
            // Set z rotation to 0 (so there is no rolling)
            Vector3 rot = transform.eulerAngles;
            rot.z = 0.0f;
            transform.eulerAngles = rot;
        }

        // If the car is very slow, default to forward.
        float sqrSpeed = player.rb.linearVelocity.sqrMagnitude;
        if (sqrSpeed < 64.0f) // speed < 8.0f
        {
            flipped = false;
            cameraTarget = flipped ? reverseTarget : forwardTarget;
            return;
        }

        float upsideDownness = Vector3.Dot(cameraTarget.transform.up, transformTracker.transform.up);

        // If the car is flipped upside-down, don't do anything
        if (upsideDownness < lockingLeniency && (downPointedness < -lockingLeniency || downPointedness > lockingLeniency))
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
    }

    // The car the camera is attached to took damage
    private void CarTookDamage(int dmg, MonoBehaviour target, MonoBehaviour source)
    {
        if(target == player)
        {
            // Camera effects
        }
    }
}
