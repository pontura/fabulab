using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;
using static System.Net.Mime.MediaTypeNames;

namespace Yaguar.StoryMaker.Editor
{
    public class FilmMakerManagerFabulab : FilmMakerManager
    {
        [SerializeField] protected Button newButton;
      //  [SerializeField] protected Button hamburguerButton;
       // [SerializeField] protected HorizontalLayoutGroup buttonsGroup;
        [SerializeField] protected float delayFactor;
        [SerializeField] Toggle toggleTransition;

        protected override void Awake() {
            base.Awake();
            StoryMakerEvents.EnableStoryEdition += EnableStoryEdition;
            StoryMakerEvents.OnTimelineSetJump += OnTimelineSetJump;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            StoryMakerEvents.EnableStoryEdition -= EnableStoryEdition;
            StoryMakerEvents.OnTimelineSetJump -= OnTimelineSetJump;
        }

        void EnableStoryEdition(bool enable) {
            deleteButton.gameObject.SetActive(enable);
            newButton.gameObject.SetActive(enable);
            //hamburguerButton.gameObject.SetActive(enable);
            toggleTransition.gameObject.SetActive(enable);
          //  buttonsGroup.spacing = enable ? 5 : 10;
        }

        protected override void SetButtons()
        {
            int total = ScenesManagerFabulab.Instance.Scenes.Count;
            if (ScenesManagerFabulab.Instance.currentSceneId == 1)
                prevButton.interactable = false;
            else
                prevButton.interactable = true;

            if (total > 1 || !isEditing)
                playButton.interactable = true;
            // else
            // playButton.interactable = false;

            if (ScenesManagerFabulab.Instance.currentSceneId >= total)
                nextButton.interactable = false;
            else
                nextButton.interactable = true;

            if (total > 1)
                deleteButton.interactable = true;
            else
                deleteButton.interactable = false;

            timelineInSceneUI.RefreshField(ScenesManagerFabulab.Instance.currentSceneId);


        }
        public override void New()
        {

            if (timeline.all.Count >= ScenesManagerFabulab.Instance.MaxKeyframes)
            {
                RunOnMaxFramesEvent();                
                return;
            }
            //Events.OnAddScore(2);
            StoryMakerEvents.OnSaveScene();
            //ScenesManagerFabulab.Instance.OnSaveScene();
            ScenesManagerFabulab.Instance.currentSceneId++;
            ScenesManagerFabulab.Instance.AddNewScene(ScenesManagerFabulab.Instance.currentSceneId);
            SetButtons();
            timeline.AddNewKeyframe();
            timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
        }
        
        public override void OnDelete(bool doIt)
        {
            if (doIt)
            {
                if (Scenario.Instance != null)
                {
                    Scenario.Instance.sceneObejctsManager.ResetScene();
                }

                ScenesManagerFabulab.Instance.RemoveScene(ScenesManagerFabulab.Instance.currentSceneId);

                if (ScenesManagerFabulab.Instance.currentSceneId > 1)
                    ScenesManagerFabulab.Instance.currentSceneId--;

                SetButtons();
                ScenesManagerFabulab.Instance.AddSceneObjectsToScene(false);

                timeline.RemoveKeyframe();

                timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
            }
        }
        public override void Next()
        {
            StoryMakerEvents.OnSaveScene();
            //ScenesManagerFabulab.Instance.OnSaveScene();
            ScenesManagerFabulab.Instance.currentSceneId++;
            SetScene(true);
            timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
        }
        public override void Prev()
        {
            StoryMakerEvents.OnSaveScene();
            //ScenesManagerFabulab.Instance.OnSaveScene();
            ScenesManagerFabulab.Instance.currentSceneId--;
            SetScene(false);
            timeline.JumpTo(ScenesManagerFabulab.Instance.currentSceneId);
        }

        void OnTimelineSetJump() {
            SetButtons();

            if (ScenesManagerFabulab.Instance.GetActiveScene() != null)
                toggleTransition.isOn = ScenesManagerFabulab.Instance.GetActiveScene().transition;

            ScenesManagerFabulab.Instance.AddSceneObjectsToScene(false);
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        protected override void SetScene(bool next)
        {
            int total = ScenesManagerFabulab.Instance.Scenes.Count;
            int nextSceneid = ScenesManagerFabulab.Instance.currentSceneId + 1;

            if (ScenesManagerFabulab.Instance.GetActiveScene() != null)
                timeline.keyframe_duration = Mathf.Max(ScenesManagerFabulab.Instance.GetActiveScene().duration, 0.5f);

            if (State == states.PLAYING && nextSceneid <= total)
            {
                string backgroundID = ScenesManagerFabulab.Instance.GetBackground(ScenesManagerFabulab.Instance.currentSceneId);
                string nextBackgroundID = ScenesManagerFabulab.Instance.GetBackground(nextSceneid);
                print(ScenesManagerFabulab.Instance.currentSceneId + " nextSceneid : " + nextSceneid + " backgroundID: " + backgroundID + " : " + nextBackgroundID);
                if (backgroundID == nextBackgroundID)
                {
                    float delay = timeline.keyframe_duration * delayFactor;
                    Debug.Log("# Delay: " + delay);
                    StartCoroutine(MoveAvatarsAfter(delay));
                }
            } else if(State == states.STOPPED) {
                Invoke(nameof(SetPaused), Time.deltaTime);
            }

            if (ScenesManagerFabulab.Instance.currentSceneId > total)
                ScenesManagerFabulab.Instance.currentSceneId = total;
            else if (ScenesManagerFabulab.Instance.currentSceneId <= 1)
                ScenesManagerFabulab.Instance.currentSceneId = 1;

            SetButtons();

            if(ScenesManagerFabulab.Instance.GetActiveScene()!=null)
                toggleTransition.isOn = ScenesManagerFabulab.Instance.GetActiveScene().transition;

            ScenesManagerFabulab.Instance.AddSceneObjectsToScene(next);
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }

        void SetPaused() {
            StoryMakerEvents.OnMoviePaused();
        }

        protected override IEnumerator MoveAvatarsAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (State == states.PLAYING)
                ScenesManagerFabulab.Instance.GetActiveScene().MoveElements(timeline.keyframe_duration - delay);
        }
        public override void JumpTo(int keyframeID)
        {
            Debug.Log("#JumpTo");
            ScenesManagerFabulab.Instance.currentSceneId = keyframeID;
            SetScene(true);
        }

        public void OnTransitionChange() {
            if(ScenesManagerFabulab.Instance!=null && ScenesManagerFabulab.Instance.GetActiveScene()!=null)
                ScenesManagerFabulab.Instance.GetActiveScene().transition = toggleTransition.isOn;
        }
    }
}
