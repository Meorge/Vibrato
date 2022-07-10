using UnityEngine;

namespace Meorge.Vibrato
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "New Vibration Profile", menuName = "Vibrato/Vibration Profile", order = 0)]
    public class VibrationProfile : ScriptableObject
    {
        public float duration = 1f;
        public bool loop = true;
        public AnimationCurve lowFrequencyCurve = new AnimationCurve();
        public AnimationCurve highFrequencyCurve = new AnimationCurve();
    }
}