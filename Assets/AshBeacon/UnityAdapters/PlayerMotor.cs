using System.Collections.Generic;
using AshBeacon.Application;
using AshBeacon.Domain;
using UnityEngine;

namespace AshBeacon.UnityAdapters
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private Transform facingMarker;
        private PlayerControls controls;
        private PlayerDefinition definition;
        private Rigidbody2D body;
        private PhysicsMaterial2D material;
        private CombatArena combat;
        private ContactFilter2D solidWalls;
        private readonly List<RaycastHit2D> knockbackHits = new List<RaycastHit2D>();
        public Rigidbody2D Body => body;

        public void Initialize(PlayerControls input, PlayerDefinition settings, CombatArena combatArena)
        {
            controls = input;
            definition = settings;
            combat = combatArena;
            solidWalls.SetLayerMask(LayerMask.GetMask("WorldSolid"));
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            material = new PhysicsMaterial2D("Player frictionless") { friction = 0, bounciness = 0 };
            var collider = GetComponent<BoxCollider2D>();
            collider.size = definition.BodySize;
            collider.sharedMaterial = material;
        }

        private void FixedUpdate()
        {
            if (controls == null) return;
            if (controls.IsPaused)
            {
                Stop();
                return;
            }
            float seconds = Time.fixedDeltaTime;
            combat.Step(controls, body.position, seconds);
            if (combat.Health.IsDead) Stop();
            else if (combat.KnockbackSeconds > CombatBalance.TimeTolerance)
                ApplyKnockback(seconds);
            else
                body.linearVelocity = controls.Move.ToUnityVector() * definition.Speed * SwordAttack.SpeedMultiplier(combat.Phase);
            controls.Advance(seconds);
        }

        private void ApplyKnockback(float seconds)
        {
            Vector2 direction = combat.Health.LastHitDirection.ToUnityVector().normalized;
            float duration = Mathf.Min(seconds, combat.KnockbackSeconds);
            float distance = CombatBalance.KnockbackDistance * duration / (float)CombatBalance.KnockbackTime;
            body.Cast(direction, solidWalls, knockbackHits, distance);
            foreach (var hit in knockbackHits)
            {
                if (Vector2.Dot(hit.normal, direction) < 0)
                    distance = Mathf.Min(distance, Mathf.Max(0, hit.distance - .005f));
            }
            body.linearVelocity = direction * (distance / seconds);
        }

        private void LateUpdate()
        {
            if (controls == null || facingMarker == null) return;
            var facing = combat.Phase == SwordPhase.Idle ? controls.Movement.Facing : combat.Sword.Facing;
            Vector2 direction = facing.ToUnityVector();
            facingMarker.gameObject.SetActive(!combat.Health.IsDead);
            facingMarker.localPosition = (Vector3)(direction * .3f) + new Vector3(0, 0, -.1f);
        }

        public void Stop() { if (body != null) body.linearVelocity = Vector2.zero; }
        private void OnDisable() { Stop(); }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
