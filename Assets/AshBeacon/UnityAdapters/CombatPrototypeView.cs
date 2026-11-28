using AshBeacon.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace AshBeacon.UnityAdapters
{
    internal sealed class CombatPrototypeView : System.IDisposable
    {
        private readonly Transform root;
        private readonly Sprite sprite;
        private readonly SpriteRenderer blade;
        private readonly MeshRenderer player;
        private readonly Text health;

        public CombatPrototypeView(Transform parent, MeshRenderer player, Transform safeArea, Text hudTemplate)
        {
            this.player = player;
            root = new GameObject("Combat prototype").transform;
            root.SetParent(parent, false);
            var white = Texture2D.whiteTexture;
            sprite = Sprite.Create(white, new Rect(0, 0, white.width, white.height), new Vector2(.5f, .5f), white.width);
            blade = Shape("Sword hit area", Vector2.zero, Vector2.one, new Color(.52f, .88f, 1f, .55f));
            blade.enabled = false;
            health = CreateHud(safeArea, hudTemplate);
        }

        public void AddTargets(CombatArena arena)
        {
            AddTarget(arena, "Training target A", new Vector2(9.66f, 7.2f));
            AddTarget(arena, "Training target B", new Vector2(10.34f, 7.2f));
        }

        private void AddTarget(CombatArena arena, string name, Vector2 position)
        {
            var target = Shape(name, position, new Vector2(.46f, .6f), new Color(.85f, .39f, .15f));
            target.gameObject.layer = LayerMask.NameToLayer("EnemyHitbox");
            var hitbox = target.gameObject.AddComponent<BoxCollider2D>();
            hitbox.size = Vector2.one;
            hitbox.isTrigger = true;
            arena.RegisterTarget(hitbox, 6);
        }

        public Transform CreateProjectile(Vector2 origin, Vector2 direction, bool hostile)
        {
            Vector2 size = hostile ? new Vector2(.18f, .18f) : new Vector2(SwordAttack.WaveLength, SwordAttack.WaveWidth);
            Color color = hostile ? new Color(.9f, .53f, .25f) : new Color(.2f, .93f, 1f);
            var shape = Shape(hostile ? "Hostile projectile" : "Sword wave", origin, size, color);
            shape.gameObject.layer = LayerMask.NameToLayer("Projectile");
            shape.transform.right = direction;
            return shape.transform;
        }

        public void DrawBlade(Vector2 origin, Vector2 direction, bool active)
        {
            blade.enabled = active;
            if (!active) return;
            Vector2 center = origin + direction * (SwordAttack.BladeLength * .5f);
            blade.transform.position = new Vector3(center.x, center.y, -.12f);
            blade.transform.right = direction;
            blade.transform.localScale = new Vector3(SwordAttack.BladeLength, SwordAttack.BladeWidth, 1);
        }

        public void DrawHealth(CombatHealth state)
        {
            health.text = state.IsDead ? "0 HP" :
                $"{state.Current}/{state.Maximum} HP";
        }

        public void DrawPlayer(CombatHealth state, double now)
        {
            player.enabled = !state.IsDead && (!state.IsProtected(now) || (int)((now - state.LastHitTime) / .08) % 2 == 0);
        }

        private SpriteRenderer Shape(string name, Vector2 position, Vector2 size, Color color)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(root, false);
            renderer.transform.position = new Vector3(position.x, position.y, -.3f);
            renderer.transform.localScale = new Vector3(size.x, size.y, 1);
            renderer.sortingOrder = 2;
            renderer.sprite = sprite;
            renderer.color = color;
            return renderer;
        }

        private static Text CreateHud(Transform safeArea, Text template)
        {
            var label = new GameObject("Combat Health", typeof(RectTransform)).AddComponent<Text>();
            var rect = label.rectTransform;
            rect.SetParent(safeArea, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(22, -18);
            rect.sizeDelta = new Vector2(170, 40);
            label.font = template.font;
            label.fontSize = 24;
            label.color = new Color(1f, .8f, .69f);
            label.raycastTarget = false;
            return label;
        }

        public void Dispose()
        {
            if (health != null) UnityEngine.Object.Destroy(health.gameObject);
            if (root != null) UnityEngine.Object.Destroy(root.gameObject);
            if (sprite != null) UnityEngine.Object.Destroy(sprite);
        }
    }
}
