// Read-only scene/content validation, run in the Editor with script eval(file).
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AshBeacon.UnityAdapters;

if (EditorApplication.isPlaying || Enumerable.Range(0,SceneManager.sceneCount).Any(i=>SceneManager.GetSceneAt(i).isDirty))
    throw new InvalidOperationException("Stop Play and save working scenes before validation.");
var setup = EditorSceneManager.GetSceneManagerSetup();
try
{
    var scene=EditorSceneManager.OpenScene(UnitySceneLoader.GameplayPath);
    var roots=scene.GetRootGameObjects();
    foreach(var root in roots)
        foreach(var transform in root.GetComponentsInChildren<Transform>(true))
            if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject)>0)
                throw new InvalidOperationException("Missing script: "+transform.name);
    var game=UnityEngine.Object.FindAnyObjectByType<GameplayController>();
    if(game==null || UnityEngine.Object.FindObjectsByType<GameplayController>().Length!=1) throw new InvalidOperationException("Expected one gameplay controller.");
    var data=new SerializedObject(game);
    foreach(var name in new[]{"motor","definition","pauseButton","wheelButton","pauseIndicator","wheelIndicator"})
        if(data.FindProperty(name).objectReferenceValue==null) throw new InvalidOperationException("Missing binding: "+name);
    var controls=data.FindProperty("touchControls");
    if(controls.arraySize!=4) throw new InvalidOperationException("Expected four touch controls.");
    for(int i=0;i<controls.arraySize;i++) if(controls.GetArrayElementAtIndex(i).objectReferenceValue==null) throw new InvalidOperationException("Missing touch control.");
    var definition=AssetDatabase.LoadAssetAtPath<PlayerDefinition>("Assets/AshBeacon/PlayerDefinition.asset");
    if(definition.Speed!=3.5f || definition.BodySize!=new Vector2(.65f,.65f)) throw new InvalidOperationException("Definition differs from GDD.");
    return new { scene=scene.path, missingScripts=0, bindings="valid", speed=definition.Speed, size=definition.BodySize, resetPolicy="Gameplay lifetime", definitionGuid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(definition)) };
}
finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
