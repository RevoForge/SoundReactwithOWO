using OWOGame;
using TMPro;
using UnityEngine;

public class AudioMagnitudeReader : MonoBehaviour
{
    [SerializeField] private RevoAudioVisualizer audioVisualizer;
    [SerializeField] private OWOAudioInteraction audioInteraction;
    [Header("Sound Channel Options")]
    [SerializeField] private bool watchSubBass1 = true;
    [SerializeField] private bool watchSubBass2 = true;
    [SerializeField] private bool watchBass1 = true;
    [SerializeField] private bool watchBass2 = true;
    [SerializeField] private bool watchLowMid1 = true;
    [SerializeField] private bool watchLowMid2 = true;
    [SerializeField] private bool watchMid1 = true;
    [SerializeField] private bool watchMid2 = true;
    [SerializeField] private bool watchUpperMid1 = true;
    [SerializeField] private bool watchUpperMid2 = true;
    [SerializeField] private bool watchPresence1 = true;
    [SerializeField] private bool watchPresence2 = true;
    [SerializeField] private bool watchBrilliance1 = true;
    [SerializeField] private bool watchBrilliance2 = true;
    [SerializeField] private bool watchHighTreble1 = true;
    [SerializeField] private bool watchHighTreble2 = true;
    [Header("Debug Log Option")]
    [SerializeField] private bool sendDebugMSG = true;
    private float[] minimumThreshold = new float[16] {5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5 };
    public TMP_InputField[] thresholdInput = new TMP_InputField[16];
    private int currentThreshold;
    private float timer;
    private bool startSuit = false;
    [Header("Output Timer")]
    [SerializeField, Tooltip("Set to How Often you Want to See The Values"), Range(0.02f, 1)] private float timedelay = 0.05f;
    private void LateUpdate()
    {
        if (audioVisualizer != null && startSuit)
        {
            timer += Time.deltaTime;
            if (timer > timedelay)
            {
                timer = 0f;

                float highestAmp = float.MinValue;
                int highestIndex = -1;

                for (int i = 0; i < 16; i++)
                {
                    bool shouldCheck;
                    switch (i)
                    {
                        case 0: shouldCheck = watchSubBass1; break;
                        case 1: shouldCheck = watchSubBass2; break;
                        case 2: shouldCheck = watchBass1; break;
                        case 3: shouldCheck = watchBass2; break;
                        case 4: shouldCheck = watchLowMid1; break;
                        case 5: shouldCheck = watchLowMid2; break;
                        case 6: shouldCheck = watchMid1; break;
                        case 7: shouldCheck = watchMid2; break;
                        case 8: shouldCheck = watchUpperMid1; break;
                        case 9: shouldCheck = watchUpperMid2; break;
                        case 10: shouldCheck = watchPresence1; break;
                        case 11: shouldCheck = watchPresence2; break;
                        case 12: shouldCheck = watchBrilliance1; break;
                        case 13: shouldCheck = watchBrilliance2; break;
                        case 14: shouldCheck = watchHighTreble1; break;
                        case 15: shouldCheck = watchHighTreble2; break;
                        default: shouldCheck = false; break;
                    }

                    if (shouldCheck)
                    {
                        float amp = audioVisualizer.peakMagnitudes[i];
                        if (amp > 100) { amp = 100; }
                        if (amp > minimumThreshold[i] && amp >= highestAmp)
                        {
                            highestAmp = amp;
                            highestIndex = i;
                        }
                        if (sendDebugMSG)
                        {
                            Debug.Log($"The amplitude of index {i} is {amp}");
                        }
                    }
                }

                if (highestIndex != -1 && highestAmp > 0)
                {
                    audioInteraction.SensationByIntensity(highestIndex, highestAmp);
                }
            }
        }
    }
    public void StartSuitLink()
    {
        startSuit = true;
        audioVisualizer.StartCapture();
    }
    public void StopSuitLink()
    {
        startSuit = false;
        OWO.Stop();
    }
    
    public void BandToggle(int bandNum)
    {
        switch (bandNum)
        {
            case 0: watchSubBass1 = !watchSubBass1; break;
            case 1: watchSubBass2 = !watchSubBass2; break;
            case 2: watchBass1 = !watchBass1; break;
            case 3: watchBass2 = !watchBass2; break;
            case 4: watchLowMid1 = !watchLowMid1; break;
            case 5: watchLowMid2 = !watchLowMid2; break;
            case 6: watchMid1 = !watchMid1; break;
            case 7: watchMid2 = !watchMid2; break;
            case 8: watchUpperMid1 = !watchUpperMid1; break;
            case 9: watchUpperMid2 = !watchUpperMid2; break;
            case 10: watchPresence1 = !watchPresence1; break;
            case 11: watchPresence2 = !watchPresence2; break;
            case 12: watchBrilliance1 = !watchBrilliance1; break;
            case 13: watchBrilliance2 = !watchBrilliance2; break;
            case 14: watchHighTreble1 = !watchHighTreble1; break;
            case 15: watchHighTreble2 = !watchHighTreble2; break;
        }
    }
    public void ThresholdBandChange(int input)
    {
        currentThreshold = input;
        ThresholdStringChange();
    }
    public void ThresholdStringChange()
    {
        if (int.TryParse(thresholdInput[currentThreshold].text, out int number))
        {
            // Parsing was successful, and 'number' now contains the integer value.
        }
        minimumThreshold[currentThreshold] = number;
    }
    public void OnInputFieldValueChanged(string newValue, int inputFieldIndex)
    {
        // Handle the new value and identify the input field
        Debug.Log($"Input Field {inputFieldIndex} Value: {newValue}");
        // You can pass this value to your other script or perform any other actions.
    }
}
