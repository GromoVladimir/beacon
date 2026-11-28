using System;
using AshBeacon.Domain;

namespace AshBeacon.Application
{
    public sealed class PlayerCombat
    {
        private readonly int shieldLevel;
        private bool started;

        public PlayerCombat(int maximumHealth, int swordLevel, int shieldLevel)
        {
            this.shieldLevel = shieldLevel;
            Health = new CombatHealth(maximumHealth, CombatBalance.PlayerProtection);
            Sword = new SwordAttack(swordLevel);
        }

        public CombatHealth Health { get; }
        public SwordAttack Sword { get; }
        public double Now { get; private set; }
        public SwordPhase Phase => Sword.Phase(Now);
        public float KnockbackSeconds => (float)Math.Max(0, CombatBalance.KnockbackTime - (Now - Health.LastHitTime));
        private bool IsKnockedBack => KnockbackSeconds > CombatBalance.TimeTolerance;

        public void BeginStep(PlayerControls controls, float seconds)
        {
            if (started) Now += seconds;
            started = true;
            if (Health.IsDead)
            {
                controls.Consume(PlayerCommand.Sword);
                return;
            }
            if (IsKnockedBack || Phase != SwordPhase.Idle) return;
            bool pressed = controls.Consume(PlayerCommand.Sword);
            if (pressed || controls.SwordHeld) Sword.TryStart(Now, controls.Movement.Facing);
        }

        public void QueueHit(CombatHit hit, Facing facing)
        {
            bool ready = Phase == SwordPhase.Idle && !IsKnockedBack;
            bool blocked = Shield.Blocks(shieldLevel, facing, ready, hit.Kind, hit.Travel);
            Health.Queue(hit, Now, blocked);
        }

        public bool ResolveHits()
        {
            if (Health.Resolve(Now) == 0) return false;
            Sword.Interrupt();
            return true;
        }
    }
}
