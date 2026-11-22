using UnityEngine;

namespace AshBeacon.UnityAdapters
{
    [RequireComponent(typeof(Camera))]
    public sealed class RoomCamera : MonoBehaviour
    {
        private Camera roomCamera;
        private void Awake() { roomCamera = GetComponent<Camera>(); }
        private void LateUpdate()
        {
            // Keep the entire 20 x 12 room visible, including a one-cell margin.
            roomCamera.orthographicSize = Mathf.Max(7f, 11f / roomCamera.aspect);
        }
    }
}
