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
        public Rigidbody2D Body => body;

        public void Initialize(PlayerControls input, PlayerDefinition settings)
        {
            controls = input;
            definition = settings;
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
            controls.Advance(Time.fixedDeltaTime);
            body.linearVelocity = controls.IsPaused ? Vector2.zero : new Vector2(controls.Move.X, controls.Move.Y) * definition.Speed;
        }

        private void LateUpdate()
        {
            if (controls == null || facingMarker == null) return;
            Vector2 direction = controls.Movement.Facing == Facing.Left ? Vector2.left :
                controls.Movement.Facing == Facing.Right ? Vector2.right :
                controls.Movement.Facing == Facing.Up ? Vector2.up : Vector2.down;
            facingMarker.localPosition = (Vector3)(direction * .3f) + new Vector3(0, 0, -.1f);
        }

        public void Stop() { if (body != null) body.linearVelocity = Vector2.zero; }
        private void OnDisable() { Stop(); }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
