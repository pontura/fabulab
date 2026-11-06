using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGames.Runner
{
    // Background layer at the top: the objects of the list are repeated in order (A, B, A, B...)
    // and move slower than the game, looping when they leave the view
    [Serializable]
    public class RunnerParallax
    {
        [Tooltip("Ids of the objects that loop in the background")]
        [SerializeField] List<string> objectIds = new List<string>();
        [Tooltip("Speed relative to the game speed (1 = same speed as the obstacles)")]
        [SerializeField] float speedFactor = 0.3f;
        [Tooltip("Y of the line where the objects stand")]
        [SerializeField] float y = 2.5f;
        [SerializeField] float scale = 0.6f;
        [Tooltip("Horizontal distance between objects")]
        [SerializeField] float spacing = 5;
        [Tooltip("Drawn behind the lanes and the obstacles")]
        [SerializeField] int sortingOrder = -200;

        class Item
        {
            public Transform transform; // pivot: at the bottom of the object
            public Transform content;
            public bool pivotReady;
        }

        readonly List<Item> items = new List<Item>();
        float halfWidth;

        public void Init(MiniGameStage stage)
        {
            items.Clear();
            List<string> ids = objectIds.FindAll(id => !string.IsNullOrEmpty(id));
            if (ids.Count == 0 || spacing <= 0)
                return;

            halfWidth = stage.CameraSize;
            // Enough items to cover the view plus one outside at each side
            int count = Mathf.CeilToInt(stage.CameraSize * 2 / spacing) + 2;
            // Multiple of the ids count, so the order is kept when they loop
            count = Mathf.CeilToInt((float)count / ids.Count) * ids.Count;
            float startX = -halfWidth - spacing * 0.5f;
            for (int i = 0; i < count; i++) {
                string id = ids[i % ids.Count];
                Transform content = stage.SpawnObject(id, Vector3.zero).transform;
                Transform pivot = BottomPivot.Wrap(content, "Parallax_" + id);
                pivot.localPosition = new Vector3(startX + i * spacing, y, 2);
                pivot.localScale = Vector3.one * scale;
                SortingGroup sorting = pivot.gameObject.AddComponent<SortingGroup>();
                sorting.sortingOrder = sortingOrder;
                items.Add(new Item { transform = pivot, content = content });
            }
        }

        public void Clear()
        {
            items.Clear(); // the stage destroys the GameObjects
        }

        public void Tick(float gameDistance)
        {
            if (items.Count == 0)
                return;

            float step = gameDistance * speedFactor;
            float loopWidth = items.Count * spacing;
            foreach (Item item in items) {
                if (!item.pivotReady) {
                    item.pivotReady = true;
                    BottomPivot.Align(item.transform, item.content);
                }
                Vector3 pos = item.transform.localPosition;
                pos.x -= step;
                // When it leaves the view by the left, it goes after the last one (keeps the A, B order)
                if (pos.x < -halfWidth - spacing)
                    pos.x += loopWidth;
                item.transform.localPosition = pos;
            }
        }
    }
}
