using UnityEngine;
using CSCore.SoundIn;
using System;
using System.Numerics;
using System.Collections.Generic;
using OWOGame;

public class RevoAudioVisualizer : MonoBehaviour
{
    private WasapiLoopbackCapture capture;
    public Transform[] eQBars; 
    private RecordingState oldState = RecordingState.Stopped;
    private List<Complex[]> pendingFrequencyData = new();
    private readonly object lockObject = new();
    [Range(1f, 5f)] public float sensitivity = 5;
    [Range(0f, 1f)] public float dampingFactor = 0.1f;  // Adjust as needed
    [HideInInspector]public float[] peakMagnitudes;
    private int minFrequency = 20; // Minimum frequency (in Hz)
    private int maxFrequency = 20000; // Maximum frequency (in Hz)
    private int numBands;
    private int[] freqBands;
    [HideInInspector] public Complex[] LatestFrequencyData { get; private set; }

    void Start()
    {
        peakMagnitudes = new float[eQBars.Length];
        InitializeAudioCapture();

        // new visualizer testing
        numBands = eQBars.Length;
        freqBands = new int[numBands];

        for (int i = 0; i < numBands; i++)
        {
            // Calculate the center frequency of each band using a logarithmic scale
            float logMin = Mathf.Log10(minFrequency);
            float logMax = Mathf.Log10(maxFrequency);
            float centerLogFrequency = logMin + (i / (float)(numBands - 1)) * (logMax - logMin);
            freqBands[i] = Mathf.RoundToInt(Mathf.Pow(10, centerLogFrequency));
        }
        // -------------------------
    }

    void InitializeAudioCapture()
    {
        capture = new WasapiLoopbackCapture();
        // Initialize the capture
        capture.Initialize();
        if (capture.Device != null)
        {
            Debug.Log("Device Name: " + capture.Device.FriendlyName);
        }
        // Subscribe to the data available event
        capture.DataAvailable += CaptureOnDataAvailable;
        capture.Stopped += (s, args) => { Debug.Log("Capture stopped."); };
    }
    public void StartCapture()
    {
        // Start capturing
        capture.Start();
        Debug.Log("Audio Capture Started");

    }
    public void StopCapture()
    {
        // Stop capturing
        capture.Stop();
        for (int i = 0; i < peakMagnitudes.Length; i++)
        {
            peakMagnitudes[i] = 0;
        }
        OWO.Stop();
        Debug.Log("Audio Capture Stopped");
    }
    [Range(0.0f, 0.5f)]
    public float updateInterval = 0.1f; // Adjust this interval as needed
    private float timeSinceLastUpdate = 0f;

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            Complex[] dataToVisualize = null;

            lock (lockObject)
            {
                if (pendingFrequencyData.Count > 0)
                {
                    dataToVisualize = pendingFrequencyData[0];
                    pendingFrequencyData.RemoveAt(0);
                }
            }

            if (dataToVisualize != null)
            {
                VisualizeFFT(dataToVisualize);
                LatestFrequencyData = dataToVisualize;
            }

            timeSinceLastUpdate = 0f; // Reset the timer
        }

        if (capture != null && oldState != capture.RecordingState)
        {
            Debug.Log("Current recording state: " + capture.RecordingState);
            oldState = capture.RecordingState;
        }
    }

    public float[] GetLatestMagnitudes()
    {
        if (LatestFrequencyData == null)
            return null;

        float[] magnitudes = new float[LatestFrequencyData.Length];
        for (int i = 0; i < LatestFrequencyData.Length; i++)
        {
            magnitudes[i] = (float)Math.Sqrt(LatestFrequencyData[i].Real * LatestFrequencyData[i].Real + LatestFrequencyData[i].Imaginary * LatestFrequencyData[i].Imaginary) * 0.01f;
        }

        return magnitudes;
    }

    private void CaptureOnDataAvailable(object sender, DataAvailableEventArgs e)
    {
        try
        {
            if (pendingFrequencyData.Count == 0)
            {
                // 24-bit audio = 3 bytes per sample
                int bytesPerSample = 3;

                // Calculate the normalization factor for 24-bit audio
                double normalizationFactor = Math.Pow(2, 23) - 1;

                // Create the doubleSamples array based on the maximum expected ByteCount
                int maxSamples = e.ByteCount / bytesPerSample;
                double[] doubleSamples = new double[maxSamples];

                for (int i = 0; i < e.ByteCount - bytesPerSample + 1 && i / bytesPerSample < doubleSamples.Length; i += bytesPerSample)
                {
                    int sample = ConvertTo24Bit(e.Data, i);
                    doubleSamples[i / bytesPerSample] = sample / normalizationFactor; // Normalize to [-1,1]
                }


                // Convert to complex (Uncomment these lines if you have implemented the FFT functions)
                Complex[] complexSamples = FastFourierTransform.doubleToComplex(doubleSamples);

                // Perform FFT
                Complex[] frequencyData = FastFourierTransform.FFT(complexSamples, false);
                    // Visualize the data
                    lock (lockObject)
                    {
                        pendingFrequencyData.Add(frequencyData);
                    }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error in CaptureOnDataAvailable: " + ex.Message);
        }
    }
    int ConvertTo24Bit(byte[] data, int offset)
    {
        int sample = (data[offset + 2] << 16) | (data[offset + 1] << 8) | data[offset];
        if ((sample & 0x800000) > 0) // if sign bit is set extend the sign bit
        {
            sample |= ~0xFFFFFF; // Set all bits outside the 24-bit range
        }
        return sample;
    }


    void OLdVisualizeFFT(Complex[] frequencyData)
    {
        int numBands = eQBars.Length;

        // Define the frequency bands (these are approximations and might need adjustments)
        int[] freqBands = { 40, 60, 155, 250, 375, 500, 750, 1000, 1500, 2000, 3000, 4000, 8000, 12000, 16000, 20000 };
        int previousEnd = 0;
        

        for (int i = 0; i < numBands; i++)
        {
            int startFreq = previousEnd;
            int endFreq = (int)((float)freqBands[i] / (48000 / 2) * (frequencyData.Length / 2)); // Assuming a sample rate of 48000
            previousEnd = endFreq;

            float peakMagnitude = CalculatePeakMagnitude(frequencyData, startFreq, endFreq);
            peakMagnitudes[i] = peakMagnitude;

            RectTransform rectTransform = eQBars[i].GetComponent<RectTransform>();
            float currentHeight = rectTransform.sizeDelta.y;
            float targetHeight = peakMagnitude * sensitivity;

            // Calculate the increment for damping
            float heightIncrement = (targetHeight - currentHeight) * dampingFactor;

            // Apply the increment to get the new height
            float newHeight = currentHeight + heightIncrement;
            rectTransform.sizeDelta = new UnityEngine.Vector2(rectTransform.sizeDelta.x, newHeight);
        }
    }
    void VisualizeFFT(Complex[] frequencyData)
    {
        int previousEnd = 0;


        for (int i = 0; i < numBands; i++)
        {
            int startFreq = previousEnd;
            int endFreq = (int)((float)freqBands[i] / (48000 / 2) * (frequencyData.Length / 2)); // Assuming a sample rate of 48000
            previousEnd = endFreq;

            float peakMagnitude = CalculatePeakMagnitude(frequencyData, startFreq, endFreq);
            peakMagnitudes[i] = peakMagnitude;

            RectTransform rectTransform = eQBars[i].GetComponent<RectTransform>();
            float currentHeight = rectTransform.sizeDelta.y;
            float targetHeight = peakMagnitude * sensitivity;

            // Calculate the increment for damping
            float heightIncrement = (targetHeight - currentHeight) * dampingFactor;

            // Apply the increment to get the new height
            float newHeight = currentHeight + heightIncrement;
            rectTransform.sizeDelta = new UnityEngine.Vector2(rectTransform.sizeDelta.x, newHeight);
        }
    }

    int FrequencyToIndex(float frequency, float sampleRate, int dataSize)
    {
        // Convert a frequency to an index in the frequencyData array
        float nyquist = sampleRate / 2.0f;
        return Mathf.RoundToInt(frequency / nyquist * dataSize);
    }

    float CalculatePeakMagnitude(Complex[] frequencyData, int start, int end)
    {
        float peak = 0;
        for (int i = start; i <= end; i++)
        {
            float magnitude = (float)Math.Sqrt(frequencyData[i].Real * frequencyData[i].Real + frequencyData[i].Imaginary * frequencyData[i].Imaginary);
            if (magnitude > peak)
                peak = magnitude;
        }
        return peak;
    }
    void OnDestroy()
    {
        if (capture != null)
        {
            capture.Stop();
            capture.Dispose();
        }
    }

}
