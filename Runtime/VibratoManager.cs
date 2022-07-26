using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Meorge.Vibrato
{
    [AddComponentMenu("")]
    public class VibratoManager : MonoBehaviour
    {
        internal static VibratoManager instance = null;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogError("Instance of VibratoManager already exists - this shouldn't happen!");
                Destroy(gameObject);
            }
        }

        public static void Initialize()
        {
            if (instance != null) return;
            var obj = new GameObject("Vibrato Manager");
            obj.AddComponent<VibratoManager>();
        }

        internal readonly List<VibrationProfileInstance> m_ActiveProfiles = new List<VibrationProfileInstance>();
        internal readonly List<VibrationChannel> m_Channels = new List<VibrationChannel>();

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
        public static VibrationProfileInstance Play([NotNull] VibrationProfile profile, VibrationChannel channel,
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
        public static VibrationProfileInstance Play([NotNull] VibrationProfile profile, string channelName, float magnitude = 1f)
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
                LowFrequency += newLf * channelMagnitude * MasterMagnitude;
                HighFrequency += newHf * channelMagnitude * MasterMagnitude;
                
                if (i.Completed()) instancesToRemove.Add(i);
            });

            m_ActiveProfiles.RemoveAll(i => instancesToRemove.Contains(i));
            
            Gamepad.current.SetMotorSpeeds(LowFrequency, HighFrequency);
        }

        // private void OnGUI()
        // {
        //     GUILayout.BeginArea(new Rect(500, 500, 200, 400));
        //     m_ActiveProfiles.ForEach((a) =>
        //     {
        //         GUILayout.Label($"{a.Name} = {a.Magnitude * a.Channel.Magnitude}");
        //     });
        //     GUILayout.EndArea();
        //     
        //     GUILayout.BeginArea(new Rect(800, 500, 200, 400));
        //     GUILayout.Label("----");
        //     GUILayout.Label($"LF: {LowFrequency}");
        //     GUILayout.Label($"HF: {HighFrequency}");
        //     GUILayout.EndArea();
        // }

        private void OnDisable()
        {
            if (Gamepad.current == null)
                return;
            
            Gamepad.current.ResetHaptics();
        }
    }
}
