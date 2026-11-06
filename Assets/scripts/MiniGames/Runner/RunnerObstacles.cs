using BoardItems.Characters;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGames.Runner
{
    // A character used as obstacle, doing an action
    [Serializable]
    public class ObstacleCharacter
    {
        public string characterId;
        public CharacterAction action = CharacterAction.Stand;
    }

    // Spawns obstacles in waves on random lanes and moves them towards the character.
    // Each obstacle is a random entry of the characters list. If the list is empty, the objects
    // of the user are used. Obstacles are loaded the first time they are needed and then reused;
    // the pool has a max size so it doesn't grow with long lists
    [Serializable]
    public class RunnerObstacles
    {
        [Tooltip("Characters used as obstacles. Empty = the objects of the user")]
        [SerializeField] List<ObstacleCharacter> characters = new List<ObstacleCharacter>();
        [Tooltip("The characters look towards the player")]
        [SerializeField] bool charactersFaceLeft = true;

        [Tooltip("Max obstacles kept loaded (active + reusable)")]
        [SerializeField] int maxPoolSize = 16;
        [SerializeField] float scale = 1;
        [Tooltip("Vertical offset relative to the lane Y")]
        [SerializeField] float offsetY = 0;
        [Tooltip("Horizontal offset per lane: lane 0 (bottom) = 0, and it grows towards the top lanes, " +
                 "so obstacles of the same wave never share the same X")]
        [SerializeField] float laneOffsetX = 0.6f;
        [Tooltip("Distance traveled between waves (random between min and max)")]
        [SerializeField] float minSpacing = 4;
        [SerializeField] float maxSpacing = 7;
        [Tooltip("Max obstacles at the same time in one wave. There is always at least one free lane")]
        [SerializeField] int maxPerWave = 2;
        [SerializeField] float firstWaveDistance = 3;
        [Tooltip("Extra distance outside the view to spawn and remove obstacles")]
        [SerializeField] float margin = 2;
        [Tooltip("Shown when there are no characters nor objects")]
        [SerializeField] Color placeholderColor = new Color(0.9f, 0.45f, 0.35f);

        // What an obstacle shows: a character with an action, an object, or a placeholder (both empty)
        struct Source
        {
            public string characterId;
            public string animName;
            public string objectId;
            public string Key => characterId != null ? "character_" + characterId + "_" + animName : "object_" + objectId;
        }

        class Obstacle
        {
            public string key;
            public Transform transform; // pivot: at the bottom of the obstacle
            public Transform content;   // the loaded character/object, moved so its lowest point is on the pivot
            public CharacterManager character; // null when it is an object
            public string animName;
            public SortingGroup sorting;
            public float baseScale;
            public int lane;
            public bool active;
            public bool pivotReady;
        }

        readonly List<Obstacle> pool = new List<Obstacle>();
        readonly List<int> laneOrder = new List<int>();
        readonly List<Source> sources = new List<Source>();
        MiniGameStage stage;
        RunnerLanes lanes;
        float distanceToNextWave;
        float spawnX;
        float despawnX;

        public void Init(MiniGameStage stage, RunnerLanes lanes, List<string> userObjectIds)
        {
            this.stage = stage;
            this.lanes = lanes;
            pool.Clear();
            spawnX = stage.CameraSize + margin;
            despawnX = -spawnX;
            distanceToNextWave = firstWaveDistance;

            sources.Clear();
            foreach (ObstacleCharacter c in characters) {
                if (c != null && !string.IsNullOrEmpty(c.characterId))
                    sources.Add(new Source { characterId = c.characterId, animName = CharacterActions.GetAnimName(c.action) });
            }
            if (sources.Count == 0 && userObjectIds != null) {
                foreach (string id in userObjectIds)
                    sources.Add(new Source { objectId = id });
            }
        }

        public void Clear()
        {
            pool.Clear(); // the stage destroys the GameObjects
        }

        public void Tick(float distance)
        {
            foreach (Obstacle o in pool) {
                if (!o.active) continue;
                if (!o.pivotReady)
                    SetPivotAtBottom(o);
                Vector3 pos = o.transform.localPosition;
                pos.x -= distance;
                o.transform.localPosition = pos;
                if (pos.x < despawnX)
                    SetActive(o, false);
            }

            distanceToNextWave -= distance;
            if (distanceToNextWave <= 0) {
                SpawnWave();
                distanceToNextWave = UnityEngine.Random.Range(minSpacing, maxSpacing);
            }
        }

        // True if an obstacle in that lane is closer than hitDistance (on X) to the given position
        public bool Hits(int lane, float x, float hitDistance)
        {
            foreach (Obstacle o in pool) {
                if (o.active && o.lane == lane && Mathf.Abs(o.transform.localPosition.x - x) < hitDistance)
                    return true;
            }
            return false;
        }

        void SpawnWave()
        {
            int max = Mathf.Clamp(maxPerWave, 1, lanes.Count - 1);
            int count = UnityEngine.Random.Range(1, max + 1);

            // Random distinct lanes
            laneOrder.Clear();
            for (int i = 0; i < lanes.Count; i++)
                laneOrder.Add(i);
            for (int i = laneOrder.Count - 1; i > 0; i--) {
                int j = UnityEngine.Random.Range(0, i + 1);
                (laneOrder[i], laneOrder[j]) = (laneOrder[j], laneOrder[i]);
            }

            for (int i = 0; i < count; i++) {
                Obstacle o = GetFree(PickSource());
                if (o == null) return;
                Place(o, laneOrder[i]);
            }
        }

        Source PickSource()
        {
            if (sources.Count == 0) return new Source { objectId = "" };
            return sources[UnityEngine.Random.Range(0, sources.Count)];
        }

        // Reuses an inactive obstacle of that source, or loads a new one.
        // When the pool is full, an inactive obstacle of another source is removed to make room
        Obstacle GetFree(Source source)
        {
            string key = source.Key;
            Obstacle free = pool.Find(o => !o.active && o.key == key);
            if (free != null)
                return free;

            if (pool.Count >= maxPoolSize) {
                Obstacle unused = pool.Find(o => !o.active);
                if (unused == null)
                    return null;
                pool.Remove(unused);
                UnityEngine.Object.Destroy(unused.transform.gameObject);
            }
            return Create(source);
        }

        Obstacle Create(Source source)
        {
            CharacterManager character = null;
            Transform content;
            string name;
            if (source.characterId != null) {
                character = stage.SpawnCharacter(source.characterId, Vector3.zero);
                content = character.transform;
                name = "Obstacle_character_" + source.characterId;
            } else if (!string.IsNullOrEmpty(source.objectId)) {
                content = stage.SpawnObject(source.objectId, Vector3.zero).transform;
                name = "Obstacle_object_" + source.objectId;
            } else {
                content = stage.CreatePlaceholder("Placeholder", placeholderColor, Vector3.zero, Vector3.one).transform;
                name = "Obstacle_placeholder";
            }

            // Parent used as pivot: placing it on the lane Y leaves the obstacle standing on the lane
            Transform pivot = BottomPivot.Wrap(content, name);
            SortingGroup sorting = pivot.gameObject.AddComponent<SortingGroup>();

            Obstacle o = new Obstacle {
                key = source.Key,
                transform = pivot,
                content = content,
                character = character,
                animName = source.animName,
                sorting = sorting,
                baseScale = scale
            };
            SetActive(o, false);
            pool.Add(o);
            return o;
        }

        // Runs once, on the first frame the obstacle is active (outside the view),
        // so the bounds include the pose of its animator
        static void SetPivotAtBottom(Obstacle o)
        {
            o.pivotReady = true;
            BottomPivot.Align(o.transform, o.content);
        }

        void Place(Obstacle o, int lane)
        {
            o.lane = lane;
            o.transform.localPosition = new Vector3(spawnX + lane * laneOffsetX, lanes.GetY(lane) + offsetY, 0);
            float s = o.baseScale * lanes.GetScale(lane);
            bool flip = o.character != null && charactersFaceLeft;
            o.transform.localScale = new Vector3(flip ? -s : s, s, s);
            o.sorting.sortingOrder = lanes.GetSortingOrder(lane) - 1; // behind the player of the same lane
            SetActive(o, true);
            // Activating a GameObject restarts its animator: the action is played every time
            if (o.character != null)
                o.character.SetAnim(o.animName);
        }

        static void SetActive(Obstacle o, bool active)
        {
            o.active = active;
            o.transform.gameObject.SetActive(active);
        }
    }
}
