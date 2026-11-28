using AshBeacon.Application;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AshBeacon.UnityAdapters
{
    public sealed class GameplayController : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerDefinition definition;
        [SerializeField] private TouchControl[] touchControls;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button wheelButton;
        [SerializeField] private GameObject pauseIndicator;
        [SerializeField] private GameObject wheelIndicator;
        private InputActionMap actions;
        private InputAction move, sword, item, interact, pause, wheel;
        private Vector2 touchMove;
        private bool touchSword, awaitingNeutral;
        private float previousTimeScale;
        private ControlFeedback pauseFeedback, wheelFeedback;
        public PlayerControls Controls { get; private set; }
        public PlayerMotor Motor => motor;

        private void Awake()
        {
            Controls = new PlayerControls();
            pauseFeedback = new ControlFeedback(pauseButton.targetGraphic);
            wheelFeedback = new ControlFeedback(wheelButton.targetGraphic);
            motor.Initialize(Controls, definition);
            foreach (var control in touchControls) control.Initialize(this);
            actions = new InputActionMap("PrototypeGameplay");
            move = actions.AddAction("Move", InputActionType.Value);
            AddMovement("<Keyboard>/w", "<Keyboard>/s", "<Keyboard>/a", "<Keyboard>/d");
            AddMovement("<Keyboard>/upArrow", "<Keyboard>/downArrow", "<Keyboard>/leftArrow", "<Keyboard>/rightArrow");
            sword = actions.AddAction("Sword", InputActionType.Button, "<Keyboard>/j");
            item = actions.AddAction("Item", InputActionType.Button, "<Keyboard>/k");
            interact = actions.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            pause = actions.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            wheel = actions.AddAction("ItemWheel", InputActionType.Button, "<Keyboard>/tab");
            pauseButton.onClick.AddListener(ToggleMenu);
            wheelButton.onClick.AddListener(ToggleWheel);
        }

        private void AddMovement(string up, string down, string left, string right)
        {
            move.AddCompositeBinding("2DVector").With("Up", up).With("Down", down).With("Left", left).With("Right", right);
        }
        private void OnEnable()
        {
            previousTimeScale = Time.timeScale;
            pauseIndicator.SetActive(false);
            wheelIndicator.SetActive(false);
            actions.Enable();
        }
        private void Update()
        {
            if (pause.WasPressedThisFrame()) ToggleMenu();
            if (wheel.WasPressedThisFrame()) ToggleWheel();
            if (Controls.IsPaused) return;
            var keyboardMove = move.ReadValue<Vector2>();
            if (awaitingNeutral)
            {
                if (keyboardMove == Vector2.zero && !sword.IsPressed() && !item.IsPressed() && !interact.IsPressed()) awaitingNeutral = false;
                return;
            }
            var value = touchMove != Vector2.zero ? touchMove : keyboardMove;
            Controls.SetMove(value.x, value.y);
            Controls.SetSword(touchSword || sword.IsPressed());
            if (sword.WasPressedThisFrame() && !sword.IsPressed()) Controls.Press(PlayerCommand.Sword);
            if (item.WasPressedThisFrame()) Controls.Press(PlayerCommand.Item);
            if (interact.WasPressedThisFrame()) Controls.Press(PlayerCommand.Interact);
        }

        public void SetTouchMove(Vector2 value) { touchMove = value; }
        public bool IsKeyboardHeld(TouchControlKind kind)
        {
            if (!isActiveAndEnabled || Controls.IsPaused || awaitingNeutral) return false;
            return kind == TouchControlKind.Item ? item.IsPressed() :
                kind == TouchControlKind.Interact && interact.IsPressed();
        }
        private void LateUpdate()
        {
            pauseFeedback.Apply(Controls.HasPause(PauseReason.Menu));
            wheelFeedback.Apply(Controls.HasPause(PauseReason.ItemWheel));
        }
        public void SetTouchButton(TouchControlKind kind, bool held)
        {
            if (kind == TouchControlKind.Sword) { touchSword = held; Controls.SetSword(held || (sword != null && sword.IsPressed())); }
            else if (held && !Controls.IsPaused) Controls.Press(kind == TouchControlKind.Item ? PlayerCommand.Item : PlayerCommand.Interact);
        }
        public void ToggleMenu() => SetPause(PauseReason.Menu, !Controls.HasPause(PauseReason.Menu));
        public void ToggleWheel() => SetPause(PauseReason.ItemWheel, !Controls.HasPause(PauseReason.ItemWheel));
        public void SetPause(PauseReason reason, bool value)
        {
            Controls.SetPause(reason, value);
            if (value)
            {
                touchMove = Vector2.zero;
                touchSword = false;
                foreach (var control in touchControls) control.ResetControl();
                Controls.ClearInput();
                awaitingNeutral = true;
                if (motor != null) motor.Stop();
            }
            Time.timeScale = Controls.IsPaused ? 0 : previousTimeScale;
            pauseIndicator.SetActive(Controls.HasPause(PauseReason.Menu));
            wheelIndicator.SetActive(Controls.HasPause(PauseReason.ItemWheel));
        }
        private void OnApplicationFocus(bool focused) { if (Controls != null) SetPause(PauseReason.Focus, !focused); }
        private void OnApplicationPause(bool paused) { if (Controls != null) SetPause(PauseReason.Background, paused); }
        private void OnDisable()
        {
            actions.Disable();
            pauseFeedback.Apply(false);
            wheelFeedback.Apply(false);
            foreach (var control in touchControls) control.ResetControl();
            Controls.ClearInput();
            foreach (PauseReason reason in System.Enum.GetValues(typeof(PauseReason))) Controls.SetPause(reason, false);
            awaitingNeutral = true;
            if (motor != null) motor.Stop();
            Time.timeScale = previousTimeScale;
        }
        private void OnDestroy()
        {
            if (pauseButton != null) pauseButton.onClick.RemoveListener(ToggleMenu);
            if (wheelButton != null) wheelButton.onClick.RemoveListener(ToggleWheel);
            actions.Dispose();
        }
    }
}
