using UnityEngine;
using System.Collections.Generic;

public class SunMoon : MonoBehaviour
{
    [SerializeField] new Light light;
    [SerializeField] Material daySkybox;
    [SerializeField] Material nightSkybox;
    [SerializeField] private float dayLength;
    [SerializeField] private float rotThreshold1 = 200.0f;
    [SerializeField] private float rotThreshold2 = 220.0f;

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

        if (targetRotation.x > rotThreshold1)
        {
            if (!sunsetTriggered)
            {
                sunsetTriggered = true;
                sunsetTrigger?.Invoke();
            }
            light.intensity = Mathf.Lerp(light.intensity, 0.0f, (targetRotation.x - rotThreshold1)/(rotThreshold2 - rotThreshold1));
        }

        if (targetRotation.x > rotThreshold2)
        {
            if (isNight)
            {
                SetToDay();
                sunriseTrigger?.Invoke();
            }
            else
                SetToNight();
            targetRotation.x = (rotThreshold1 - rotThreshold2) * 2.0f;
            sunsetTriggered = false;
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
        RenderSettings.skybox = daySkybox;
    }

    void SetToNight()
    {
        isNight = true;
        light.color = timesOfDay["Night"].colour;
        light.colorTemperature = timesOfDay["Night"].kelvin;
        light.intensity = timesOfDay["Night"].intensity;
        RenderSettings.skybox = nightSkybox;
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
