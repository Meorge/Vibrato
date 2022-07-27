using UnityEngine;

namespace Meorge.Vibrato
{
    public interface IVibrationProfile
    {
        public string Name { get; set; }
        public bool Loop { get; set; }
        public float Duration { get; set; }
        internal abstract (float, float) Evaluate(float t);
    }
}