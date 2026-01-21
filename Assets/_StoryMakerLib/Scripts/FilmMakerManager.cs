using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yaguar.StoryMaker.Editor;

namespace Yaguar.StoryMaker.Editor
{
    public class FilmMakerManager : MonoBehaviour
    {
        [field:SerializeField] public states State { private set; get;}
        public enum states
        {
            STOPPED,
            PLAYING
        }
        [SerializeField] GameObject panel;
        [SerializeField] Button nextButton;
        [SerializeField] Button prevButton;
        [SerializeField] Button playButton;
        [SerializeField] Button deleteButton;

        [SerializeField] private TimelineInSceneUI timelineInSceneUI;
        [SerializeField] private Timeline timeline;
        [SerializeField] bool isEditing;

        public event Action<Action<bool>> DeleteDialog;
        public event Action OnMaxFrames;

        static FilmMakerManager mInstance = null;
        public static FilmMakerManager Instance
        {
            get
            {
                return mInstance;
            }
        }
        void Awake()
        {
            if (!mInstance)
                mInstance = this;
            
            StoryMakerEvents.OnTimelinePlay += OnTimelinePlay;
            StoryMakerEvents.Restart += Restart;
        }
        private void OnDestroy()
        {
            StoryMakerEvents.OnTimelinePlay -= OnTimelinePlay;
            StoryMakerEvents.Restart -= Restart;
            DeleteDialog = null;
            OnMaxFrames = null;
        }

        public void Show(bool enable)
        {
            panel.SetActive(enable);
        }
        
        void Restart()
        {
            Debug.Log("Restart");
            timeline.Reset();
            timelineInSceneUI.RefreshField(1);
        }
        public void OnTimelinePlay(bool isOn)
        {
            if (isOn)
            {
                //Events.OnAddScore(1);
                if (isEditing)
                    StoryMakerEvents.OnSaveScene();
                State = states.PLAYING;
                timeline.InitPlaying();
                StoryMakerEvents.ReorderSceneObjectsInZ();
            }
            else
            {
                playButton.GetComponent<PlayButton>().SetButtons(false);
                State = states.STOPPED;
            }
        }
        private void Update()
        {
            if (State == states.STOPPED)
                return;

            timeline.OnUpdate();
        }



        public void SetOn(bool isOn)
        {
            if (isOn)
            {
                panel.SetActive(true);
                timelineInSceneUI.Init();
                SetButtons();
            }
            else
            {
                panel.SetActive(false);
            }

        }
        void SetButtons()
        {
            int total = ScenesManager.Instance.GetTotalScenes();
            if (ScenesManager.Instance.currentSceneId == 1)
                prevButton.interactable = false;
            else
                prevButton.interactable = true;

            if (total > 1 || !isEditing)
                playButton.interactable = true;
            // else
            // playButton.interactable = false;

            if (ScenesManager.Instance.currentSceneId >= total)
                nextButton.interactable = false;
            else
                nextButton.interactable = true;

            if (total > 1)
                deleteButton.interactable = true;
            else
                deleteButton.interactable = false;

            timelineInSceneUI.RefreshField(ScenesManager.Instance.currentSceneId);


        }
        public void New()
        {
            if (timeline.all.Count >= ScenesManager.Instance.MaxKeyframes)
            {
                OnMaxFrames();                
                return;
            }
            //Events.OnAddScore(2);
            StoryMakerEvents.OnSaveScene();
            //ScenesManager.Instance.OnSaveScene();
            ScenesManager.Instance.currentSceneId++;
            ScenesManager.Instance.AddNewScene(ScenesManager.Instance.currentSceneId);
            SetButtons();
            timeline.AddNewKeyframe();
            timeline.JumpTo(ScenesManager.Instance.currentSceneId);
        }
        public void Delete()
        {
            DeleteDialog(OnDelete);            
        }
        public void OnDelete(bool doIt)
        {
            if (doIt)
            {
                if (Scenario.Instance != null)
                {
                    Scenario.Instance.sceneObejctsManager.ResetScene();
                }

                ScenesManager.Instance.RemoveScene(ScenesManager.Instance.currentSceneId);

                if (ScenesManager.Instance.currentSceneId > 1)
                    ScenesManager.Instance.currentSceneId--;

                SetButtons();
                ScenesManager.Instance.AddSceneObjectsToScene(false);

                timeline.RemoveKeyframe();

                timeline.JumpTo(ScenesManager.Instance.currentSceneId);
            }
        }
        public void Next()
        {
            StoryMakerEvents.OnSaveScene();
            //ScenesManager.Instance.OnSaveScene();
            ScenesManager.Instance.currentSceneId++;
            SetScene(true);
            timeline.JumpTo(ScenesManager.Instance.currentSceneId);
        }
        public void Prev()
        {
            StoryMakerEvents.OnSaveScene();
            //ScenesManager.Instance.OnSaveScene();
            ScenesManager.Instance.currentSceneId--;
            SetScene(false);
            timeline.JumpTo(ScenesManager.Instance.currentSceneId);
        }
        void SetScene(bool next)
        {
            int total = ScenesManager.Instance.GetTotalScenes();
            int nextSceneid = ScenesManager.Instance.currentSceneId + 1;

            if (State == states.PLAYING && nextSceneid <= total)
            {
                int backgroundID = ScenesManager.Instance.GetBackground(ScenesManager.Instance.currentSceneId);
                int nextBackgroundID = ScenesManager.Instance.GetBackground(nextSceneid);
                print(ScenesManager.Instance.currentSceneId + " nextSceneid : " + nextSceneid + " backgroundID: " + backgroundID + " : " + nextBackgroundID);
                if (backgroundID == nextBackgroundID)
                {
                    float delay = timeline.keyframe_duration - (timeline.keyframe_duration / 3);
                    StartCoroutine(MoveAvatarsAfter(delay));
                }
            }

            if (ScenesManager.Instance.currentSceneId > total)
                ScenesManager.Instance.currentSceneId = total;
            else if (ScenesManager.Instance.currentSceneId <= 1)
                ScenesManager.Instance.currentSceneId = 1;

            SetButtons();

            ScenesManager.Instance.AddSceneObjectsToScene(next);
            StoryMakerEvents.ReorderSceneObjectsInZ();
        }
        IEnumerator MoveAvatarsAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (State == states.PLAYING)
                ScenesManager.Instance.GetActiveScene().MakeCharactersWalk(timeline.keyframe_duration - delay);
        }
        public void JumpTo(int keyframeID)
        {
            Debug.Log("#JumpTo");
            ScenesManager.Instance.currentSceneId = keyframeID;
            SetScene(true);
        }
    }
}
