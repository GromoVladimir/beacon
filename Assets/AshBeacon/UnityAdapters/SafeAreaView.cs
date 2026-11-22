using UnityEngine;

namespace AshBeacon.UnityAdapters
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaView : MonoBehaviour
    {
        private void Update()
        {
            var safe = Screen.safeArea;
            if (Screen.width <= 0 || Screen.height <= 0) return;
            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
