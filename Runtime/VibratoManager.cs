using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Meorge.Vibrato
{
    public class VibratoManager : MonoBehaviour
    {
        private readonly List<VibrationProfileInstance> m_ActiveProfiles = new List<VibrationProfileInstance>();
        private readonly List<VibrationChannel> m_Channels = new List<VibrationChannel>();

        /// <summary>
        /// 
        /// </summary>
        public float MasterMagnitude { get; set; } = 1f;
        
        /// <summary>
        /// 
        /// </summary>
        public float LowFrequency { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public float HighFrequency { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        private VibrationChannel GetChannelFromName(string channelName)
        {
            return m_Channels.Find(a => a.Name == channelName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="channel"></param>
        /// <param name="magnitude"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public VibrationProfileInstance Play([NotNull] VibrationProfile profile, VibrationChannel channel,
            float magnitude = 1f)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            var inst = new VibrationProfileInstance(profile, channel, magnitude);
            m_ActiveProfiles.Add(inst);
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
        public VibrationProfileInstance Play([NotNull] VibrationProfile profile, string channelName, float magnitude = 1f)
        {
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
        public VibrationChannel AddChannel(string channelName, float magnitude = 1f)
        {
            var newChannel = new VibrationChannel();
            m_Channels.Add(newChannel);
            return newChannel;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public void RemoveChannel(VibrationChannel channel)
        {
            m_Channels.Remove(channel);
        }

        private void Update()
        {
            if (Gamepad.current == null)
            {
                Debug.LogError("No current gamepad");
                return;
            }

            LowFrequency = 0f;
            HighFrequency = 0f;

            List<VibrationProfileInstance> instancesToRemove = new List<VibrationProfileInstance>();
            m_ActiveProfiles.ForEach((instance) =>
            {
                instance.Update();
                var (newLf, newHf) = instance.CurrentValues();
                
                var channelMagnitude = instance.Channel.Magnitude;
                LowFrequency += newLf * channelMagnitude * MasterMagnitude;
                HighFrequency += newHf * channelMagnitude * MasterMagnitude;
                
                if (instance.Completed()) instancesToRemove.Add(instance);
            });

            m_ActiveProfiles.RemoveAll(i => instancesToRemove.Contains(i));
            
            Gamepad.current.SetMotorSpeeds(LowFrequency, HighFrequency);
        }

        private void OnDisable()
        {
            Gamepad.current.ResetHaptics();
        }
    }
}