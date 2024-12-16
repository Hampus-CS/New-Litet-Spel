using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimeManager : MonoBehaviour
{
    public Light2D globalLight;
    public float dayDuration = 15f;
    private float currentTime;

    private float dayStart = 6f;
    private float dayEnd = 24f;

    public bool isTimeFrozen = false; // Flag to freeze time

    private void Start()
    {
        if (dayDuration <= 0f)
        {
            dayDuration = 15f;
        }

        currentTime = dayStart;
        Debug.Log($"Time initialized to {currentTime} hours with duration {dayDuration} seconds.");
    }


    public bool AdvanceTime()
    {
        if (isTimeFrozen) return false;

        // Add safety checks for Time.deltaTime and dayDuration
        if (dayDuration <= 0f)
        {
            //Debug.LogError("Invalid dayDuration. Cannot advance time.");
            return false;
        }

        if (Time.deltaTime <= 0f)
        {
            Debug.LogWarning("Time.deltaTime is zero. Skipping time advancement this frame.");
            return false;
        }

        float timeIncrement = (dayEnd - dayStart) / dayDuration * Time.deltaTime;
        currentTime += timeIncrement;

        Debug.Log($"Time Increment: {timeIncrement:F4}, Current Time: {currentTime:F2}");

        if (currentTime >= dayEnd)
        {
            currentTime = dayEnd;
            return true;
        }

        return false;
    }


    public void UpdateLighting()
    {
        float normalizedTime = (currentTime - dayStart) / (dayEnd - dayStart);

        globalLight.intensity = Mathf.Lerp(0.2f, 1f, Mathf.Sin(normalizedTime * Mathf.PI));

        if (normalizedTime < 0.5f)
        {
            globalLight.color = Color.Lerp(new Color(1f, 0.5f, 0.5f), Color.white, normalizedTime * 2f);
        }
        else
        {
            globalLight.color = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.5f), (normalizedTime - 0.5f) * 2f);
        }
    }

    public void ResetTime()
    {
        currentTime = dayStart;
        Debug.Log($"Time reset to {currentTime} hours.");
    }
}
