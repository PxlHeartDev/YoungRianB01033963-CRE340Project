using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Trailer : MonoBehaviour
{
    public Volume volume;
    Bloom bloom;

    float bloomIntensity = 0.25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volume.profile.TryGet<Bloom>(out bloom);
    }

    // Update is called once per frame
    void Update()
    {
        if (bloom.intensity.value != bloomIntensity)
            bloom.intensity.value = bloomIntensity;
    }
}
