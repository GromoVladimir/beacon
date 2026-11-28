using UnityEngine;

namespace AshBeacon.UnityAdapters
{
    [CreateAssetMenu(menuName = "AshBeacon/Player definition")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float speed = 3.5f;
        [SerializeField] private Vector2 bodySize = new Vector2(.65f, .65f);
        [SerializeField, Min(1)] private int startingHearts = 3;
        [SerializeField, Range(1, 3)] private int swordLevel = 1;
        [SerializeField, Range(1, 2)] private int shieldLevel = 1;
        public float Speed => speed;
        public Vector2 BodySize => bodySize;
        public int StartingHearts => startingHearts;
        public int SwordLevel => swordLevel;
        public int ShieldLevel => shieldLevel;
    }
}
