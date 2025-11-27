using UnityEngine;
using System.Collections.Generic;

public class SunMoon : MonoBehaviour
{
    [SerializeField] new Light light;
    [SerializeField] private float dayLength;
    [SerializeField] private float rotThreshold1 = 180.0f;
    [SerializeField] private float rotThreshold2 = 360.0f;

    private Vector3 startingRotation;
    private Vector3 targetRotation;

    public System.Action sunsetTrigger;
    public System.Action sunriseTrigger;

    bool sunsetTriggered = false;

    private bool isNight;

    private Dictionary<string, TimeOfDay> timesOfDay = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetRots();

        timesOfDay.Add("Day", new TimeOfDay(
            "Day",
            Color.white,
            5000,
            2.0f));
        timesOfDay.Add("Night", new TimeOfDay(
            "Night",
            new Color(0.588f, 0.588f, 1.0f),
            20000,
            0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Lerp(Quaternion.Euler(startingRotation), Quaternion.Euler(targetRotation), Time.time);

        targetRotation.x += (360.0f/dayLength) * Time.deltaTime;

        if (targetRotation.x > rotThreshold1 && !sunsetTriggered)
        {
            sunsetTriggered = true;
            SetToNight();
            sunsetTrigger?.Invoke();
        }

        if (targetRotation.x > rotThreshold2 && isNight)
        {
            sunsetTriggered = false;
            SetToDay();
            sunriseTrigger?.Invoke();

            targetRotation.x = 0.0f;
        }
    }

    void SetRots()
    {
        startingRotation = transform.eulerAngles;
        targetRotation = startingRotation;
    }

    void SetToDay()
    {
        isNight = false;
        light.color = timesOfDay["Day"].colour;
        light.colorTemperature = timesOfDay["Day"].kelvin;
        light.intensity = timesOfDay["Day"].intensity;
    }

    void SetToNight()
    {
        isNight = true;
        light.color = timesOfDay["Night"].colour;
        light.colorTemperature = timesOfDay["Night"].kelvin;
        light.intensity = timesOfDay["Night"].intensity;
    }
}

public class TimeOfDay
{
    public string name;
    public Color colour;
    public int kelvin;
    public float intensity;

    public TimeOfDay(string _name, Color _colour, int _kelvin, float _intensity)
    {
        name = _name;
        colour = _colour;
        kelvin = _kelvin;
        intensity = _intensity;
    }
}
