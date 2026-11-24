using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WebEditor;

// Creates Assets/Scenes/Editor.unity from Game.unity: the whole app (editors of stories, characters, objects and
// backgrounds) without its login, plus the "WebEditor" object that opens the editor asked by the web page.
// Run it once to create the scene. Running it again rebuilds it from Game.unity and loses manual changes.
public static class WebEditorSceneBuilder
{
    const string SourceScene = "Assets/Scenes/Game.unity";
    const string TargetScene = "Assets/Scenes/Editor.unity";

    // The web page signs the user in: the login of the app is not needed
    static readonly string[] RootsToRemove = { "LoginScreen", "Video Player" };

    [MenuItem("Fabulab/Build Web Editor Scene")]
    public static void Build() {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScene) != null) {
            if (!EditorUtility.DisplayDialog("Web Editor", TargetScene + " already exists. Rebuilding it from " + SourceScene
                + " loses the changes made to it by hand. Rebuild it?", "Rebuild", "Cancel"))
                return;
            AssetDatabase.DeleteAsset(TargetScene);
        }
        if (!AssetDatabase.CopyAsset(SourceScene, TargetScene)) {
            Debug.LogError("[WebEditorSceneBuilder] Could not copy " + SourceScene);
            return;
        }

        var scene = EditorSceneManager.OpenScene(TargetScene, OpenSceneMode.Single);
        List<string> log = new List<string>();

        foreach (GameObject root in scene.GetRootGameObjects()) {
            if (RootsToRemove.Contains(root.name)) {
                log.Add("Removed " + root.name);
                Object.DestroyImmediate(root);
            }
        }

        GameObject editor = new GameObject("WebEditor");
        editor.AddComponent<WebEditorController>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = editor;

        Debug.Log("[WebEditorSceneBuilder] " + TargetScene + " built\n" + string.Join("\n", log));
    }
}
