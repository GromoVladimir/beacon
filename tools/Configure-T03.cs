// Run with script eval(file). Edits Gameplay in the Editor, preserving existing GUIDs.
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using AshBeacon.UnityAdapters;

if (Application.unityVersion != "6000.6.10f1" || !Application.dataPath.Replace('\\','/').EndsWith("/likeZelda/Assets"))
    throw new InvalidOperationException("Unexpected Editor or project.");
if (EditorApplication.isPlaying || Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
    throw new InvalidOperationException("Stop Play and save working scenes before configuring T03.");
var setup = EditorSceneManager.GetSceneManagerSetup();
try
{
    var scene = EditorSceneManager.OpenScene(UnitySceneLoader.GameplayPath);
    if (UnityEngine.Object.FindAnyObjectByType<GameplayController>() != null)
        throw new InvalidOperationException("T03 already configured; inspect before changing it.");
    const string definitionPath = "Assets/AshBeacon/PlayerDefinition.asset";
    var definition = AssetDatabase.LoadAssetAtPath<PlayerDefinition>(definitionPath);
    if (definition == null) { definition = ScriptableObject.CreateInstance<PlayerDefinition>(); AssetDatabase.CreateAsset(definition, definitionPath); }
    var player = new GameObject("Player", typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlayerMotor));
    player.transform.position = new Vector3(10, 6, -.2f);
    const string materialPath = "Assets/AshBeacon/UI/PrototypePlayer.mat";
    var playerMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
    if (playerMaterial == null)
    {
        playerMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name="PrototypePlayer" };
        playerMaterial.SetColor("_BaseColor",new Color(.95f,.62f,.20f));
        AssetDatabase.CreateAsset(playerMaterial,materialPath);
    }
    GameObject Quad(string name, Transform parent, Vector2 size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = name; go.transform.SetParent(parent, false);
        UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.localScale = new Vector3(size.x, size.y, 1);
        go.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/AshBeacon/UI/PrototypeWall.mat");
        return go;
    }
    var visual = Quad("Body", player.transform, new Vector2(.65f,.65f));
    visual.GetComponent<MeshRenderer>().sharedMaterial = playerMaterial;
    var marker = Quad("Facing", player.transform, new Vector2(.20f,.20f));
    marker.transform.localPosition = new Vector3(0,-.3f,-.1f);
    var motor = player.GetComponent<PlayerMotor>();
    var motorData = new SerializedObject(motor);
    motorData.FindProperty("facingMarker").objectReferenceValue = marker.transform;
    motorData.ApplyModifiedPropertiesWithoutUndo();
    var canvas = GameObject.Find("NavigationCanvas");
    var safe = new GameObject("ControlsSafeArea", typeof(RectTransform), typeof(SafeAreaView));
    safe.transform.SetParent(canvas.transform, false);
    var safeRect = (RectTransform)safe.transform;
    safeRect.anchorMin = Vector2.zero; safeRect.anchorMax = Vector2.one; safeRect.offsetMin = safeRect.offsetMax = Vector2.zero;
    RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent,false);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return rect;
    }
    void Label(Transform parent, string text, float size = 30)
    {
        // Legacy UI font is already installed with Unity and includes Cyrillic; no new font assets.
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false);
        var rect = (RectTransform)go.transform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var label = go.GetComponent<Text>(); label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = text; label.fontSize = (int)size; label.alignment = TextAnchor.MiddleCenter; label.raycastTarget = false;
    }
    var stick = Rect("MoveStick", safe.transform, Vector2.zero, new Vector2(170,180), new Vector2(240,240), new Color(.25f,.32f,.38f,.75f));
    var knob = Rect("Knob", stick, new Vector2(.5f,.5f), Vector2.zero, new Vector2(80,80), new Color(.8f,.75f,.5f));
    knob.GetComponent<Image>().raycastTarget = false;
    var stickControl = stick.gameObject.AddComponent<TouchControl>();
    var stickData = new SerializedObject(stickControl); stickData.FindProperty("knob").objectReferenceValue = knob; stickData.ApplyModifiedPropertiesWithoutUndo();
    TouchControl MakeTouch(string name, string text, TouchControlKind kind, Vector2 position, Vector2 size)
    {
        var rect = Rect(name, safe.transform, Vector2.right, position, size, new Color(.62f,.32f,.12f,.9f)); Label(rect,text);
        var control = rect.gameObject.AddComponent<TouchControl>(); var data = new SerializedObject(control);
        data.FindProperty("kind").enumValueIndex = (int)kind; data.ApplyModifiedPropertiesWithoutUndo(); return control;
    }
    var sword = MakeTouch("SwordButton", "Меч", TouchControlKind.Sword, new Vector2(-160,180), new Vector2(190,190));
    var item = MakeTouch("ItemButton", "Предмет", TouchControlKind.Item, new Vector2(-340,110), new Vector2(140,110));
    var interact = MakeTouch("InteractButton", "Действие", TouchControlKind.Interact, new Vector2(-340,250), new Vector2(140,110));
    Button MakeButton(string name, string text, Vector2 anchor, Vector2 position)
    {
        var rect = Rect(name,safe.transform,anchor,position,new Vector2(240,70),new Color(.3f,.35f,.4f));
        Label(rect,text); var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>(); return button;
    }
    var pause = MakeButton("PauseButton","Пауза",Vector2.one,new Vector2(-150,-70));
    var wheel = MakeButton("WheelButton","Выбор предмета",Vector2.up,new Vector2(180,-70));
    var paused = Rect("PauseIndicator",safe.transform,new Vector2(.5f,.5f),new Vector2(0,120),new Vector2(450,70),new Color(.08f,.1f,.15f,.9f)); Label(paused,"Пауза"); paused.GetComponent<Image>().raycastTarget=false; paused.gameObject.SetActive(false);
    var wheelPanel = Rect("WheelIndicator",safe.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(450,70),new Color(.08f,.1f,.15f,.9f)); Label(wheelPanel,"Выбор предмета"); wheelPanel.GetComponent<Image>().raycastTarget=false; wheelPanel.gameObject.SetActive(false);
    var controller = new GameObject("GameplayControls",typeof(GameplayController)).GetComponent<GameplayController>();
    var serialized = new SerializedObject(controller);
    serialized.FindProperty("motor").objectReferenceValue = motor;
    serialized.FindProperty("definition").objectReferenceValue = definition;
    serialized.FindProperty("pauseButton").objectReferenceValue = pause;
    serialized.FindProperty("wheelButton").objectReferenceValue = wheel;
    serialized.FindProperty("pauseIndicator").objectReferenceValue = paused.gameObject;
    serialized.FindProperty("wheelIndicator").objectReferenceValue = wheelPanel.gameObject;
    var controls = serialized.FindProperty("touchControls"); controls.arraySize = 4;
    var touch = new[] { stickControl,sword,item,interact };
    for (int i=0;i<touch.Length;i++) controls.GetArrayElementAtIndex(i).objectReferenceValue=touch[i];
    serialized.ApplyModifiedPropertiesWithoutUndo();
    // Hide the obsolete T02 placeholder and keep navigation above the controls.
    GameObject.Find("Subtitle").SetActive(false);
    var navigation = GameObject.Find("ActionButton").GetComponent<RectTransform>();
    navigation.anchoredPosition = new Vector2(0,-450);
    EditorSceneManager.SaveScene(scene);
    AssetDatabase.SaveAssets();
    return new { scene=scene.path, definition=definitionPath, controls=touch.Length };
}
finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
