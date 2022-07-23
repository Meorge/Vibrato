using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Meorge.Vibrato.Editor
{
    [UnityEditor.CustomEditor(typeof(VibratoManager))]
    public class VibratoManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label($"Low Frequency: {VibratoManager.LowFrequency}");
            GUILayout.Label($"High Frequency: {VibratoManager.HighFrequency}");
        }
    }
}