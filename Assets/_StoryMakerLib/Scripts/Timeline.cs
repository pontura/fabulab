using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class Timeline : MonoBehaviour
    {
        public FilmMakerManager filmMakerUI;
        public KeyFrameUI keyframe;
        public Transform container;
        public List<KeyFrameUI> all;
        public KeyframeMarker keyFrameMarker;
        private float total_x_marker = 282;
        public float timer;
        public float totalTimer;
        public float keyframe_duration;

        void Start()
        {
            

            StoryMakerEvents.OnLoadFilm += Reset;
            StoryMakerEvents.ChangeSpeed += ChangeSpeed;

            StoryMakerEvents.ChangeSpeed(ScenesManager.Instance.currentFilmData.speed);

            //Invoke(nameof(Reset), Time.deltaTime * 3);
        }

        private void OnDestroy()
        {
            StoryMakerEvents.OnLoadFilm -= Reset;
            StoryMakerEvents.ChangeSpeed -= ChangeSpeed;
        }
        void ChangeSpeed(int speed)
        {
            filmMakerUI.OnTimelinePlay(false);
            keyframe_duration = ScenesManager.Instance.Keyframe_duration - (speed);
        }
        public void Reset()
        {
            if (ScenesManager.Instance.currentFilmData != null)
                StoryMakerEvents.ChangeSpeed(ScenesManager.Instance.currentFilmData.speed);

            keyFrameMarker.transform.localPosition = Vector3.zero;
            totalTimer = 0;
            timer = 0;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            all.Clear();
            RefreshKeyframes();
            filmMakerUI.JumpTo(1);
        }
        void RefreshKeyframes()
        {
            for (int a = 0; a < ScenesManager.Instance.scenes.Count; a++)
                AddNewKeyframe();
        }
        public void AddNewKeyframe()
        {
            KeyFrameUI kf = Instantiate(keyframe);
            all.Add(kf);
            kf.transform.SetParent(container);
            kf.transform.localScale = Vector2.one;
            kf.transform.localPosition = Vector2.zero;
            Debug.Log(kf.transform.localPosition);
        }
        public void RemoveKeyframe()
        {
            all.RemoveAt(0);
            KeyFrameUI kf = container.GetComponentInChildren<KeyFrameUI>();
            Destroy(kf.gameObject);
        }
        public void InitPlaying()
        {
            Scenario.Instance.ResetScenario();
            JumpTo(1);
            filmMakerUI.JumpTo(1);
        }
        public void OnUpdate()
        {
            timer += Time.deltaTime;
            SetPosition();
        }
        int activeAnimatedKeyframeID;
        void SetPosition()
        {
            Vector2 pos = keyFrameMarker.transform.localPosition;
            pos.x = timer * total_x_marker / totalTimer;
            if (timer >= totalTimer)
            {
                StoryMakerEvents.OnTimelinePlay(false);
                pos.x = total_x_marker;
                StoryMakerEvents.OnMovieOver();
                print("____________termino");
                //filmMakerUI.JumpTo(1);
            }
            else
                CheckToNextAnimatedKeyframe();
            keyFrameMarker.transform.localPosition = pos;
        }
        public void JumpTo(int keyframeId)
        {
            activeAnimatedKeyframeID = keyframeId;
            totalTimer = (all.Count * keyframe_duration);
            timer = ((keyframeId - 1) * keyframe_duration);
            SetPosition();
        }
        void CheckToNextAnimatedKeyframe()
        {
            float nextTimer = ((activeAnimatedKeyframeID) * keyframe_duration);
            if (timer >= nextTimer)
            {
                activeAnimatedKeyframeID++;
                filmMakerUI.JumpTo(activeAnimatedKeyframeID);
            }
        }
    }
}
