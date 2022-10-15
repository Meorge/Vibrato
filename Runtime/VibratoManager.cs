using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Meorge.Vibrato
{
    [AddComponentMenu("")]
    public class VibratoManager : MonoBehaviour
    {
        internal static VibratoManager instance = null;

        public static void Initialize()
        {
            if (instance != null) return;
            var obj = new GameObject("Vibrato Manager");
            DontDestroyOnLoad(obj);
            var manager = obj.AddComponent<VibratoManager>();
            instance = manager;
            instance.SetUpDebug();
            
            Channels = new ReadOnlyCollection<VibrationChannel>(instance.m_Channels);
        }

        internal readonly List<VibrationProfileInstance> m_ActiveProfiles = new List<VibrationProfileInstance>();
        internal readonly List<VibrationChannel> m_Channels = new List<VibrationChannel>();

        public static ReadOnlyCollection<VibrationChannel> Channels { get; private set; } = null;

        internal bool m_notifiedAboutNoGamepad = false;

        /// <summary>
        /// 
        /// </summary>
        public static float MasterMagnitude { get; set; } = 1f;
        
        /// <summary>
        /// 
        /// </summary>
        public static float LowFrequency { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public static float HighFrequency { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static VibrationChannel GetChannelFromName(string channelName)
        {
            Initialize();
            return instance.m_Channels.Find(a => a.Name == channelName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="channel"></param>
        /// <param name="magnitude"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static VibrationProfileInstance Play([NotNull] IVibrationProfile profile, VibrationChannel channel,
            float magnitude = 1f)
        {
            Initialize();
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            var inst = new VibrationProfileInstance(profile, channel, magnitude);
            instance.m_ActiveProfiles.Add(inst);
            return inst;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="channelName"></param>
        /// <param name="magnitude"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static VibrationProfileInstance Play([NotNull] IVibrationProfile profile, string channelName, float magnitude = 1f)
        {
            Initialize();
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var index = GetChannelFromName(channelName);
            return Play(profile, index, magnitude);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="magnitude"></param>
        /// <returns></returns>
        public static VibrationChannel AddChannel(string channelName, float magnitude = 1f)
        {
            Initialize();
            var newChannel = new VibrationChannel
            {
                Name = channelName,
                Magnitude = magnitude
            };
            instance.m_Channels.Add(newChannel);
            return newChannel;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public static void RemoveChannel(VibrationChannel channel)
        {
            Initialize();
            instance.m_Channels.Remove(channel);
        }

        private void Update()
        {
            if (Gamepad.current == null)
            {
                if (!m_notifiedAboutNoGamepad)
                {
                    Debug.LogError("Gamepad.current is null, so haptics won't be sent.");
                }
                m_notifiedAboutNoGamepad = true;
                return;
            }
            m_notifiedAboutNoGamepad = false;

            LowFrequency = 0f;
            HighFrequency = 0f;
            
            List<VibrationProfileInstance> instancesToRemove = new List<VibrationProfileInstance>();
            m_ActiveProfiles.ForEach((i) =>
            {
                i.Update();
                var (newLf, newHf) = i.CurrentValues();
                
                var channelMagnitude = i.Channel.Magnitude;
                var lfToAdd = newLf * channelMagnitude * MasterMagnitude;
                var hfToAdd = newHf * channelMagnitude * MasterMagnitude;
                LowFrequency += lfToAdd;
                HighFrequency += hfToAdd;

                if (i.Completed()) instancesToRemove.Add(i);
            });
            m_ActiveProfiles.RemoveAll(i => instancesToRemove.Contains(i));

            LowFrequency = Mathf.Clamp01(LowFrequency);
            HighFrequency = Mathf.Clamp01(HighFrequency);
            
            Gamepad.current.SetMotorSpeeds(LowFrequency, HighFrequency);

            if (DebugDisplay)
            {
                m_frequencyHistory.Add((Time.time, (LowFrequency, HighFrequency)));
                while (m_frequencyHistory.Count > graphWindowRect.width)
                    m_frequencyHistory.RemoveAt(0);
            }

        }

        private string GetActiveProfileDebugInformation()
        {
            string s = "";

            foreach (var channel in m_Channels)
            {
                string profileInfo = "";
                int numProfiles = 0;
                foreach (var profile in m_ActiveProfiles)
                {
                    if (profile.Channel != channel) continue;
                    profileInfo += $"\t\"{profile.Name}\"\n";
                    profileInfo += $"\t\tmag = {profile.Magnitude}\n";
                    profileInfo += $"\t\tLF = {profile.CurrentValues().Item1}\n";
                    profileInfo += $"\t\tHF = {profile.CurrentValues().Item2}\n";
                    profileInfo += $"\t\t{profile.Time} / {profile.Duration}\n";
                    numProfiles++;
                }
                
                s += $"\"{channel.Name}\" mag = {channel.Magnitude}, active profiles = {numProfiles}\n{profileInfo}";
            }

            return s;
        }

        public static bool DebugDisplay = false;
        
        private Material graphMaterial = null;
        private List<(float, (float, float))> m_frequencyHistory = new List<(float, (float, float))>();
        private Rect graphWindowRect = new Rect(100, 100, 300, 300);

        private Rect graphRect =>
            new Rect(graphWindowRect.position, graphWindowRect.size);

        private float graphPadding = 25f;
        
        private void SetUpDebug()
        {
            graphWindowRect.x = Screen.width - graphWindowRect.width - 25;
            graphWindowRect.y = Screen.height - graphWindowRect.height - 25;
        }
        
        private void OnGUI()
        {
            if (!DebugDisplay) return;
            
            if (!graphMaterial) graphMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            
            graphWindowRect = GUI.Window(0, graphWindowRect, DrawGraph, "Vibration Amplitudes");
        }

        private void DrawGraph(int winID)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (m_frequencyHistory.Count < 2) return;
            
            GL.PushMatrix();
            GL.Clear(true, false, Color.black);
            graphMaterial.SetPass(0);
            
            var leftT = m_frequencyHistory[0].Item1;
            var rightT = m_frequencyHistory[^1].Item1;

            List<(Vector2, Vector2)> mappedPoints = m_frequencyHistory.ConvertAll((input) =>
            {
                var remappedT = Remap(
                    leftT, rightT,
                    graphPadding, graphWindowRect.width - graphPadding,
                    input.Item1);
                
                var remappedLF = Remap(
                    0, 1,
                    graphWindowRect.height - graphPadding, graphPadding,
                    input.Item2.Item1);
                
                var remappedHF = Remap(
                    0, 1,
                    graphWindowRect.height - graphPadding, graphPadding,
                    input.Item2.Item2);
                return (new Vector2(remappedT, remappedLF), new Vector2(remappedT, remappedHF));
            });

            // Draw low frequency
            GL.Begin(GL.QUADS);
            GL.Color(Color.red);
            for (var i = 1; i < mappedPoints.Count; i++)
            {
                var previousPoint = mappedPoints[i - 1].Item1;
                var thisPoint = mappedPoints[i].Item1;
                DrawThickLine(previousPoint, thisPoint, 2);
            }
            GL.End();
            
            // Draw high frequency
            GL.Begin(GL.QUADS);
            GL.Color(Color.green);
            for (var i = 1; i < mappedPoints.Count; i++)
            {
                var previousPoint = mappedPoints[i - 1].Item2;
                var thisPoint = mappedPoints[i].Item2;
                DrawThickLine(previousPoint, thisPoint, 2);
            }
            GL.End();
            
            GL.PopMatrix();
        }

        private void DrawThickLine(Vector2 a, Vector2 b, float thickness)
        {
            var bToA = b - a;
            var perp = new Vector2(bToA.y, -bToA.x).normalized;

            var aPoint1 = a + thickness / 2.0f * perp;
            var aPoint2 = a - thickness / 2.0f * perp;

            var bPoint1 = b + thickness / 2.0f * perp;
            var bPoint2 = b - thickness / 2.0f * perp;
            
            GL.Vertex3(aPoint1.x, aPoint1.y, 0);
            GL.Vertex3(bPoint1.x, bPoint1.y, 0);
            GL.Vertex3(bPoint2.x, bPoint2.y, 0);
            GL.Vertex3(aPoint2.x, aPoint2.y, 0);
        }

        private float Remap(float a, float b, float c, float d, float x)
        {
            return (x - a) / (b - a) * (d - c) + c;
        }

        private void OnDisable()
        {
            if (Gamepad.current == null)
                return;
            
            Gamepad.current.ResetHaptics();
        }
    }
}
