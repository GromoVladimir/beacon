using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AshBeacon.Tests
{
    public sealed class ArtImportTests
    {
        private const string Folder = "Assets/AshBeacon/Tests/Editor/ArtImport/";
        private const string FloorPath = Folder + "env_import_probe_floor_01.png";
        private const string SheetPath = Folder + "prop_import_probe_glow_sheet_01.png";
        private const string ClipPath = Folder + "ImportProbeGlow.anim";
        private const string PrefabPath = Folder + "ArtImportProbe.prefab";
        private const int CellPixels = 32;

        [Test]
        public void FloorFillsFourGridCellsWithContinuousEdges()
        {
            AssertImportProfile(FloorPath, SpriteImportMode.Single, CellPixels);
            var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var grid = contents.GetComponent<Grid>();
                var map = contents.GetComponentInChildren<Tilemap>();
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(FloorPath);
                Assert.That(grid.cellGap, Is.EqualTo(Vector3.zero));
                Assert.That(map.GetComponent<TilemapRenderer>(), Is.Not.Null);
                Assert.That(map.cellBounds.size, Is.EqualTo(new Vector3Int(2, 2, 1)));
                Assert.That(FillsOneCell(sprite, grid.cellSize), Is.True);

                for (var y = 0; y < 2; y++)
                for (var x = 0; x < 2; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    var cellOrigin = (Vector2)map.CellToWorld(cell);
                    var center = (Vector2)map.GetCellCenterWorld(cell);
                    Assert.That(map.GetSprite(cell), Is.EqualTo(sprite));
                    Assert.That(center + (Vector2)sprite.bounds.min, Is.EqualTo(cellOrigin));
                    Assert.That(center + (Vector2)sprite.bounds.max, Is.EqualTo(cellOrigin + Vector2.one));
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }

            AssertMatchingEdges();
        }

        [Test]
        public void ReimportKeepsBothFramesAndExistingConsumerReferences()
        {
            AssertImportProfile(SheetPath, SpriteImportMode.Multiple, 2 * CellPixels);
            var tileBefore = SpriteIdentity(AssetDatabase.LoadAssetAtPath<Sprite>(FloorPath));
            var framesBefore = ReadFrames().Select(SpriteIdentity).ToArray();
            AssertConsumerReferencesAndClip();

            AssetDatabase.ImportAsset(FloorPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(SheetPath, ImportAssetOptions.ForceUpdate);

            AssertImportProfile(FloorPath, SpriteImportMode.Single, CellPixels);
            AssertImportProfile(SheetPath, SpriteImportMode.Multiple, 2 * CellPixels);
            Assert.That(SpriteIdentity(AssetDatabase.LoadAssetAtPath<Sprite>(FloorPath)), Is.EqualTo(tileBefore));
            Assert.That(ReadFrames().Select(SpriteIdentity).ToArray(), Is.EqualTo(framesBefore));
            AssertConsumerReferencesAndClip();
        }

        [Test]
        public void WrongPpuOnIndependentCopyDoesNotFillOneCell()
        {
            var copy = AssetDatabase.GenerateUniqueAssetPath(Folder + "WrongPpu.png");
            Assert.That(AssetDatabase.CopyAsset(FloorPath, copy), Is.True);
            try
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(copy);
                importer.spritePixelsPerUnit = 16;
                importer.SaveAndReimport();

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(copy);
                Assert.That((Vector2)sprite.bounds.size, Is.EqualTo(new Vector2(2, 2)));
                Assert.That(FillsOneCell(sprite, new Vector3(1, 1)), Is.False);
            }
            finally
            {
                AssetDatabase.DeleteAsset(copy);
            }
        }

        private static void AssertImportProfile(string path, SpriteImportMode mode, int width)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.spriteImportMode, Is.EqualTo(mode));
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(CellPixels));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.maxTextureSize, Is.GreaterThanOrEqualTo(width));

            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.GreaterThanOrEqualTo(width));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.RGBA32));

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That((texture.width, texture.height), Is.EqualTo((width, CellPixels)));
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                Assert.That(texture.format, Is.EqualTo(TextureFormat.RGBA32));
        }

        private static bool FillsOneCell(Sprite sprite, Vector3 cellSize)
        {
            return sprite.pixelsPerUnit == CellPixels && cellSize.x == 1 && cellSize.y == 1 &&
                sprite.bounds.size.x == cellSize.x && sprite.bounds.size.y == cellSize.y;
        }

        private static void AssertMatchingEdges()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes(FloorPath)), Is.True);
                var pixels = texture.GetPixels32();
                for (var i = 0; i < CellPixels; i++)
                {
                    Assert.That(pixels[i * CellPixels], Is.EqualTo(pixels[i * CellPixels + CellPixels - 1]),
                        $"Horizontal join at row {i}");
                    Assert.That(pixels[i], Is.EqualTo(pixels[(CellPixels - 1) * CellPixels + i]),
                        $"Vertical join at column {i}");
                }
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        private static Sprite[] ReadFrames()
        {
            var frames = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>()
                .OrderBy(sprite => sprite.rect.x).ToArray();
            Assert.That(frames, Has.Length.EqualTo(2));
            for (var i = 0; i < frames.Length; i++)
            {
                Assert.That(frames[i].rect, Is.EqualTo(new Rect(i * CellPixels, 0, CellPixels, CellPixels)));
                Assert.That(frames[i].pivot, Is.EqualTo(new Vector2(16, 6)));
                Assert.That(frames[i].name, Is.EqualTo($"prop_import_probe_glow_f{i:00}_01"));
            }
            return frames;
        }

        private static (string AssetGuid, long LocalId) SpriteIdentity(Sprite sprite)
        {
            Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out var guid, out long id), Is.True);
            Assert.That(id, Is.Not.Zero);
            return (guid, id);
        }

        private static void AssertConsumerReferencesAndClip()
        {
            var frames = ReadFrames();
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var map = contents.GetComponentInChildren<Tilemap>();
                Assert.That(map.GetSprite(Vector3Int.zero), Is.EqualTo(AssetDatabase.LoadAssetAtPath<Sprite>(FloorPath)));
                var renderer = contents.GetComponentInChildren<SpriteRenderer>();
                Assert.That(renderer.sprite, Is.EqualTo(frames[0]));
                var controller = renderer.GetComponent<Animator>().runtimeAnimatorController;
                Assert.That(controller.animationClips, Does.Contain(clip));
                Assert.That(clip.isLooping, Is.True);

                clip.SampleAnimation(renderer.gameObject, 0.25f);
                Assert.That(renderer.sprite, Is.EqualTo(frames[0]));
                clip.SampleAnimation(renderer.gameObject, 0.75f);
                Assert.That(renderer.sprite, Is.EqualTo(frames[1]));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
    }
}
