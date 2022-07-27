using System;
using UnityEngine;

namespace Meorge.Vibrato
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable] public class VibrationProfileInstance
    {
        /// <summary>
        /// 
        /// </summary>
        public enum ProfileInstanceStatus
        {
            Playing,
            Paused,
            Complete
        }

        /// <summary>
        /// 
        /// </summary>
        public ProfileInstanceStatus Status { get; private set; }= ProfileInstanceStatus.Playing;
        
        /// <summary>
        /// 
        /// </summary>
        public float Magnitude { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public VibrationChannel Channel => m_Channel;
        
        public string Name => m_Profile.Name;
        
        private IVibrationProfile m_Profile;
        private float m_T;
        private VibrationChannel m_Channel;

        internal VibrationProfileInstance(IVibrationProfile profile, VibrationChannel channel, float magnitude = 1f)
        {
            m_Profile = profile;
            m_Channel = channel;
            Magnitude = magnitude;
        }
            
        internal void Update()
        {
            if (Status != ProfileInstanceStatus.Playing) return;
                
            m_T += Time.deltaTime;
                
            if (m_T < m_Profile.Duration) return;
                
            if (m_Profile.Loop)
            {
                m_T -= m_Profile.Duration;
            }
            else
            {
                Status = ProfileInstanceStatus.Complete;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Completed() => Status == ProfileInstanceStatus.Complete;
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public (float, float) CurrentValues()
        {
            var (lf, hf) = m_Profile.Evaluate(m_T);
            return (lf * Magnitude, hf * Magnitude);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Pause() => Status = ProfileInstanceStatus.Paused;
        
        /// <summary>
        /// 
        /// </summary>
        public void Kill() => Status = ProfileInstanceStatus.Complete;
    }
}