using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Meorge.Vibrato.Editor
{
    public class VibratoMixer : EditorWindow
    {

        [MenuItem("Vibrato/Vibrato Mixer")]
        public static void ShowWindow()
        {
            var t = new GUIContent("Vibration Mixer");
            var window = EditorWindow.GetWindow(typeof(VibratoMixer));
            window.titleContent = t;
        }

        private void OnGUI()
        {
            
        }
    }
}