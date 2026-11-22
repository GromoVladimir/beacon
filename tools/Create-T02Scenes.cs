// Run through Unity script_eval(file). Creates new scenes only; never overwrites existing scenes.
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using AshBeacon.Composition;
using AshBeacon.UnityAdapters;

if (Application.unityVersion != "6000.6.10f1" || !Application.dataPath.Replace('\\','/').EndsWith("/likeZelda/Assets"))
    throw new InvalidOperationException("Unexpected project or Editor.");
if (EditorApplication.isPlaying || Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
    throw new InvalidOperationException("Stop Play and save your working scenes first.");
var paths = new[] { UnitySceneLoader.BootstrapPath, UnitySceneLoader.MenuPath, UnitySceneLoader.GameplayPath };
if (paths.Any(p => AssetDatabase.LoadAssetAtPath<SceneAsset>(p) != null))
    throw new InvalidOperationException("T02 scenes already exist; inspect and edit them in place.");
if (!AssetDatabase.IsValidFolder("Assets/AshBeacon/UI")) AssetDatabase.CreateFolder("Assets/AshBeacon", "UI");
var font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf"), 48, 5,
    UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024);
font.name = "Prototype Cyrillic";
string characters = "Пепельный маякМеханический прототипНачать прототипПроверочная комната 20 × 12В главное менюДвижение и бой — следующий этап.Не удалось открыть экран. Попробуйте ещё раз.";
if (!font.TryAddCharacters(characters, out string missing)) throw new InvalidOperationException("Font missing characters: " + missing);
font.atlasPopulationMode = AtlasPopulationMode.Static;
AssetDatabase.CreateAsset(font, "Assets/AshBeacon/UI/PrototypeFont.asset");
AssetDatabase.AddObjectToAsset(font.material, font);
foreach (var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture, font);
var settings = new SerializedObject(TMP_Settings.instance);
settings.FindProperty("m_defaultFontAsset").objectReferenceValue = font;
settings.FindProperty("m_defaultSpriteAsset").objectReferenceValue = null; // No sprite text in this prototype.
settings.ApplyModifiedPropertiesWithoutUndo();

Camera MakeCamera(bool room)
{
    var camera = new GameObject("MainCamera", typeof(Camera)).GetComponent<Camera>();
    camera.tag = "MainCamera";
    camera.orthographic = true;
    camera.orthographicSize = 7;
    camera.transform.position = room ? new Vector3(10,6,-10) : new Vector3(0,0,-10);
    camera.clearFlags = CameraClearFlags.SolidColor;
    camera.backgroundColor = new Color(0.055f,0.075f,0.095f);
    if (room) camera.gameObject.AddComponent<RoomCamera>();
    return camera;
}
TextMeshProUGUI Label(Transform parent, string name, string text, Vector2 position, Vector2 size, float fontSize)
{
    var go = new GameObject(name, typeof(RectTransform));
    go.transform.SetParent(parent, false);
    var rect = (RectTransform)go.transform;
    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
    rect.anchoredPosition = position;
    rect.sizeDelta = size;
    var label = go.AddComponent<TextMeshProUGUI>();
    label.font = font;
    label.text = text;
    label.fontSize = fontSize;
    label.alignment = TextAlignmentOptions.Center;
    label.color = new Color(.93f,.91f,.84f);
    label.raycastTarget = false;
    return label;
}
void MakeUI(bool gameplay)
{
    var canvas = new GameObject("NavigationCanvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
    canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
    var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new Vector2(1920,1080);
    scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
    Label(canvas.transform, "Title", gameplay ? "Проверочная комната 20 × 12" : "Пепельный маяк",
        new Vector2(0,gameplay ? 450 : 180), new Vector2(1100,100), gameplay ? 36 : 72);
    Label(canvas.transform, "Subtitle", gameplay ? "Движение и бой — следующий этап." : "Механический прототип",
        new Vector2(0,gameplay ? -470 : 60), new Vector2(1100,60), 30);
    var buttonObject = new GameObject("ActionButton", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
    buttonObject.transform.SetParent(canvas.transform,false);
    var rect = (RectTransform)buttonObject.transform;
    rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f);
    rect.anchoredPosition = new Vector2(0,gameplay ? -380 : -90);
    rect.sizeDelta = new Vector2(480,92);
    var image = buttonObject.GetComponent<UnityEngine.UI.Image>();
    image.color = new Color(.68f,.36f,.15f);
    var button = buttonObject.GetComponent<UnityEngine.UI.Button>();
    button.targetGraphic = image;
    Label(buttonObject.transform, "Label", gameplay ? "В главное меню" : "Начать прототип", Vector2.zero, new Vector2(450,80), 34);
    var status = Label(canvas.transform,"Status","",new Vector2(0,-230),new Vector2(1200,100),28);
    var view = canvas.AddComponent<NavigationView>();
    var serialized = new SerializedObject(view);
    serialized.FindProperty("actionButton").objectReferenceValue = button;
    serialized.FindProperty("statusText").objectReferenceValue = status;
    serialized.ApplyModifiedPropertiesWithoutUndo();
    new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
}
Material MakeMaterial(string name, Color color)
{
    var shader = Shader.Find("Universal Render Pipeline/Unlit");
    if (shader == null) throw new InvalidOperationException("URP Unlit shader not found.");
    var material = new Material(shader) { name = name };
    material.SetColor("_BaseColor",color);
    AssetDatabase.CreateAsset(material,"Assets/AshBeacon/UI/"+name+".mat");
    return material;
}
void Quad(Transform parent,string name,Vector3 position,Vector2 size,Material material,bool solid)
{
    var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
    go.name=name;
    go.transform.SetParent(parent,false);
    go.transform.position=position;
    go.transform.localScale=new Vector3(size.x,size.y,1);
    UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
    go.GetComponent<MeshRenderer>().sharedMaterial=material;
    if(solid) go.AddComponent<BoxCollider2D>();
}
var setup = EditorSceneManager.GetSceneManagerSetup();
try
{
    var bootstrap = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    new GameObject("BootstrapRoot",typeof(BootstrapRoot));
    EditorSceneManager.SaveScene(bootstrap,paths[0]);
    var menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    MakeCamera(false); MakeUI(false);
    EditorSceneManager.SaveScene(menu,paths[1]);
    var game = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    MakeCamera(true); MakeUI(true);
    var room = new GameObject("TestRoom");
    var floor = MakeMaterial("PrototypeFloor",new Color(.14f,.20f,.23f));
    var wall = MakeMaterial("PrototypeWall",new Color(.29f,.34f,.36f));
    Quad(room.transform,"Floor",new Vector3(10,6,0),new Vector2(20,12),floor,false);
    Quad(room.transform,"WestWall",new Vector3(-.5f,6,-.05f),new Vector2(1,14),wall,true);
    Quad(room.transform,"EastWall",new Vector3(20.5f,6,-.05f),new Vector2(1,14),wall,true);
    Quad(room.transform,"SouthWall",new Vector3(10,-.5f,-.05f),new Vector2(20,1),wall,true);
    Quad(room.transform,"NorthWall",new Vector3(10,12.5f,-.05f),new Vector2(20,1),wall,true);
    EditorSceneManager.SaveScene(game,paths[2]);
    AssetDatabase.SaveAssets();
    EditorBuildSettings.scenes=paths.Select(p => new EditorBuildSettingsScene(p,true)).ToArray();
}
finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
EditorSceneManager.OpenScene(paths[0]);
// Do not override playModeStartScene: Unity Test Runner must start its own scene.
return new { created=paths, font=AssetDatabase.GetAssetPath(font), project=Application.dataPath };
