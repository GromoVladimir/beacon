using UnityEngine;
using UnityEngine.EventSystems;

namespace AshBeacon.UnityAdapters
{
    public enum TouchControlKind { Stick, Sword, Item, Interact }

    public sealed class TouchControl : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private TouchControlKind kind;
        [SerializeField] private RectTransform knob;
        private GameplayController owner;
        private int? pointer;
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
            if (knob != null) knob.anchoredPosition = Vector2.zero;
            if (owner == null) return;
            if (kind == TouchControlKind.Stick) owner.SetTouchMove(Vector2.zero);
            else owner.SetTouchButton(kind, false);
        }
        private void OnDisable() { ResetControl(); }
    }
}
