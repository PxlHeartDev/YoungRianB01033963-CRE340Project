using UnityEngine;
using UnityEngine.VFX;

public class CarVFX : MonoBehaviour
{
    public Car car;

    public VisualEffect smoke;

    // Update is called once per frame
    void Update()
    {
        smoke.SetFloat("Speed", car.rb.linearVelocity.magnitude + 1);
    }
}
