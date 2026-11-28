using System.Collections;
using AshBeacon.Application;
using AshBeacon.Composition;
using AshBeacon.Domain;
using AshBeacon.UnityAdapters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AshBeacon.Tests
{
    public sealed class CombatRoomTests
    {
        private GameplayController game;
        private CombatArena Arena => game.Combat;
        private Rigidbody2D Body => game.Motor.Body;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            foreach (var root in Object.FindObjectsByType<BootstrapRoot>()) Object.Destroy(root.gameObject);
            yield return null;
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync(UnitySceneLoader.GameplayPath);
            yield return null;
            game = Object.FindAnyObjectByType<GameplayController>();
            Assert.That(game, Is.Not.Null);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var room = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("CombatTestCleanup"));
            yield return SceneManager.UnloadSceneAsync(room);
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TapHitsMultipleTargetsOnceIncludingTheWaveTarget()
        {
            Assert.That(GameObject.Find("Training target A"), Is.Not.Null);
            Assert.That(GameObject.Find("Training target B"), Is.Not.Null);
            var left = Target(new Vector2(9.72f, 6.8f));
            var center = Target(new Vector2(10, 6.65f));
            var right = Target(new Vector2(10.28f, 6.8f));
            var behind = Target(new Vector2(10, 5.3f));
            Face(Vector2.up);
            TapSword();
            yield return Steps(24);
            Assert.That(left.Current, Is.EqualTo(4));
            Assert.That(center.Current, Is.EqualTo(4));
            Assert.That(right.Current, Is.EqualTo(4));
            Assert.That(behind.Current, Is.EqualTo(6));
            Assert.That(Arena.ProjectileCount, Is.Zero);
            Assert.That(Arena.Phase, Is.EqualTo(SwordPhase.Idle));
        }

        [UnityTest]
        public IEnumerator HoldingSwordRepeatsAfterRecoveryAndPausingFreezesAnAttack()
        {
            Face(Vector2.right);
            var target = Target(new Vector2(10.75f, 6));
            game.SetTouchButton(TouchControlKind.Sword, true);
            yield return Steps(10);
            Assert.That(target.Current, Is.EqualTo(4));
            yield return Steps(18);
            Assert.That(target.Current, Is.EqualTo(2));

            var phase = Arena.Phase;
            var attack = Arena.Sword.Application;
            game.SetPause(PauseReason.Menu, true);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Arena.Phase, Is.EqualTo(phase));
            Assert.That(Arena.Sword.Application, Is.SameAs(attack));
            Assert.That(target.Current, Is.EqualTo(2));
            game.SetPause(PauseReason.Menu, false);
            yield return Steps(24);
            Assert.That(target.Current, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator WaveStopsAtFirstTargetAndWall()
        {
            Face(Vector2.right);
            var first = Target(new Vector2(12, 6));
            var second = Target(new Vector2(13, 6));
            TapSword();
            yield return Steps(28);
            Assert.That(first.Current, Is.EqualTo(4));
            Assert.That(second.Current, Is.EqualTo(6));
            Assert.That(Arena.ProjectileCount, Is.Zero);

            Wall(new Vector2(11.5f, 6));
            yield return Steps(22);
            TapSword();
            yield return Steps(22);
            Assert.That(first.Current, Is.EqualTo(4));
            Assert.That(Arena.ProjectileCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator WaveHitsTheFirstTargetAtItsVisibleEdge()
        {
            Face(Vector2.right);
            var edge = Target(new Vector2(12, 6.38f));
            var behind = Target(new Vector2(13, 6));
            TapSword();
            yield return Steps(28);
            Assert.That(edge.Current, Is.EqualTo(4));
            Assert.That(behind.Current, Is.EqualTo(6));
            Assert.That(Arena.ProjectileCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator WaveReachesSixCellsButCannotHitBeyondThem()
        {
            Face(Vector2.right);
            var beyond = Target(new Vector2(16.4f, 6));
            TapSword();
            yield return Steps(30);
            Assert.That(Arena.ProjectileCount, Is.EqualTo(1));
            yield return Steps(22);
            Assert.That(Arena.ProjectileCount, Is.Zero);
            Assert.That(beyond.Current, Is.EqualTo(6));
        }

        [UnityTest]
        public IEnumerator ShieldBlocksWhileWalkingButNotDuringAttack()
        {
            game.SetTouchMove(Vector2.right);
            yield return Steps(4);
            Assert.That(Body.linearVelocity.x, Is.GreaterThan(3));
            Stone(Body.position + Vector2.right * 1.1f, Vector2.left, 8);
            yield return Steps(6);
            Assert.That(Arena.Health.Current, Is.EqualTo(12));
            Assert.That(Arena.ProjectileCount, Is.Zero);

            game.SetTouchMove(Vector2.zero);
            TapSword();
            Stone(Body.position + Vector2.right * .5f, Vector2.left, 2);
            yield return Steps(6);
            Assert.That(Arena.Health.Current, Is.EqualTo(10));
            Assert.That(Arena.Phase, Is.EqualTo(SwordPhase.Idle));
            Assert.That(Arena.ProjectileCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator RearHitsUseMaximumAndUnobstructedKnockbackMovesSixTenths()
        {
            var origin = Body.position;
            Stone(origin + Vector2.up * 1.1f, Vector2.down, 2);
            Stone(origin + Vector2.up * 1.1f, Vector2.down, 3);
            Stone(origin + Vector2.down * 1.1f, Vector2.up, 9);
            yield return Steps(11);
            Assert.That(Arena.Health.Current, Is.EqualTo(9));
            Assert.That(Body.position.y, Is.EqualTo(origin.y - .6f).Within(.025f));
            Stone(Body.position + Vector2.up, Vector2.down, 6);
            yield return Steps(8);
            Assert.That(Arena.Health.Current, Is.EqualTo(9));
            TapSword();
            yield return Steps(6);
            Assert.That(Arena.Phase, Is.EqualTo(SwordPhase.Active));
            Assert.That(Arena.ProjectileCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator KnockbackStopsAtWallAndDeathLocksMovementAndAttacks()
        {
            Assert.That(Physics2D.GetIgnoreLayerCollision(8, 9), Is.False);
            Assert.That(Physics2D.GetIgnoreLayerCollision(9, 12), Is.True);
            Body.position = new Vector2(10, .56f);
            Stone(Body.position + Vector2.up, Vector2.down, 2);
            yield return Steps(14);
            Assert.That(Arena.Health.Current, Is.EqualTo(10));
            Assert.That(Body.position.y, Is.InRange(.315f, .38f));

            yield return Steps(42);
            Stone(Body.position + Vector2.up, Vector2.down, 20);
            yield return Steps(8);
            var position = Body.position;
            game.SetTouchMove(Vector2.right);
            game.SetTouchButton(TouchControlKind.Sword, true);
            yield return Steps(10);
            Assert.That(Arena.Health.IsDead, Is.True);
            Assert.That(Body.position, Is.EqualTo(position));
            Assert.That(Arena.Phase, Is.EqualTo(SwordPhase.Idle));
        }

        private void Face(Vector2 direction) => game.Controls.Movement.Read(direction.x, direction.y);

        private void TapSword()
        {
            game.SetTouchButton(TouchControlKind.Sword, true);
            game.SetTouchButton(TouchControlKind.Sword, false);
        }

        private void Stone(Vector2 position, Vector2 direction, int damage) =>
            Arena.LaunchHostileProjectile(position, direction, 20, HitKind.Stone, damage);

        private CombatHealth Target(Vector2 position)
        {
            var target = new GameObject("Combat test target");
            target.layer = LayerMask.NameToLayer("EnemyHitbox");
            target.transform.position = position;
            var hitbox = target.AddComponent<BoxCollider2D>();
            hitbox.size = Vector2.one * .29f;
            hitbox.isTrigger = true;
            Physics2D.SyncTransforms();
            return Arena.RegisterTarget(hitbox, 6);
        }

        private static void Wall(Vector2 position)
        {
            var wall = new GameObject("Combat test wall");
            wall.layer = LayerMask.NameToLayer("WorldSolid");
            wall.transform.position = position;
            wall.AddComponent<BoxCollider2D>().size = new Vector2(.2f, 1);
            Physics2D.SyncTransforms();
        }

        private static IEnumerator Steps(int count)
        {
            for (int i = 0; i < count; i++) yield return new WaitForFixedUpdate();
        }
    }
}
