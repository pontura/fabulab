using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UI.MainApp;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Yaguar.StoryMaker.Editor
{
    public class TimelineFabulab : Timeline
    {
        public ShotButtons shotButtons;
        [SerializeField] GhostImage ghostImage;

        public float offset = 10;
        protected override void Start() {
            print("OnStopDraw Start");      
            StoryMakerEvents.OnStopDraw += OnStopDraw;
            base.Start();
            Invoke(nameof(SetTotalMarkers), Time.deltaTime * 3);
        }
        public override void OnDestroyed()
        {            
            print("OnStopDraw OnDestroyed");      
            StoryMakerEvents.OnStopDraw -= OnStopDraw;
        }

        private void OnStopDraw(SceneObject so)
        {
            SOData soData = so.GetData();
            print("OnStopDraw" + soData.itemName);      
            SceneElement sceneElement = ScenesManagerFabulab.Instance.GetSOInScene(ScenesManagerFabulab.Instance.currentSceneId-2, soData.id);
            if(sceneElement == null) 
            {
                Debug.Log("OnStopDraw no sceneElement");  return;
            }
            V3 v3 = sceneElement.data.pos;
            Vector2 pos = new Vector2(v3.x, v3.y);
            float diff = Vector2.Distance(pos, so.gameObject.transform.localPosition);
            print("OnStopDraw diff: "  + diff); 
            if(diff<3)
            {
                soData.pos = v3;
                print("Snap" + soData.itemName + " old pos: " + pos + "new pos: " + v3.ToVector3());     
                so.gameObject.transform.localPosition = v3.ToVector3();
            }
        }

        public override void OnDisabled()
        {
            ghostImage.Show(false);
        }
        public override  void OnEnabled()
        {
            if(all.Count>1)
                ghostImage.Show(true);
        }
        protected override void Rewind() {
            ScenesManagerFabulab.Instance.currentSceneId = 1;
            base.SetJump(1);
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
            ghostImage.Reset();
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

            if(filmMakerUI.isEditing)
                Scenario.Instance.StartCoroutine(RefreshKeyframesC());
            else
            {
                int a = 0;
                foreach (SceneDataFabulab s in ScenesManagerFabulab.Instance.Scenes)
                {
                    a++;
                    AddNewKeyframe(s.duration);
                }
                filmMakerUI.JumpTo(1);
                UpdateKeyframes();
                Events.OnLoading(false);
            }
            OnStop();
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
        public override void OnJumpDone()
        {
            base.OnJumpDone();
            print("ScenesManagerFabulab.Instance.currentSceneId: " + ScenesManagerFabulab.Instance.currentSceneId);
            if(ScenesManagerFabulab.Instance.currentSceneId<2) { ghostImage.Show(false); return; }
            int prevSceneID = ScenesManagerFabulab.Instance.currentSceneId-2;
            
            ghostImage.Show(true);
            ghostImage.Init(all[prevSceneID].sprite);
        }
        
        public override void OnMarkerUpdated(float timer_pos)
        {            
            shotButtons.OnMarkerUpdated(timer_pos);
        }
        public override void OnPlay() { shotButtons.Show(false); }
        public override void OnStop() { shotButtons.Show(true); }
        
        public virtual void UpdateDrawDone() {}
    }
}
