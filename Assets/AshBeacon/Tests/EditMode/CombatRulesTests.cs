using AshBeacon.Application;
using AshBeacon.Domain;
using NUnit.Framework;

namespace AshBeacon.Tests
{
    public sealed class CombatRulesTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void OneFixedStepAppliesMaximumIndependentOfQueueOrder(bool reverse)
        {
            var target = new CombatHealth(12, CombatBalance.PlayerProtection);
            var weak = Hit(2, new MoveVector(0, 1));
            var strong = Hit(5, new MoveVector(-1, 0));
            Assert.That(target.Queue(reverse ? strong : weak, 0), Is.True);
            Assert.That(target.Queue(reverse ? weak : strong, 0), Is.True);
            Assert.That(target.Resolve(0), Is.EqualTo(5));
            Assert.That(target.Resolve(0), Is.Zero);
            Assert.That(target.Current, Is.EqualTo(7));
            Assert.That(target.LastHitDirection.X, Is.EqualTo(-1));
        }

        [TestCase(CombatBalance.PlayerProtection)]
        [TestCase(CombatBalance.EnemyProtection)]
        [TestCase(CombatBalance.BossProtection)]
        public void RejectedHitDoesNotConsumeApplicationAndHpClampsAtZero(double duration)
        {
            var target = new CombatHealth(3, duration);
            var later = Hit(10, new MoveVector(1, 0));
            Assert.That(target.Queue(later, 0, blocked: true), Is.False);
            Assert.That(target.Queue(Hit(1, new MoveVector(1, 0)), 0), Is.True);
            target.Resolve(0);
            Assert.That(target.Queue(later, duration - .001), Is.False);
            Assert.That(target.Queue(later, duration), Is.True);
            Assert.That(target.Resolve(duration), Is.EqualTo(2));
            Assert.That(target.IsDead, Is.True);
            Assert.That(target.Queue(Hit(1, new MoveVector(1, 0)), 1), Is.False);
        }

        [Test]
        public void LosingCandidateCanHitAfterProtectionExpires()
        {
            var target = new CombatHealth(12, .15);
            var weak = Hit(2, new MoveVector(0, 1));
            target.Queue(weak, 0);
            target.Queue(Hit(5, new MoveVector(-1, 0)), 0);
            target.Resolve(0);
            Assert.That(target.Queue(weak, .149), Is.False);
            Assert.That(target.Queue(weak, .15), Is.True);
            Assert.That(target.Resolve(.15), Is.EqualTo(2));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EqualHitsResolveTheSameApplicationInEitherQueueOrder(bool reverse)
        {
            var target = new CombatHealth(6, .15);
            var first = Hit(2, new MoveVector(1, 0));
            var second = Hit(2, new MoveVector(1, 0));
            target.Queue(reverse ? second : first, 0);
            target.Queue(reverse ? first : second, 0);
            Assert.That(target.Resolve(0), Is.EqualTo(2));
            Assert.That(target.Queue(first, .15), Is.False);
            Assert.That(target.Queue(second, .15), Is.True);
            Assert.That(target.Resolve(.15), Is.EqualTo(2));
            Assert.That(target.Current, Is.EqualTo(2));
        }

        [Test]
        public void ReleasedTapStartsExactlyOneSwingOnFirstStepAfterKnockback()
        {
            var player = new PlayerCombat(12, 1, 1);
            var controls = new PlayerControls();
            const float step = .02f;
            player.BeginStep(controls, step);
            player.QueueHit(Hit(2, new MoveVector(1, 0)), Facing.Down);
            player.ResolveHits();
            for (int i = 1; i <= 5; ++i)
            {
                controls.Advance(step);
                if (i == 5) TapSword(controls);
                player.BeginStep(controls, step);
                Assert.That(player.Phase, Is.EqualTo(SwordPhase.Idle));
            }
            controls.Advance(step);
            player.BeginStep(controls, step);
            Assert.That(player.Phase, Is.EqualTo(SwordPhase.Windup));
            var swing = player.Sword.Application;
            for (int i = 0; i < 25; ++i)
            {
                controls.Advance(step);
                player.BeginStep(controls, step);
            }
            Assert.That(player.Phase, Is.EqualTo(SwordPhase.Idle));
            Assert.That(player.Sword.Application, Is.SameAs(swing));
        }

        [TestCase(2, .14f)]  // First step after knockback is later than the tap buffer.
        [TestCase(12, .02f)] // Fresh input after a lethal hit.
        public void ExpiredOrPostDeathTapDoesNotStartAttack(int damage, float step)
        {
            var player = new PlayerCombat(12, 1, 1);
            var controls = new PlayerControls();
            player.BeginStep(controls, step);
            player.QueueHit(Hit(damage, new MoveVector(1, 0)), Facing.Down);
            player.ResolveHits();
            TapSword(controls);
            controls.Advance(step);
            player.BeginStep(controls, step);
            Assert.That(player.Phase, Is.EqualTo(SwordPhase.Idle));
            Assert.That(player.Sword.Application, Is.Null);
        }

        [Test]
        public void BladeAndWaveShareApplicationButEachTargetCanBeHit()
        {
            var first = new CombatHealth(6, 0);
            var second = new CombatHealth(6, 0);
            var hit = Hit(2, new MoveVector(0, 1));
            Assert.That(first.Queue(hit, 0), Is.True);
            Assert.That(first.Queue(hit, 0), Is.False);
            Assert.That(second.Queue(hit, 0), Is.True);
            first.Resolve(0);
            second.Resolve(0);
            Assert.That(first.Queue(hit, .2), Is.False);
            Assert.That(first.Current, Is.EqualTo(4));
            Assert.That(second.Current, Is.EqualTo(4));
        }

        [TestCase(4, 100, HitKind.Melee, 3)]
        [TestCase(6, 100, HitKind.Melee, 5)]
        [TestCase(8, 100, HitKind.Melee, 6)]
        [TestCase(7, 75, HitKind.Melee, 5)]
        [TestCase(9, 75, HitKind.Fall, 3)]
        public void ArmorThenAccessibilityRoundUpAndFallBypassesArmor(int raw, int percent, HitKind kind, int expected)
        {
            var target = new CombatHealth(12, 0, armored: true, damagePercent: percent);
            target.Queue(new CombatHit(new HitApplication(), kind, raw, new MoveVector(0, 1)), 0);
            Assert.That(target.Resolve(0), Is.EqualTo(expected));
        }

        [TestCase(1, HitKind.Stone, true, -1f, 0f, true)]
        [TestCase(1, HitKind.Seed, true, -1f, 0f, true)]
        [TestCase(1, HitKind.Arrow, true, -1f, 0f, true)]
        [TestCase(1, HitKind.Stone, true, 1f, 0f, false)]
        [TestCase(1, HitKind.Stone, true, -1f, 1.18f, true)]
        [TestCase(1, HitKind.Stone, true, -1f, 1.21f, false)]
        [TestCase(1, HitKind.Stone, false, -1f, 0f, false)]
        [TestCase(2, HitKind.Melee, true, -1f, 0f, false)]
        [TestCase(2, HitKind.Other, true, -1f, 0f, false)]
        [TestCase(1, HitKind.Fire, true, -1f, 0f, false)]
        [TestCase(2, HitKind.Fire, true, -1f, 0f, true)]
        [TestCase(2, HitKind.Magic, true, -1f, 0f, true)]
        public void ShieldChecksProjectileTypeAndIncomingDirection(int level, HitKind kind, bool ready, float x, float y, bool expected)
        {
            Assert.That(Shield.Blocks(level, Facing.Right, ready, kind, new MoveVector(x, y)), Is.EqualTo(expected));
        }

        [Test]
        public void SwordLocksFacingAndMatchesTimingAndSpeedAtBoundaries()
        {
            var sword = new SwordAttack(2);
            Assert.That(sword.TryStart(0, Facing.Left), Is.True);
            Assert.That(sword.TryStart(.059, Facing.Right), Is.False);
            Assert.That(sword.Facing, Is.EqualTo(Facing.Left));
            Assert.That(sword.Damage, Is.EqualTo(4));
            Assert.That(sword.Phase(.059), Is.EqualTo(SwordPhase.Windup));
            Assert.That(sword.Phase(.06), Is.EqualTo(SwordPhase.Active));
            Assert.That(sword.Phase(.18), Is.EqualTo(SwordPhase.Recovery));
            Assert.That(sword.Phase(.36), Is.EqualTo(SwordPhase.Idle));
            Assert.That(SwordAttack.SpeedMultiplier(SwordPhase.Windup), Is.EqualTo(.5f));
            Assert.That(SwordAttack.SpeedMultiplier(SwordPhase.Active), Is.Zero);
            Assert.That(SwordAttack.SpeedMultiplier(SwordPhase.Recovery), Is.EqualTo(.5f));
        }

        [Test]
        public void WaveRequiresFullHealthAndItsOwnCooldownAcrossSwings()
        {
            var sword = new SwordAttack(1);
            sword.TryStart(0, Facing.Up);
            Assert.That(sword.TryLaunchWave(.06, true), Is.True);
            Assert.That(sword.TryLaunchWave(.08, true), Is.False);
            sword.TryStart(.4, Facing.Up);
            Assert.That(sword.TryLaunchWave(.46, true), Is.False);
            sword.TryStart(.8, Facing.Up);
            Assert.That(sword.TryLaunchWave(.86, false), Is.False);
            sword.TryStart(1.2, Facing.Up);
            Assert.That(sword.TryLaunchWave(1.26, true), Is.True);
        }

        private static CombatHit Hit(int damage, MoveVector direction) =>
            new CombatHit(new HitApplication(), HitKind.Melee, damage, direction);

        private static void TapSword(PlayerControls controls)
        {
            controls.SetSword(true);
            controls.SetSword(false);
        }
    }
}
