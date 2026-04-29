using System.Collections.Generic;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class Timeline : MonoBehaviour
    {
        [SerializeField] DurationBtn durationBtn;
        [SerializeField] protected float min_speed = 0.5f;
        [SerializeField] protected float max_speed = 4;

        public FilmMakerManager filmMakerUI;
        public KeyFrameUI keyframe;
        public Transform container;
        public List<KeyFrameUI> all;
        public KeyframeMarker keyFrameMarker;
        public float total_x_marker = 282;
        public float timer;
        public float totalTimer
        {
            get 
            {
                float t = 0;
                foreach(KeyFrameUI k in all)
                    t+= k.duration;
                return t;
            }
        }
        public float keyframe_duration = 2;

        protected virtual void Start()
        {
            StoryMakerEvents.OnLoadFilm += Reset;
            StoryMakerEvents.OnStartNewStory += Reset;
            StoryMakerEvents.ChangeSpeed += ChangeSpeed;
            StoryMakerEvents.ChangeSpeed(ScenesManager.Instance.currentFilmData.speed);
        }
        private void OnEnable() {

            if (all.Count == 0)
            {
                Reset();
            } else
                UpdateKeyframes();

            float duration = (keyframe_duration - min_speed) / (max_speed - min_speed);
            durationBtn.Init(this, duration);
        }
        protected void OnDestroy()
        {
            StoryMakerEvents.OnLoadFilm -= Reset;
            StoryMakerEvents.OnStartNewStory -= Reset;
            StoryMakerEvents.ChangeSpeed -= ChangeSpeed;
        }
        protected virtual void ChangeSpeed(int speed)
        {
            filmMakerUI.OnTimelinePlay(false);
        }
        public virtual void Reset()
        {
            activeAnimatedKeyframeID = 1;
            if (ScenesManager.Instance.currentFilmData != null)
                StoryMakerEvents.ChangeSpeed(ScenesManager.Instance.currentFilmData.speed);

            keyFrameMarker.transform.localPosition = Vector3.zero;
            timer = 0;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            all.Clear();
            RefreshKeyframes();
            filmMakerUI.JumpTo(1);
        }
        protected virtual void RefreshKeyframes()
        {
            //for (int a = 0; a < ScenesManager.Instance.Scenes.Count; a++)
            //    AddNewKeyframe();
        }
        public void AddNewKeyframe(float duration = 0)
        {
            if (duration == 0) duration = keyframe_duration;
            KeyFrameUI kf = Instantiate(keyframe);
            kf.SetDuration(duration);
            kf.Init(this);
            all.Add(kf);
            kf.transform.SetParent(container);
            kf.transform.localScale = Vector2.one;
            kf.transform.localPosition = Vector2.zero;
            //Debug.Log(kf.transform.localPosition);
            UpdateKeyframes();
            kf.UpdateScreenshot();
        }
        protected void UpdateKeyframes()
        {
            int id = 0;
            float totalDuration = 0;
            foreach (KeyFrameUI kf in all)
                totalDuration += kf.duration;

            foreach (KeyFrameUI kf in all)
            {
                kf.SetID(id+1);
                kf.SetSize(totalDuration);
                kf.SetColor(id %2 == 0);
                if(id+1 == activeAnimatedKeyframeID)
                    kf.SetSelected(true);
                else if(kf.selected)
                    kf.SetSelected(false);
                id++;
            }
            print(all.Count + " activeAnimatedKeyframeID_ " + activeAnimatedKeyframeID);
            if(all.Count>0 && activeAnimatedKeyframeID<all.Count)
                all[activeAnimatedKeyframeID - 1].UpdateScreenshot();
        }
        public void RemoveKeyframe()
        {
            print("RemoveKeyframe");
            KeyFrameUI kf = all[activeAnimatedKeyframeID - 1];
            Destroy(kf.gameObject);
            all.Remove(kf);
            UpdateKeyframes();
        }
        public void InitPlaying()
        {
            if (!StoryMakerEvents.isEditing)
            {
                Scenario.Instance.ResetScenario();

                JumpTo(ScenesManager.Instance.currentSceneId);
                filmMakerUI.JumpTo(ScenesManager.Instance.currentSceneId);
            }
        }
        public void OnUpdate()
        {
            timer += Time.deltaTime;
            SetPosition();
        }
        protected int activeAnimatedKeyframeID = 1;
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

        public virtual void SetJump(int id) {
            StoryMakerEvents.OnTimelineSetJump(activeAnimatedKeyframeID);
            JumpTo(id);
        }

        public void JumpTo(int keyframeId)
        {
            print("JumpTo " + keyframeId);
            activeAnimatedKeyframeID = keyframeId;
            timer = CalculateTimer();
            SetPosition();
            UpdateKeyframes();
            ForceMarkerPosition();

            float sliderValue = (all[activeAnimatedKeyframeID - 1].duration - min_speed) / (max_speed - min_speed);

            durationBtn?.SetValue(sliderValue);
        }
        protected void ForceMarkerPosition()
        {
            float timer_pos = CalculateTimer();
            Vector2 pos = keyFrameMarker.transform.localPosition;
            pos.x = timer_pos * (total_x_marker / totalTimer);
            keyFrameMarker.transform.localPosition = pos;
        }
        float CalculateTimer()
        {
            float timer_pos = 0;
            for (int a = 0; a < activeAnimatedKeyframeID - 1; a++)
            {
                if (a < all.Count)
                    timer_pos += all[a].duration;
            }
            print("CalculateTimer timer_pos" + timer_pos + " activeAnimatedKeyframeID:" + activeAnimatedKeyframeID);
            return timer_pos;
        }
        void CheckToNextAnimatedKeyframe()
        {
            float nextTimer = 0;
            for (int a = 0; a < activeAnimatedKeyframeID; a++)
            {
                if (a < all.Count)
                    nextTimer += all[a].duration;
            }
            if (timer >= nextTimer)
            {
                activeAnimatedKeyframeID++;
                filmMakerUI.JumpTo(activeAnimatedKeyframeID);
            }
        }
        public virtual float OnChangeDuration(float value)
        {
            float duration = Mathf.Lerp(min_speed, max_speed, value);
            all[activeAnimatedKeyframeID-1].SetDuration(duration);
            UpdateKeyframes();
            ForceMarkerPosition();
            return duration;
        }
        public virtual void OnChangeLight(float value)
        {
            print("change light " + value);
        }
    }
}
