using UnityEngine;

namespace Meorge.Vibrato
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "New Vibration Profile", menuName = "Vibrato/Vibration Profile", order = 0)]
    public class VibrationProfileAsset : ScriptableObject, IVibrationProfile
    {
        [SerializeField] private float duration = 1f;
        [SerializeField] private bool loop = true;
        [SerializeField] private AnimationCurve lowFrequencyCurve = new AnimationCurve();
        [SerializeField] private AnimationCurve highFrequencyCurve = new AnimationCurve();

        public string Name
        {
            get => name;
            set { }
        }
        
        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        public float Duration
        {
            get => duration;
            set => duration = value;
        }

        (float, float) IVibrationProfile.Evaluate(float t)
        {
            var nt = t / Duration;
            return (
                lowFrequencyCurve.Evaluate(nt), 
                highFrequencyCurve.Evaluate(nt)
            );
        }
    }
}