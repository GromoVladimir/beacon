using System;
using System.Collections.Generic;
using AshBeacon.Application;
using AshBeacon.Domain;
using UnityEngine;

namespace AshBeacon.UnityAdapters
{
    // All queries and resolutions run before this player's Rigidbody2D simulation step.
    public sealed class CombatArena
    {
        private sealed class Projectile
        {
            public CombatHit Hit;
            public bool Hostile;
            public Vector2 Position;
            public Vector2 Direction;
            public float Speed;
            public float Remaining;
            public Transform View;
        }

        private static readonly int World = LayerMask.GetMask("WorldSolid");
        private static readonly int Enemy = LayerMask.GetMask("EnemyHitbox");
        private static readonly int Player = LayerMask.GetMask("PlayerBody");
        private readonly Dictionary<Collider2D, CombatHealth> targets = new Dictionary<Collider2D, CombatHealth>();
        private readonly List<Collider2D> overlaps = new List<Collider2D>();
        private readonly List<RaycastHit2D> rayHits = new List<RaycastHit2D>();
        private readonly List<Projectile> projectiles = new List<Projectile>();
        private readonly CombatPrototypeView view;
        private readonly PlayerCombat playerCombat;

        internal CombatArena(PlayerDefinition definition, CombatPrototypeView view)
        {
            this.view = view;
            playerCombat = new PlayerCombat(definition.StartingHearts * CombatBalance.HealthPerHeart,
                definition.SwordLevel, definition.ShieldLevel);
            view.DrawHealth(Health);
        }

        public CombatHealth Health => playerCombat.Health;
        public SwordAttack Sword => playerCombat.Sword;
        public SwordPhase Phase => playerCombat.Phase;
        public int ProjectileCount => projectiles.Count;
        public float KnockbackSeconds => playerCombat.KnockbackSeconds;

        public CombatHealth RegisterTarget(Collider2D hitbox, int health)
        {
            var state = new CombatHealth(health, CombatBalance.EnemyProtection);
            targets.Add(hitbox, state);
            return state;
        }

        public void LaunchHostileProjectile(Vector2 position, Vector2 direction, float speed, HitKind kind, int damage)
        {
            if (!(speed > 0) || float.IsInfinity(speed)) throw new ArgumentOutOfRangeException(nameof(speed));
            Vector2 unit = direction.normalized;
            if (!(unit.sqrMagnitude > 0)) throw new ArgumentException("A projectile needs a finite direction.", nameof(direction));
            var hit = new CombatHit(new HitApplication(), kind, damage, unit.ToMoveVector());
            Launch(position, unit, speed, float.PositiveInfinity, hit, true);
        }

        internal void Step(PlayerControls controls, Vector2 origin, float seconds)
        {
            playerCombat.BeginStep(controls, seconds);
            double now = playerCombat.Now;
            if (Phase == SwordPhase.Active)
            {
                Vector2 direction = Sword.Facing.ToUnityVector();
                var strike = new CombatHit(Sword.Application, HitKind.Melee, Sword.Damage, direction.ToMoveVector());
                Strike(origin, direction, strike);
                if (Sword.TryLaunchWave(now, Health.IsFull))
                    Launch(origin, direction, SwordAttack.WaveSpeed, SwordAttack.WaveRange, strike, false);
            }

            AdvanceProjectiles(seconds, controls.Movement.Facing);
            foreach (var target in targets)
            {
                if (target.Value.Resolve(now) == 0) continue;
                if (target.Value.IsDead) target.Key.gameObject.SetActive(false);
            }
            if (playerCombat.ResolveHits()) view.DrawHealth(Health);
            view.DrawBlade(origin, Sword.Facing.ToUnityVector(), Phase == SwordPhase.Active);
            view.DrawPlayer(Health, now);
        }

        private void Strike(Vector2 origin, Vector2 direction, CombatHit strike)
        {
            Vector2 center = origin + direction * (SwordAttack.BladeLength * .5f);
            Vector2 size = direction.x != 0
                ? new Vector2(SwordAttack.BladeLength, SwordAttack.BladeWidth)
                : new Vector2(SwordAttack.BladeWidth, SwordAttack.BladeLength);
            Physics2D.OverlapBox(center, size, 0, Filter(Enemy, true), overlaps);
            foreach (Collider2D collider in overlaps)
            {
                if (!targets.TryGetValue(collider, out var target)) continue;
                if (Physics2D.Linecast(origin, collider.bounds.center, World).collider != null) continue;
                target.Queue(strike, playerCombat.Now);
            }
        }

        private void Launch(Vector2 origin, Vector2 direction, float speed, float range, CombatHit hit, bool hostile)
        {
            projectiles.Add(new Projectile
            {
                Hit = hit, Hostile = hostile, Position = origin, Direction = direction, Speed = speed, Remaining = range,
                View = view.CreateProjectile(origin, direction, hostile)
            });
        }

        private void AdvanceProjectiles(float seconds, Facing facing)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = projectiles[i];
                float distance = Mathf.Min(projectile.Remaining, projectile.Speed * seconds);
                bool finished = Impact(projectile, distance, facing);
                projectile.Remaining -= distance;
                if (finished || projectile.Remaining <= 1e-5f)
                {
                    UnityEngine.Object.Destroy(projectile.View.gameObject);
                    projectiles.RemoveAt(i);
                    continue;
                }
                projectile.Position += projectile.Direction * distance;
                projectile.View.position = new Vector3(projectile.Position.x, projectile.Position.y, -.3f);
            }
        }

        private bool Impact(Projectile projectile, float distance, Facing facing)
        {
            int mask = World | (projectile.Hostile ? Player : Enemy);
            var filter = Filter(mask, true);
            if (projectile.Hostile)
                Physics2D.Raycast(projectile.Position, projectile.Direction, filter, rayHits, distance);
            else
            {
                var size = new Vector2(SwordAttack.WaveLength, SwordAttack.WaveWidth);
                float angle = projectile.Direction.x == 0 ? 90f : 0f;
                Physics2D.BoxCast(projectile.Position, size, angle, projectile.Direction, filter, rayHits, distance);
            }
            // Unity sorts these results by distance. Each projectile stops at the first valid obstacle or target.
            foreach (var hit in rayHits)
            {
                if (((1 << hit.collider.gameObject.layer) & World) != 0) return true;
                if (projectile.Hostile)
                {
                    playerCombat.QueueHit(projectile.Hit, facing);
                    return true;
                }
                if (!targets.TryGetValue(hit.collider, out var target) || target.IsDead) continue;
                target.Queue(projectile.Hit, playerCombat.Now);
                return true;
            }
            return false;
        }

        private static ContactFilter2D Filter(int mask, bool triggers)
        {
            var filter = new ContactFilter2D { useTriggers = triggers };
            filter.SetLayerMask(mask);
            return filter;
        }
    }

    internal static class CombatVectors
    {
        public static Vector2 ToUnityVector(this Facing facing) => facing.Direction().ToUnityVector();
        public static Vector2 ToUnityVector(this MoveVector vector) => new Vector2(vector.X, vector.Y);
        public static MoveVector ToMoveVector(this Vector2 vector) => new MoveVector(vector.x, vector.y);
    }
}
