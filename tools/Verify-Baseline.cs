// Read-only T01 inspection. Run via Unity MCP script_eval(file) on the pinned project.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

if (Application.unityVersion != "6000.6.10f1")
    throw new Exception("Unexpected Editor version; pin the intended project before running.");
if (EditorApplication.isCompiling || EditorApplication.isUpdating)
    throw new Exception("Wait for compilation/import to finish.");
if (EditorSettings.serializationMode != SerializationMode.ForceText ||
    VersionControlSettings.mode != "Visible Meta Files")
    throw new Exception("Incorrect serialization/version control settings.");
if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android ||
    !BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
    throw new Exception("Android target/support is unavailable.");

var issues = new List<string>();
var objectsChecked = 0;
var componentsChecked = 0;
Action<UnityEngine.Object, string> inspect = (obj, path) =>
{
    if (obj == null) { issues.Add(path + ": missing object"); return; }
    objectsChecked++;
    using (var serialized = new SerializedObject(obj))
    {
        var property = serialized.GetIterator();
        while (property.Next(true))
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                property.objectReferenceValue == null &&
                property.objectReferenceEntityIdValue.ToString() != "0:0")
                issues.Add(path + ": missing reference " + property.propertyPath);
    }
};
var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/")).ToArray();
foreach (var path in paths.Where(p => p.EndsWith(".asset") || p.EndsWith(".mat") || p.EndsWith(".scenetemplate")))
    foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path)) inspect(obj, path);
var scenes = paths.Where(p => p.EndsWith(".unity")).ToArray();
foreach (var path in scenes)
{
    var scene = EditorSceneManager.OpenPreviewScene(path);
    try
    {
        foreach (var root in scene.GetRootGameObjects())
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var go = transform.gameObject;
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) != 0)
                    issues.Add(path + ": Missing Script on " + go.name);
                foreach (var component in go.GetComponents<Component>())
                {
                    componentsChecked++;
                    inspect(component, path + "/" + go.name);
                }
            }
    }
    finally { EditorSceneManager.ClosePreviewScene(scene); }
}
if (issues.Count != 0) throw new Exception(string.Join("\n", issues));
return new { result = "PASS", project = Application.dataPath, version = Application.unityVersion,
    target = EditorUserBuildSettings.activeBuildTarget.ToString(), objectsChecked, componentsChecked, scenes,
    sdk = UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath,
    ndk = UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath,
    jdk = UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath };
