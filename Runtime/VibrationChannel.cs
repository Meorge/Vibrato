using System;

namespace Meorge.Vibrato
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable] public class VibrationChannel
    {
        public string Name { get; set; } = "Vibration Channel";
        public float Magnitude { get; set; } = 1f;
    }
}