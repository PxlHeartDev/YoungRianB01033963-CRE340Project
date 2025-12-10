using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using UnityEngine.InputSystem;

[ExecuteAlways]
public class Trailer : MonoBehaviour
{
    public InputActionAsset actions;
    private InputAction m_playAction;

    public Animator animator;

    public Volume volume;
    Bloom bloom;

    public Player player;

    public float bloomIntensity = 0.25f;

    int stage = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_playAction = InputSystem.actions.FindAction("Jump");

        actions.FindActionMap("Player").Enable();

        volume.profile.TryGet<Bloom>(out bloom);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_playAction.WasPressedThisFrame())
        {
            stage++;
            switch(stage)
            {
                case 1:
                    animator.SetTrigger("StartIntro");
                    break;
                case 2:
                    player.LockInputs(false);
                    break;
            }
        }

        if (bloom.intensity.value != bloomIntensity)
            bloom.intensity.value = bloomIntensity;
    }
}
