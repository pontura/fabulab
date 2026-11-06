using BoardItems.BoardData;
using BoardItems.Characters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniGames.Runner
{
    // Runner on 4 lanes: the character runs in place, changes lane with swipe up/down
    // and has to avoid the obstacles. Score = distance traveled
    public class RunnerMiniGame : MiniGame
    {
        [SerializeField] RunnerLanes lanes = new RunnerLanes();
        [SerializeField] SwipeDetector swipe = new SwipeDetector();
        [SerializeField] RunnerObstacles obstacles = new RunnerObstacles();
        [SerializeField] RunnerBackground background = new RunnerBackground();
        [SerializeField] RunnerParallax parallax = new RunnerParallax();
        [SerializeField] float characterX = -2;
        [SerializeField] int startLane = 1;
        [SerializeField] float laneChangeDuration = 0.18f;

        [Header("Speed (world units per second)")]
        [SerializeField] float startSpeed = 6;
        [SerializeField] float maxSpeed = 14;
        [SerializeField] float acceleration = 0.2f;
        [Tooltip("Seconds walking before the character starts to run")]
        [SerializeField] float runAfterSeconds = 4;

        [Header("Collision and score")]
        [Tooltip("Horizontal distance between the character and an obstacle of the same lane that counts as a hit")]
        [SerializeField] float hitDistance = 0.8f;
        [SerializeField] float scorePerUnit = 1;

        [Header("Lane guides")]
        [SerializeField] bool showLaneGuides = true;
        [Tooltip("Vertical offset of the guides relative to the lane Y (the character pivot)")]
        [SerializeField] float guideOffsetY = 0;
        [SerializeField] Color guideColorA = new Color(1, 1, 1, 0.35f);
        [SerializeField] Color guideColorB = new Color(1, 1, 1, 0.15f);

        CharacterManager character;
        SortingGroup characterSorting;
        float baseScale;

        int lane;
        float fromY, toY;
        float fromScale, toScale;
        float moveTime = 1; // normalized progress of the lane change (1 = done)

        float speed;
        float distance;
        bool running;
        float playTime;

        public override void Play()
        {
            stage.Clear();
            obstacles.Clear();
            parallax.Clear();
            background.Clear();
            background.Init(stage);
            if (showLaneGuides)
                CreateLaneGuides();
            parallax.Init(stage);

            character = stage.SpawnCharacter(characterId, Vector3.zero);
            // Starts walking and runs after runAfterSeconds
            character.SetAnim(Data.Instance.characterAnimsManager.defaultWalk.name);
            running = false;
            playTime = 0;
            characterSorting = character.GetComponent<SortingGroup>();
            if (characterSorting == null)
                characterSorting = character.gameObject.AddComponent<SortingGroup>();
            baseScale = character.transform.localScale.x;

            obstacles.Init(stage, lanes, GetObstacleIds());

            swipe.Reset();
            SetLane(lanes.Clamp(startLane), true);
            speed = startSpeed;
            distance = 0;
            ResetScore();
            IsPlaying = true;
        }

        public override void Stop()
        {
            base.Stop();
            character = null;
            characterSorting = null;
        }

        void Update()
        {
            if (!IsPlaying || character == null)
                return;

            switch (swipe.Tick()) {
                case SwipeDirection.Up: SetLane(lane + 1, false); break;
                case SwipeDirection.Down: SetLane(lane - 1, false); break;
            }
            UpdateLaneChange();

            speed = Mathf.Min(maxSpeed, speed + acceleration * Time.deltaTime);
            playTime += Time.deltaTime;
            if (!running && playTime >= runAfterSeconds) {
                running = true;
                character.SetAnim(Data.Instance.characterAnimsManager.defaultRun.name);
            }
            float step = speed * Time.deltaTime;
            distance += step;
            obstacles.Tick(step);
            background.Tick(step);
            parallax.Tick(step);
            SetScore(Mathf.FloorToInt(distance * scorePerUnit));

            if (obstacles.Hits(GetCurrentLane(), characterX, hitDistance))
                OnHit();
        }

        void OnHit()
        {
            character.SetAnim(Data.Instance.characterAnimsManager.defaultIdle.name);
            Finish();
        }

        // All the generic objects of the logged user (empty = placeholder obstacles)
        List<string> GetObstacleIds()
        {
            List<string> ids = new List<string>();
            BoardItems.SObjectsData sObjectsData = Data.Instance.sObjectsData;
            foreach (PropMetaData md in sObjectsData.GetUserMetadataByType(SObjectData.types.generic)) {
                if (sObjectsData.data.Exists(x => x.id == md.id))
                    ids.Add(md.id);
            }
            return ids;
        }

        // Lane where the character is now (the closest one, also while changing lane)
        int GetCurrentLane()
        {
            float y = character.transform.localPosition.y;
            int closest = 0;
            for (int i = 1; i < lanes.Count; i++) {
                if (Mathf.Abs(lanes.GetY(i) - y) < Mathf.Abs(lanes.GetY(closest) - y))
                    closest = i;
            }
            return closest;
        }

        void SetLane(int newLane, bool instant)
        {
            newLane = lanes.Clamp(newLane);
            if (newLane == lane && !instant)
                return;

            lane = newLane;
            // Starts from the current state, so a swipe during a lane change retargets smoothly
            fromY = character.transform.localPosition.y;
            fromScale = character.transform.localScale.x;
            toY = lanes.GetY(lane);
            toScale = baseScale * lanes.GetScale(lane);
            moveTime = instant ? 1 : 0;
            if (instant)
                ApplyLaneChange(1);
        }

        void UpdateLaneChange()
        {
            if (moveTime >= 1)
                return;
            moveTime = laneChangeDuration > 0 ? Mathf.Min(1, moveTime + Time.deltaTime / laneChangeDuration) : 1;
            ApplyLaneChange(Mathf.SmoothStep(0, 1, moveTime));
        }

        void ApplyLaneChange(float t)
        {
            Transform tr = character.transform;
            tr.localPosition = new Vector3(characterX, Mathf.Lerp(fromY, toY, t), 0);
            tr.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, t);
            // Changes the drawing order halfway, when the character is between both lanes
            if (t >= 0.5f || moveTime >= 1)
                characterSorting.sortingOrder = lanes.GetSortingOrder(lane);
        }

        void CreateLaneGuides()
        {
            float width = stage.CameraSize * 2 + 2;
            float height = lanes.LaneHeight * 0.9f;
            for (int i = 0; i < lanes.Count; i++) {
                SpriteRenderer sr = stage.CreatePlaceholder(
                    "LaneGuide_" + i,
                    i % 2 == 0 ? guideColorA : guideColorB,
                    new Vector3(0, lanes.GetY(i) + guideOffsetY, 1),
                    new Vector3(width, height, 1));
                sr.sortingOrder = -100;
            }
        }
    }
}
