using StoryPlayer;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Yaguar.StoryMaker.Editor;

// Builds Assets/Scenes/StoryPlayer.unity from Game.unity: keeps the data, the board and the scenario
// (everything needed to draw a story) and removes the app UI. It adds the "StoryPlayer" object that
// receives the id of a story and plays it. Run it again whenever Game.unity changes.
public static class StoryPlayerSceneBuilder
{
    const string SourceScene = "Assets/Scenes/Game.unity";
    const string TargetScene = "Assets/Scenes/StoryPlayer.unity";
    const string RenderTexturePath = "Assets/materials/render.renderTexture";

    // Roots of Game.unity that only belong to the app UI
    static readonly string[] RootsToRemove = { "Canvas", "LoginScreen", "Video Player" };

    [MenuItem("Fabulab/Build Story Player Scene")]
    public static void Build() {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScene) != null) {
            if (!EditorUtility.DisplayDialog("Story Player", TargetScene + " already exists. Rebuild it from " + SourceScene + "?", "Rebuild", "Cancel"))
                return;
            AssetDatabase.DeleteAsset(TargetScene);
        }
        if (!AssetDatabase.CopyAsset(SourceScene, TargetScene)) {
            Debug.LogError("[StoryPlayerSceneBuilder] Could not copy " + SourceScene);
            return;
        }

        var scene = EditorSceneManager.OpenScene(TargetScene, OpenSceneMode.Single);
        List<string> log = new List<string>();

        // The playback settings live in the story editor UI: read them before removing it
        float delayFactor = 0.2f;
        Vector2 limitsCamZoom1 = new Vector2(15, 20);
        float minKeyframeDuration = 0.05f;
        FilmMakerManager filmMaker = Object.FindFirstObjectByType<FilmMakerManager>(FindObjectsInactive.Include);
        if (filmMaker != null) {
            SerializedObject so = new SerializedObject(filmMaker);
            delayFactor = so.FindProperty("delayFactor").floatValue;
            limitsCamZoom1 = so.FindProperty("limitsCamZoom1").vector2Value;
        } else {
            log.Add("FilmMakerManager not found: default playback settings");
        }
        Timeline timeline = Object.FindFirstObjectByType<Timeline>(FindObjectsInactive.Include);
        if (timeline != null)
            minKeyframeDuration = timeline.Min_speed;

        // The character and object of the editing board are hidden in the story maker (StoryEditorPanel)
        UI.BoardUI boardUI = Object.FindFirstObjectByType<UI.BoardUI>(FindObjectsInactive.Include);
        if (boardUI != null) {
            foreach (Component boardItem in new Component[] { boardUI.characterManager, boardUI.sceneobjectsManager }) {
                if (boardItem != null) {
                    boardItem.gameObject.SetActive(false);
                    log.Add("Deactivated board item " + boardItem.name);
                }
            }
        }

        foreach (GameObject root in scene.GetRootGameObjects()) {
            if (RootsToRemove.Contains(root.name)) {
                log.Add("Removed " + root.name);
                Object.DestroyImmediate(root);
            }
        }

        // Items tools that need the removed UI (CanvasZoom reads a scrollbar of the Canvas on Start)
        foreach (var items in Object.FindObjectsByType<BoardItems.Items>(FindObjectsInactive.Include, FindObjectsSortMode.None)) {
            RemoveComponent<BoardItems.UI.CanvasZoom>(items.gameObject, log);
            RemoveComponent<BoardItems.UI.CanvasDragger>(items.gameObject, log);
        }

        Scenario scenario = Object.FindFirstObjectByType<Scenario>(FindObjectsInactive.Include);
        if (scenario == null) {
            Debug.LogError("[StoryPlayerSceneBuilder] No Scenario in the scene");
            return;
        }
        scenario.gameObject.SetActive(true);

        // The camera of the scenario renders a region of the story into a texture
        if (scenario.Cam != null) {
            CameraRegionRenderer regionRenderer = scenario.Cam.gameObject.AddComponent<CameraRegionRenderer>();
            RenderTexture texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(RenderTexturePath);
            if (texture != null) {
                SerializedObject regionSO = new SerializedObject(regionRenderer);
                regionSO.FindProperty("target").objectReferenceValue = texture;
                regionSO.ApplyModifiedPropertiesWithoutUndo();
            } else {
                log.Add(RenderTexturePath + " not found: assign the texture of CameraRegionRenderer");
            }
            log.Add("Added CameraRegionRenderer to " + scenario.Cam.name);
        } else {
            log.Add("The Scenario has no camera: CameraRegionRenderer not added");
        }

        GameObject player = new GameObject("StoryPlayer");
        player.AddComponent<StoryPlayback>();
        StoryPlayerController controller = player.AddComponent<StoryPlayerController>();
        player.AddComponent<StoryPlayerProgressBar>();
        player.AddComponent<StoryPlayerTouchInput>();
        SerializedObject controllerSO = new SerializedObject(controller);
        controllerSO.FindProperty("scenarioCameraManager").objectReferenceValue = scenario.GetComponentInChildren<ScenarioCameraManager>(true);
        controllerSO.FindProperty("delayFactor").floatValue = delayFactor;
        controllerSO.FindProperty("limitsCamZoom1").vector2Value = limitsCamZoom1;
        controllerSO.FindProperty("minKeyframeDuration").floatValue = minKeyframeDuration;
        controllerSO.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = player;

        Debug.Log("[StoryPlayerSceneBuilder] " + TargetScene + " built. delayFactor=" + delayFactor
            + " limitsCamZoom1=" + limitsCamZoom1 + " minKeyframeDuration=" + minKeyframeDuration
            + "\n" + string.Join("\n", log));
    }

    static void RemoveComponent<T>(GameObject go, List<string> log) where T : Component {
        T component = go.GetComponent<T>();
        if (component == null)
            return;
        Object.DestroyImmediate(component);
        log.Add("Removed " + typeof(T).Name + " from " + go.name);
    }
}
