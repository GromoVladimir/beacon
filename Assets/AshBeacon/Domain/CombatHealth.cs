using System;
using System.Collections.Generic;
using System.Threading;

namespace AshBeacon.Domain
{
    public enum HitKind { Melee, Seed, Arrow, Stone, Fire, Magic, Other, Fall }

    public static class CombatBalance
    {
        public const int HealthPerHeart = 4;
        public const double PlayerProtection = .8;
        public const double EnemyProtection = .15;
        public const double BossProtection = .35;
        public const double KnockbackTime = .12;
        public const float KnockbackDistance = .6f;
        public const double TimeTolerance = 1e-7;
    }

    public static class Shield
    {
        private static readonly double HalfSectorCosine = Math.Cos(50 * Math.PI / 180);

        public static bool Blocks(int level, Facing facing, bool ready, HitKind kind, MoveVector velocity)
        {
            if (!ready || level <= 0) return false;
            bool basic = kind == HitKind.Seed || kind == HitKind.Arrow || kind == HitKind.Stone;
            bool advanced = kind == HitKind.Fire || kind == HitKind.Magic;
            if (!basic && !(level >= 2 && advanced)) return false;

            MoveVector front = facing.Direction();
            double incoming = -(front.X * velocity.X + front.Y * velocity.Y);
            double length = Math.Sqrt((double)velocity.X * velocity.X + (double)velocity.Y * velocity.Y);
            return length > 0 && incoming >= length * (HalfSectorCosine - 1e-7);
        }
    }

    public sealed class HitApplication
    {
        private static long nextSequence;
        private readonly HashSet<CombatHealth> targets = new HashSet<CombatHealth>();
        internal long Sequence { get; } = Interlocked.Increment(ref nextSequence);
        internal bool HasHit(CombatHealth target) => targets.Contains(target);
        internal void Register(CombatHealth target) => targets.Add(target);
    }

    public readonly struct CombatHit
    {
        public CombatHit(HitApplication application, HitKind kind, int damage, MoveVector travel)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            if (damage < 1) throw new ArgumentOutOfRangeException(nameof(damage));
            Kind = kind;
            Damage = damage;
            Travel = travel;
        }

        public HitApplication Application { get; }
        public HitKind Kind { get; }
        public int Damage { get; }
        public MoveVector Travel { get; }
    }

    public sealed class CombatHealth
    {
        private readonly double protection;
        private readonly bool armored;
        private readonly int damagePercent;
        private int pendingDamage;
        private MoveVector pendingDirection;
        private HitApplication pendingApplication;
        private double protectedUntil;

        public CombatHealth(int maximum, double protection, bool armored = false, int damagePercent = 100)
        {
            if (maximum < 1) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (protection < 0 || double.IsNaN(protection) || double.IsInfinity(protection))
                throw new ArgumentOutOfRangeException(nameof(protection));
            if (damagePercent != 100 && damagePercent != 75 && damagePercent != 50)
                throw new ArgumentOutOfRangeException(nameof(damagePercent));
            Current = Maximum = maximum;
            this.protection = protection;
            this.armored = armored;
            this.damagePercent = damagePercent;
        }

        public int Current { get; private set; }
        public int Maximum { get; }
        public bool IsDead => Current == 0;
        public bool IsFull => Current == Maximum;
        public MoveVector LastHitDirection { get; private set; }
        public double LastHitTime { get; private set; } = double.NegativeInfinity;
        public bool IsProtected(double now) => now + CombatBalance.TimeTolerance < protectedUntil;

        public bool Queue(CombatHit hit, double now, bool blocked = false)
        {
            if (blocked || IsDead || IsProtected(now) || hit.Application == null ||
                hit.Application == pendingApplication || hit.Application.HasHit(this)) return false;
            int damage = hit.Kind == HitKind.Fall ? CombatBalance.HealthPerHeart : hit.Damage;
            if (armored && hit.Kind != HitKind.Fall) damage = (int)Math.Ceiling(damage * .75);
            damage = (int)Math.Ceiling(damage * (damagePercent / 100.0));
            if (damage > pendingDamage || (damage == pendingDamage && PrecedesPending(hit)))
            {
                pendingDamage = damage;
                pendingDirection = hit.Travel;
                pendingApplication = hit.Application;
            }
            return true;
        }

        private bool PrecedesPending(CombatHit hit)
        {
            int x = hit.Travel.X.CompareTo(pendingDirection.X);
            if (x != 0) return x < 0;
            int y = hit.Travel.Y.CompareTo(pendingDirection.Y);
            if (y != 0) return y < 0;
            // Creation order stays stable when the same applications arrive in a different order.
            return hit.Application.Sequence < pendingApplication.Sequence;
        }

        public int Resolve(double now)
        {
            int damage = Math.Min(Current, pendingDamage);
            if (damage == 0) return 0;
            pendingApplication.Register(this);
            pendingApplication = null;
            Current -= damage;
            pendingDamage = 0;
            LastHitTime = now;
            LastHitDirection = pendingDirection;
            protectedUntil = now + protection;
            return damage;
        }
    }
}
