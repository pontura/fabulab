using System.Collections.Generic;
using System.Linq;
using MiniGames;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WebMiniGames;

// Creates Assets/Scenes/MiniGames.unity from Game.unity: the app without its login, plus the "WebMiniGames" object
// that opens the minigame screen (MiniGameScreen) as soon as the data of the user is loaded. The rest of the app
// stays in the scene because the minigames use it: the data, the board, and the story player for the cutscenes.
// Run it once to create the scene. Running it again rebuilds it from Game.unity and loses manual changes.
public static class WebMiniGamesSceneBuilder
{
    const string SourceScene = "Assets/Scenes/Game.unity";
    const string TargetScene = "Assets/Scenes/MiniGames.unity";

    // The web page signs the user in: the login of the app is not needed
    static readonly string[] RootsToRemove = { "LoginScreen", "Video Player" };

    [MenuItem("Fabulab/Build Web MiniGames Scene")]
    public static void Build() {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScene) != null) {
            if (!EditorUtility.DisplayDialog("Web MiniGames", TargetScene + " already exists. Rebuilding it from " + SourceScene
                + " loses the changes made to it by hand. Rebuild it?", "Rebuild", "Cancel"))
                return;
            AssetDatabase.DeleteAsset(TargetScene);
        }
        if (!AssetDatabase.CopyAsset(SourceScene, TargetScene)) {
            Debug.LogError("[WebMiniGamesSceneBuilder] Could not copy " + SourceScene);
            return;
        }

        var scene = EditorSceneManager.OpenScene(TargetScene, OpenSceneMode.Single);
        List<string> log = new List<string>();

        if (Object.FindFirstObjectByType<MiniGameScreen>(FindObjectsInactive.Include) == null) {
            Debug.LogError("[WebMiniGamesSceneBuilder] " + SourceScene + " has no MiniGameScreen");
            return;
        }

        foreach (GameObject root in scene.GetRootGameObjects()) {
            if (RootsToRemove.Contains(root.name)) {
                log.Add("Removed " + root.name);
                Object.DestroyImmediate(root);
            }
        }

        GameObject miniGames = new GameObject("WebMiniGames");
        miniGames.AddComponent<WebMiniGamesController>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = miniGames;

        Debug.Log("[WebMiniGamesSceneBuilder] " + TargetScene + " built\n" + string.Join("\n", log));
    }
}
