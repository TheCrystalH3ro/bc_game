using Assets.Scripts.Enums;

namespace Assets.Scripts.Models
{
    public class StatusEffect
    {
        private readonly StatusEffectType type;
        private float value;
        private uint duration;

        public StatusEffect(StatusEffectType type, float value, uint duration)
        {
            this.type = type;
            this.value = value;
            this.duration = duration;
        }

        public bool Equals(StatusEffect other)
        {
            return (this.type == other.GetEffectType() && this.value == other.GetValue());
        }

        public StatusEffectType GetEffectType()
        {
            return type;
        }

        public float GetValue()
        {
            return value;
        }

        public uint GetDuration()
        {
            return duration;
        }

        public void SetDuration(uint duration)
        {
            this.duration = duration;
        }

        public void DecreaseDuration()
        {
            if (duration == 0)
                return;

            duration--;
        }
    }
}