using BoardItems.BoardData;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGames.Runner
{
    // A background of the user repeated side by side in X, moving slower than the game and looping
    [Serializable]
    public class RunnerBackground
    {
        [Tooltip("Id of the background. Empty = no background")]
        [SerializeField] string backgroundId;
        [Tooltip("Speed relative to the game speed (1 = same speed as the obstacles)")]
        [SerializeField] float speedFactor = 0.2f;
        [SerializeField] float y = 0;
        [SerializeField] float scale = 1;
        [Tooltip("Drawn behind everything")]
        [SerializeField] int sortingOrder = -300;
        [Tooltip("Uses the color of the background as the color of the stage")]
        [SerializeField] bool useBackgroundColor = true;

        readonly List<Transform> tiles = new List<Transform>();
        float tileWidth;
        float leftOffset; // from the tile position to its left edge
        float halfWidth;

        public void Init(MiniGameStage stage)
        {
            tiles.Clear();
            if (string.IsNullOrEmpty(backgroundId))
                return;

            SObjectData bgData = Data.Instance.sObjectsData.GetSO(backgroundId);
            if (bgData == null)
                Debug.LogWarning("RunnerBackground: background not found in the user's objects: " + backgroundId);
            else if (useBackgroundColor) {
                List<Color> colors = Data.Instance.palettesManager.GetColorsByName(bgData.bg);
                if (colors != null && colors.Count > 0)
                    stage.SetBackgroundColor(colors[0]);
            }

            halfWidth = stage.CameraSize;
            Transform first = CreateTile(stage);
            if (!GetBounds(first, out Bounds bounds) || bounds.size.x <= 0.01f) {
                tiles.Add(first);
                return; // nothing to repeat
            }
            tileWidth = bounds.size.x;
            leftOffset = bounds.min.x - first.position.x;

            // Enough tiles to cover the view plus one more, so there is never a hole while they loop
            int count = Mathf.CeilToInt(stage.CameraSize * 2 / tileWidth) + 1;
            float startX = -halfWidth - leftOffset; // the first tile starts at the left edge of the view
            tiles.Add(first);
            for (int i = 1; i < count; i++)
                tiles.Add(CreateTile(stage));
            for (int i = 0; i < tiles.Count; i++)
                tiles[i].localPosition = new Vector3(startX + i * tileWidth, y, 3);
        }

        public void Clear()
        {
            tiles.Clear(); // the stage destroys the GameObjects
        }

        public void Tick(float gameDistance)
        {
            if (tiles.Count < 2)
                return;

            float step = gameDistance * speedFactor;
            float loopWidth = tiles.Count * tileWidth;
            foreach (Transform t in tiles) {
                Vector3 pos = t.localPosition;
                pos.x -= step;
                // When it is completely out of the view by the left, it goes after the last one
                if (pos.x + leftOffset + tileWidth < -halfWidth)
                    pos.x += loopWidth;
                t.localPosition = pos;
            }
        }

        Transform CreateTile(MiniGameStage stage)
        {
            Transform root = stage.SpawnBackground(backgroundId, new Vector3(0, y, 3), scale);
            SortingGroup sorting = root.gameObject.AddComponent<SortingGroup>();
            sorting.sortingOrder = sortingOrder;
            return root;
        }

        static bool GetBounds(Transform root, out Bounds bounds)
        {
            bounds = default;
            bool found = false;
            foreach (SpriteRenderer sr in root.GetComponentsInChildren<SpriteRenderer>()) {
                if (!sr.enabled || sr.sprite == null) continue;
                if (!found) {
                    bounds = sr.bounds;
                    found = true;
                } else
                    bounds.Encapsulate(sr.bounds);
            }
            return found;
        }
    }
}
