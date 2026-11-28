using System.Collections;
using System.Linq;
using AshBeacon.Application;
using AshBeacon.Composition;
using AshBeacon.UnityAdapters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AshBeacon.Tests
{
    public sealed class PlayerMovementTests
    {
        private GameplayController game;
        private Keyboard keyboard;
        private InputSettings.BackgroundBehavior backgroundBehavior;
        private InputSettings.EditorInputBehaviorInPlayMode editorInputBehavior;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            foreach (var root in Object.FindObjectsByType<BootstrapRoot>()) Object.Destroy(root.gameObject);
            yield return null;
            Time.timeScale = 1;
            // CLI runs may leave the Game view unfocused. Keep synthetic input in the player loop.
            backgroundBehavior = InputSystem.settings.backgroundBehavior;
            editorInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync(UnitySceneLoader.GameplayPath);
            yield return null;
            game = Object.FindAnyObjectByType<GameplayController>();
            Assert.That(game, Is.Not.Null, "Run tools/Configure-T03.cs in the Editor first.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.CreateScene("T03TestCleanup"));
            yield return SceneManager.UnloadSceneAsync(previous);
            InputSystem.settings.editorInputBehaviorInPlayMode = editorInputBehavior;
            InputSystem.settings.backgroundBehavior = backgroundBehavior;
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        private void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        private static IEnumerator Steps(int count)
        {
            for (int i = 0; i < count; i++) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator ActionButtonsShowIndependentHoldsAndClearOnFocusLoss()
        {
            var buttons = Object.FindObjectsByType<TouchControl>().Where(x => x.Kind != TouchControlKind.Stick).ToArray();
            var colors = buttons.Select(x => x.GetComponent<UnityEngine.UI.Graphic>().color).ToArray();
            var scales = buttons.Select(x => x.transform.localScale).ToArray();
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].OnPointerDown(new PointerEventData(EventSystem.current) { pointerId = 200 + i });
            yield return null; yield return null;
            for (int i = 0; i < buttons.Length; i++)
            {
                Assert.That(buttons[i].GetComponent<UnityEngine.UI.Graphic>().color, Is.Not.EqualTo(colors[i]));
                Assert.That(buttons[i].transform.localScale.x, Is.EqualTo(scales[i].x * .94f).Within(.001f));
            }
            buttons[0].OnPointerUp(new PointerEventData(EventSystem.current) { pointerId = 999 });
            yield return null; yield return null;
            Assert.That(buttons[0].transform.localScale.x, Is.EqualTo(scales[0].x * .94f).Within(.001f));
            buttons[0].OnPointerUp(new PointerEventData(EventSystem.current) { pointerId = 200 });
            yield return null; yield return null;
            Assert.That(buttons[0].GetComponent<UnityEngine.UI.Graphic>().color, Is.EqualTo(colors[0]));
            Assert.That(buttons[1].GetComponent<UnityEngine.UI.Graphic>().color, Is.Not.EqualTo(colors[1]));
            game.SendMessage("OnApplicationFocus", false);
            yield return null; yield return null;
            for (int i = 0; i < buttons.Length; i++)
            {
                Assert.That(buttons[i].GetComponent<UnityEngine.UI.Graphic>().color, Is.EqualTo(colors[i]));
                Assert.That(buttons[i].transform.localScale, Is.EqualTo(scales[i]));
            }
        }

        [UnityTest]
        public IEnumerator KeyboardFeedbackAndMenuHighlightsWorkWhilePaused()
        {
            var buttons = Object.FindObjectsByType<TouchControl>().Where(x => x.Kind != TouchControlKind.Stick).ToArray();
            var colors = buttons.Select(x => x.GetComponent<UnityEngine.UI.Graphic>().color).ToArray();
            Keys(Key.J, Key.K, Key.E);
            yield return null; yield return null;
            for (int i = 0; i < buttons.Length; i++)
                Assert.That(buttons[i].GetComponent<UnityEngine.UI.Graphic>().color, Is.Not.EqualTo(colors[i]));
            Keys(); yield return null; yield return null;
            for (int i = 0; i < buttons.Length; i++)
                Assert.That(buttons[i].GetComponent<UnityEngine.UI.Graphic>().color, Is.EqualTo(colors[i]));
            var pause = GameObject.Find("PauseButton").GetComponent<UnityEngine.UI.Button>();
            var wheel = GameObject.Find("WheelButton").GetComponent<UnityEngine.UI.Button>();
            var pauseColor = pause.targetGraphic.color;
            var wheelColor = wheel.targetGraphic.color;
            game.ToggleMenu(); game.ToggleWheel();
            yield return null; yield return null;
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(pause.targetGraphic.color, Is.Not.EqualTo(pauseColor));
            Assert.That(wheel.targetGraphic.color, Is.Not.EqualTo(wheelColor));
            game.ToggleMenu(); yield return null; yield return null;
            Assert.That(Time.timeScale, Is.Zero);
            Assert.That(pause.targetGraphic.color, Is.EqualTo(pauseColor));
            Assert.That(wheel.targetGraphic.color, Is.Not.EqualTo(wheelColor));
            game.ToggleWheel(); yield return null; yield return null;
            Assert.That(wheel.targetGraphic.color, Is.EqualTo(wheelColor));
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator KeyboardMovementHasEqualDiagonalSpeedAndStopsOnRelease()
        {
            var body = game.Motor.Body;
            Keys(Key.D);
            yield return null;
            yield return Steps(2);
            Vector2 start = body.position;
            double started = Time.fixedTimeAsDouble;
            yield return Steps(10);
            double axisSpeed = Vector2.Distance(start, body.position) / (Time.fixedTimeAsDouble - started);
            Keys(); yield return null; yield return Steps(2);
            Keys(Key.D, Key.W); yield return null;
            yield return Steps(2);
            start = body.position;
            started = Time.fixedTimeAsDouble;
            yield return Steps(10);
            double diagonalSpeed = Vector2.Distance(start, body.position) / (Time.fixedTimeAsDouble - started);
            Assert.That(axisSpeed, Is.EqualTo(3.5).Within(.03));
            Assert.That(diagonalSpeed, Is.EqualTo(axisSpeed).Within(.03));
            Assert.That(body.linearVelocity.magnitude, Is.EqualTo(3.5f).Within(.001));
            Keys(); yield return null; yield return Steps(2);
            Assert.That(body.linearVelocity, Is.EqualTo(Vector2.zero));
        }

        [UnityTest]
        public IEnumerator SolidWallsAllowSlidingAndEscapeFromCorner()
        {
            var body = game.Motor.Body;
            Assert.That(game.Motor.GetComponent<BoxCollider2D>().size, Is.EqualTo(new Vector2(.65f,.65f)));
            body.position = new Vector2(19.6f, 5);
            Keys(Key.D, Key.W); yield return null;
            yield return Steps(20);
            Assert.That(body.position.x, Is.LessThan(19.7f));
            Assert.That(body.position.y, Is.GreaterThan(5.5f));
            body.position = new Vector2(19.6f,11.6f);
            yield return Steps(15);
            Assert.That(body.position.x, Is.LessThan(19.7f));
            Assert.That(body.position.y, Is.LessThan(11.7f));
            Vector2 corner = body.position;
            Keys(Key.A, Key.S); yield return null; yield return Steps(15);
            Assert.That(body.position.x, Is.LessThan(corner.x-.4f));
            Assert.That(body.position.y, Is.LessThan(corner.y-.4f));
            body.position = new Vector2(.4f,.4f);
            yield return Steps(15);
            Assert.That(body.position.x, Is.GreaterThan(.3f));
            Assert.That(body.position.y, Is.GreaterThan(.3f));
        }

        [UnityTest]
        public IEnumerator TwoPointerOwnersKeepSwordAndStickIndependent()
        {
            var all = Object.FindObjectsByType<TouchControl>();
            var stick = all.Single(x => x.Kind == TouchControlKind.Stick);
            var sword = all.Single(x => x.Kind == TouchControlKind.Sword);
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)stick.transform;
            var screen = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(new Vector3(rect.rect.width*.45f,0,0)));
            var movePointer = new PointerEventData(EventSystem.current) { pointerId = 101, position = screen };
            var swordPointer = new PointerEventData(EventSystem.current) { pointerId = 102 };
            stick.OnPointerDown(movePointer);
            sword.OnPointerDown(swordPointer);
            yield return null; yield return Steps(3);
            Assert.That(game.Controls.Move.X, Is.GreaterThan(.5f));
            Assert.That(game.Controls.SwordHeld, Is.True);
            stick.OnPointerUp(swordPointer); // another finger cannot release the stick
            sword.OnPointerUp(swordPointer);
            yield return null;
            Assert.That(game.Controls.SwordHeld, Is.False);
            Assert.That(game.Controls.Move.X, Is.GreaterThan(.5f));
            sword.OnPointerDown(swordPointer);
            stick.OnPointerUp(movePointer);
            yield return null;
            Assert.That(game.Controls.Move.X, Is.Zero);
            Assert.That(game.Controls.SwordHeld, Is.True);
            game.SendMessage("OnApplicationFocus", false);
            game.SendMessage("OnApplicationFocus", true);
            yield return null;
            Assert.That(game.Controls.SwordHeld, Is.False);
            Assert.That(game.Controls.Consume(PlayerCommand.Sword), Is.False);
        }

        [UnityTest]
        public IEnumerator PauseOwnersFreezeWorldAndRequireFreshKeyboardInput()
        {
            Keys(Key.D, Key.J); yield return null; yield return Steps(3);
            Assert.That(game.Controls.SwordHeld, Is.True);
            game.SetPause(PauseReason.Menu, true);
            game.SendMessage("OnApplicationPause", true);
            var position = game.Motor.Body.position;
            yield return null; yield return null;
            Assert.That(game.Motor.Body.position, Is.EqualTo(position));
            Assert.That(Time.timeScale, Is.Zero);
            game.ToggleMenu();
            Assert.That(Time.timeScale, Is.Zero);
            game.SendMessage("OnApplicationPause", false);
            yield return null; yield return Steps(2);
            Assert.That(game.Controls.SwordHeld, Is.False);
            Assert.That(game.Controls.Move.X, Is.Zero);
            Keys(); yield return null; yield return null;
            Keys(Key.D,Key.J); yield return null;
            // The paused swing resumes first; movement returns during its recovery.
            yield return Steps(10);
            Assert.That(game.Controls.SwordHeld, Is.True);
            Assert.That(game.Motor.Body.position.x, Is.GreaterThan(position.x));
            game.ToggleWheel();
            Assert.That(Time.timeScale, Is.Zero); // teardown must restore time even while paused
        }
    }
}
