namespace Meorge.Vibrato
{
    public class VibrationQuickEffect : IVibrationProfile
    {
        public string Name { get; set; } = "Quick Effect";
        public bool Loop { get; set; } = false;
        public float Duration { get; set; }
        
        public float LowFrequency { get; set; }
        public float HighFrequency { get; set; }
        
        public VibrationQuickEffect(float lf, float hf, float duration)
        {
            this.LowFrequency = lf;
            this.HighFrequency = hf;
            this.Duration = duration;
        }
        (float, float) IVibrationProfile.Evaluate(float t)
        {
            return (LowFrequency, HighFrequency);
        }
    }
}