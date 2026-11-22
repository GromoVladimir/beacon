using UnityEngine;

namespace AshBeacon.UnityAdapters
{
    [CreateAssetMenu(menuName = "AshBeacon/Player definition")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float speed = 3.5f;
        [SerializeField] private Vector2 bodySize = new Vector2(.65f, .65f);
        public float Speed => speed;
        public Vector2 BodySize => bodySize;
    }
}
