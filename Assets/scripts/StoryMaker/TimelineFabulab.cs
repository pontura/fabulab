using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Yaguar.StoryMaker.Editor
{
    public class TimelineFabulab : Timeline
    {
        VerticalLayoutGroup vlg;
        protected override void Start() {

            base.Start();
            vlg = GetComponentInParent<VerticalLayoutGroup>();
            Invoke(nameof(SetTotalMarkers), Time.deltaTime * 3);
        }

        void SetTotalMarkers() {
            total_x_marker = (vlg.transform as RectTransform).sizeDelta.x - vlg.padding.left - vlg.padding.right;
        }
        protected override void ChangeSpeed(int speed)
        {
            filmMakerUI.OnTimelinePlay(false);
            keyframe_duration = ScenesManagerFabulab.Instance.Keyframe_duration - (speed);
        }
        public override void Reset()
        {
            if (ScenesManagerFabulab.Instance.currentFilmData != null)
                StoryMakerEvents.ChangeSpeed(ScenesManagerFabulab.Instance.currentFilmData.speed);

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
        protected override void RefreshKeyframes()
        {
            for (int a = 0; a < ScenesManagerFabulab.Instance.Scenes.Count; a++)
                AddNewKeyframe();
        }        
    }
}
