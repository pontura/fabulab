using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class TimelineFabulab : Timeline
    {
    

        public float offset = 10;
        protected override void Start() {

            base.Start();
            Invoke(nameof(SetTotalMarkers), Time.deltaTime * 3);
        }

        void SetTotalMarkers() {
            RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            total_x_marker = canvasRect.rect.width - offset;
        }
        protected override void ChangeSpeed(int speed)
        {
            filmMakerUI.OnTimelinePlay(false);
            if(ScenesManagerFabulab.Instance.GetActiveScene()!=null)
                keyframe_duration = Mathf.Max(ScenesManagerFabulab.Instance.GetActiveScene().duration,0.5f) - (speed);
            else
                keyframe_duration = ScenesManagerFabulab.Instance.Keyframe_default_duration - (speed);
        }
        public override void Reset()
        {
            activeAnimatedKeyframeID = 1;
            if (ScenesManagerFabulab.Instance.currentFilmData != null)
                StoryMakerEvents.ChangeSpeed(ScenesManagerFabulab.Instance.currentFilmData.speed);

            keyFrameMarker.transform.localPosition = Vector3.zero;
            timer = 0;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            all.Clear();

            Scenario.Instance.StartCoroutine(RefreshKeyframesC());
        }
        IEnumerator RefreshKeyframesC()
        {
            int a = 0;
            foreach (SceneDataFabulab s in ScenesManagerFabulab.Instance.Scenes)
            {
                a++;
                AddNewKeyframe(s.duration);
                yield return new WaitForSeconds(Time.deltaTime * 10);
                filmMakerUI.JumpTo(a);
            }
            filmMakerUI.JumpTo(1);
            UpdateKeyframes();

            Events.OnLoading(false);
        }
        public override float OnChangeDuration(float value) {
            float duration = Mathf.Lerp(min_speed, max_speed, value);
            all[activeAnimatedKeyframeID - 1].SetDuration(duration);
            if (ScenesManagerFabulab.Instance.GetActiveScene() != null)
                ScenesManagerFabulab.Instance.GetActiveScene().duration = duration;
            UpdateKeyframes();
            ForceMarkerPosition();
            return duration;
        }

        public override void SetJump(int id) {
            StoryMakerEvents.OnSaveScene();
            ScenesManagerFabulab.Instance.currentSceneId = id;
            base.SetJump(id);
        }
    }
}
