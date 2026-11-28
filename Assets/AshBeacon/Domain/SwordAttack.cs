using System;

namespace AshBeacon.Domain
{
    public enum SwordPhase { Idle, Windup, Active, Recovery }

    public sealed class SwordAttack
    {
        public const double WindupTime = .06;
        public const double ActiveTime = .12;
        public const double RecoveryTime = .18;
        public const double WaveInterval = .8;
        public const float BladeLength = 1f;
        public const float BladeWidth = .8f;
        public const float WaveRange = 6f;
        public const float WaveSpeed = 7f;
        public const float WaveLength = .22f;
        public const float WaveWidth = .58f;

        private double started = double.NegativeInfinity;
        private double lastWave = double.NegativeInfinity;
        private bool waveChecked;

        public SwordAttack(int level)
        {
            if (level < 1 || level > 3) throw new ArgumentOutOfRangeException(nameof(level));
            Damage = level * 2;
        }

        public int Damage { get; }
        public Facing Facing { get; private set; }
        public HitApplication Application { get; private set; }

        public SwordPhase Phase(double now)
        {
            double elapsed = now - started + CombatBalance.TimeTolerance;
            if (elapsed < WindupTime) return SwordPhase.Windup;
            if (elapsed < WindupTime + ActiveTime) return SwordPhase.Active;
            return elapsed < WindupTime + ActiveTime + RecoveryTime ? SwordPhase.Recovery : SwordPhase.Idle;
        }

        public static float SpeedMultiplier(SwordPhase phase)
        {
            if (phase == SwordPhase.Active) return 0f;
            return phase == SwordPhase.Idle ? 1f : .5f;
        }

        public bool TryStart(double now, Facing facing)
        {
            if (Phase(now) != SwordPhase.Idle) return false;
            started = now;
            Facing = facing;
            Application = new HitApplication();
            waveChecked = false;
            return true;
        }

        public bool TryLaunchWave(double now, bool fullHealth)
        {
            if (waveChecked || Phase(now) != SwordPhase.Active) return false;
            waveChecked = true;
            if (!fullHealth || now - lastWave + CombatBalance.TimeTolerance < WaveInterval) return false;
            lastWave = now;
            return true;
        }

        public void Interrupt() => started = double.NegativeInfinity;
    }
}
