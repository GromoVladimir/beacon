using UnityEngine;
using UnityEngine.EventSystems;

namespace AshBeacon.UnityAdapters
{
    // Presentation only: never feeds input back into the game.
    internal sealed class ControlFeedback
    {
        private readonly UnityEngine.UI.Graphic graphic;
        private readonly Transform transform;
        private readonly Color normalColor;
        private readonly Vector3 normalScale;
        private readonly UnityEngine.UI.Button button;
        private readonly UnityEngine.UI.ColorBlock normalColors;
        private bool active;

        public ControlFeedback(UnityEngine.UI.Graphic graphic)
        {
            this.graphic = graphic;
            transform = graphic.transform;
            normalColor = graphic.color;
            normalScale = transform.localScale;
            button = graphic.GetComponent<UnityEngine.UI.Button>();
            if (button != null) normalColors = button.colors;
        }

        public void Apply(bool value)
        {
            if (active == value || graphic == null) return;
            active = value;
            graphic.color = active ? new Color(1f, .68f, .22f, normalColor.a) : normalColor;
            transform.localScale = normalScale * (active ? .94f : 1f);
            if (button == null) return;
            var colors = normalColors;
            if (active)
                colors.normalColor = colors.highlightedColor = colors.selectedColor = colors.pressedColor = Color.white;
            button.colors = colors;
        }
    }

    public enum TouchControlKind { Stick, Sword, Item, Interact }

    public sealed class TouchControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private TouchControlKind kind;
        [SerializeField] private RectTransform knob;
        private GameplayController owner;
        private int? pointer;
        private ControlFeedback feedback;
        private void Awake()
        {
            var graphic = GetComponent<UnityEngine.UI.Graphic>();
            if (kind != TouchControlKind.Stick && graphic != null) feedback = new ControlFeedback(graphic);
        }
        private void LateUpdate()
        {
            feedback?.Apply(owner != null && owner.isActiveAndEnabled && !owner.Controls.IsPaused &&
                (kind == TouchControlKind.Sword ? owner.Controls.SwordHeld : pointer.HasValue || owner.IsKeyboardHeld(kind)));
        }
        public void Initialize(GameplayController controller) { owner = controller; }
        public TouchControlKind Kind => kind;

        public void OnPointerDown(PointerEventData data)
        {
            if (pointer.HasValue || owner == null || owner.Controls.IsPaused) return;
            pointer = data.pointerId;
            if (kind == TouchControlKind.Stick) OnDrag(data);
            else owner.SetTouchButton(kind, true);
        }

        public void OnDrag(PointerEventData data)
        {
            if (pointer != data.pointerId || kind != TouchControlKind.Stick) return;
            var rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, data.position, data.pressEventCamera, out var local)) return;
            float radius = Mathf.Min(rect.rect.width, rect.rect.height) * .5f;
            var value = Vector2.ClampMagnitude(local / radius, 1);
            if (value.magnitude < .15f) value = Vector2.zero;
            owner.SetTouchMove(value);
            if (knob != null) knob.anchoredPosition = value * radius * .6f;
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (pointer != data.pointerId) return;
            ResetControl();
        }

        public void ResetControl()
        {
            pointer = null;
            feedback?.Apply(false);
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            if (owner == null) return;
            if (kind == TouchControlKind.Stick) owner.SetTouchMove(Vector2.zero);
            else owner.SetTouchButton(kind, false);
        }
        private void OnDisable() { ResetControl(); }
    }
}
