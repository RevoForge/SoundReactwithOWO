
using UnityEngine;
using OWOGame;
using System.Threading;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Sensation
{
    [HideInInspector]public string sensationName;

    [Tooltip("Frequency for the Sensation event.")]
    [Range(1, 100)]
    public int frequency = 100;

    [Tooltip("Duration of the Sensation event.")]
    [Range(0.1f, 20)]
    public float duration = 0.2f;

    [Tooltip("Ramp up time. Only 0.1 Increments affect the Vest.")]
    [Range(0, 2)]
    public float rampUp = 0f;

    [Tooltip("Ramp down time. Only 0.1 Increments affect the Vest.")]
    [Range(0, 2)]
    public float rampDown = 0f;
}
[ExecuteInEditMode]
public class OWOAudioInteraction : MonoBehaviour 
{
    public Sensation[] sensations = new Sensation[16];
    public TMP_InputField[] inputFields = new TMP_InputField[16];
    public Slider intensitySlider;
    private void OnValidate()
    {
        EnsureSensationsArray();
    }
    private void EnsureSensationsArray()
    {
        string[] defaultNames =
        {
            "Sub Bass 1", "Sub Bass 2", "Bass 1", "Bass 2",
            "Low Mid 1", "Low Mid 2", "Mid 1", "Mid 2",
            "Upper Mid 1", "Upper Mid 2", "Presence 1", "Presence 2",
            "Brilliance 1", "Brilliance 2", "High Treble 1", "High Treble 2"
        };

        if (sensations.Length != 16)
            sensations = new Sensation[16];

        for (int i = 0; i < sensations.Length; i++)
        {
            if (sensations[i] == null)
                sensations[i] = new Sensation();

            if (string.IsNullOrEmpty(sensations[i].sensationName))
                sensations[i].sensationName = defaultNames[i];
        }
    }
    private void Start()
    {
        // InitializeOWO(); // For Testing
        for (int i = 0; i < sensations.Length; i++)
        {
            inputFields[i].placeholder.GetComponent<TextMeshProUGUI>().text = sensations[i].frequency.ToString();
        }
    }
    public void InitializeOWO()
    {
        OWO.AutoConnect();
        StartCoroutine(CheckConnection());
    }

    private IEnumerator CheckConnection()
    {
        while (OWO.ConnectionState != ConnectionState.Connected)
        {
            yield return new WaitForSeconds(0.1f);  // Wait for 0.1 seconds before checking again
        }

        StartCoroutine(StartupPulse());
    }

    private IEnumerator StartupPulse()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Startup Pulse");
        float elapsedTime = 0f;
        const float timeInterval = 1f;
        var startup = SensationsFactory.Create(100, 1, 25, 0.5f, 0.5f, 0);
        while (elapsedTime < 3f)
        {
            OWO.Send(startup.WithMuscles(Muscle.All));
            yield return new WaitForSeconds(timeInterval);
            elapsedTime += timeInterval;
        }
    }
    public void SensationByIntensity(int sensationIndex, float hit)
    {
        if (sensationIndex < 0 || sensationIndex >= sensations.Length)
        {
            Debug.LogError("Invalid sensation index");
            return;
        }
        float adjustedHit = hit * intensitySlider.value;
        Sensation sensation = sensations[sensationIndex];
        int sensationHit = (int)(adjustedHit);
        //Debug.Log($"intensity: {sensationHit}");
        var audiohit = SensationsFactory.Create(sensation.frequency, sensation.duration, sensationHit, sensation.rampUp, sensation.rampDown,0).WithMuscles(Muscle.All);
        OWO.Send(audiohit);
    }
}
