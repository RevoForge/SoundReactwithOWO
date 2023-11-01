using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RevoAudioVisualizer))]
public class RevoAudioVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();  // Draw the default inspector

        RevoAudioVisualizer myScript = (RevoAudioVisualizer)target;
        if (GUILayout.Button("Start Capture"))
        {
            myScript.StartCapture();
        }

        if (GUILayout.Button("Stop Capture"))
        {
            myScript.StopCapture();
        }
    }
}
